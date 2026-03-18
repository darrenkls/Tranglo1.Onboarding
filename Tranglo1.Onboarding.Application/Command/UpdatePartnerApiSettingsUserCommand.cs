using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities.PartnerManagement;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.Managers;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.Command
{
    //[Permission(PermissionGroupCode.PartnerAPISetting, UACAction.Edit)]
    internal class UpdatePartnerApiSettingsUserCommand : BaseCommand<Result<PartnerAPISettingsUserUpdateInputDTO>>
    {
        public long PartnerCode { get; set; }
        public long PartnerSubscriptionCode { get; set; }
        public UpdatePartnerAPISettingsUser Staging { get; set; }
        public UpdatePartnerAPISettingsUser Production { get; set; }
        public PartnerAPISettingsUserUpdateInputDTO inputDTO { get; set; }

        public override Task<string> GetAuditLogAsync(Result<PartnerAPISettingsUserUpdateInputDTO> result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Updated User API Settings for PartnerSubscriptionCode: [{this.PartnerSubscriptionCode}]";
                return Task.FromResult(_description);
            }
            return Task.FromResult<string>(null);
        }
    }
    internal class UpdatePartnerApiSettingsUserCommandHandler : IRequestHandler<UpdatePartnerApiSettingsUserCommand, Result<PartnerAPISettingsUserUpdateInputDTO>>
    {
        private readonly PartnerService _partnerService;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IntegrationManager _integrationManager;
        private readonly ILogger<UpdatePartnerApiSettingsUserCommandHandler> _logger;
        private static IConfiguration _config;
        private static string strCryptoKey;
        private static byte[] IV;

        public UpdatePartnerApiSettingsUserCommandHandler(PartnerService partnerService, IPartnerRepository partnerRepository, ILogger<UpdatePartnerApiSettingsUserCommandHandler> logger,
            IntegrationManager integrationManager, IConfiguration config)
        {
            _partnerService = partnerService;
            _partnerRepository = partnerRepository;
            _integrationManager = integrationManager;
            _logger = logger;
            _config = config;
        }
        public async Task<Result<PartnerAPISettingsUserUpdateInputDTO>> Handle(UpdatePartnerApiSettingsUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var password = Encrypt(request.Staging.Password);
                var partnerRegistration = await _partnerService.GetPartnerRegistrationByCodeAsync(request.PartnerCode);
                var partnerSubscription = await _partnerRepository.GetSubscriptionAsync(request.PartnerSubscriptionCode);
                PartnerAPISetting stgPartnerAPISetting = _partnerService.GetPartnerAPISettingByCodeAsync(request.Staging.PartnerAPISettingId).Result;
                stgPartnerAPISetting.PartnerCode = request.PartnerCode;
                stgPartnerAPISetting.PartnerSubscriptionCode = request.PartnerSubscriptionCode;
                stgPartnerAPISetting.APIUserId = request.Staging.APIUserId;
                stgPartnerAPISetting.Password = password;
                stgPartnerAPISetting.SecretKey = request.Staging.SecretKey;

                var result = await _partnerService.UpdatePartnerAPISettingAsync(stgPartnerAPISetting);

                await _integrationManager.UpdateAPISettingsAsync(partnerSubscription.RspStagingId, partnerRegistration.Email,
                    request.Staging.APIUserId, password, request.Staging.SecretKey);

                return Result.Success(request.inputDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[UpdatePartnerApiSettingsUserCommand] {ex.Message}");
            }
            return Result.Failure<PartnerAPISettingsUserUpdateInputDTO>(
                            $"Update user partner settings failed for PartnerCode: {request.PartnerCode} and PartnerSubscriptionCode: {request.PartnerSubscriptionCode}."
                        );
        }

        private static string GetHash(string plainText)
        {
            byte[] plainBytes = null;
            System.Security.Cryptography.MD5CryptoServiceProvider hashEngine = null;
            byte[] hashBytes = null;
            string hashText = null;

            plainBytes = Encoding.UTF8.GetBytes(plainText);

            hashEngine = new System.Security.Cryptography.MD5CryptoServiceProvider();
            hashBytes = hashEngine.ComputeHash(plainBytes);
            hashText = BitConverter.ToString(hashBytes).Replace("-", "");
            return hashText;
        }

        public static string Encrypt(string plainText)
        {
            strCryptoKey = _config.GetValue<string>("CryptoKey");
            IV = new byte[] { 50, 199, 10, 159, 132, 55, 236, 189, 51, 243, 244, 91, 17, 136, 39, 230 };

            string workText = plainText.Replace(Convert.ToChar(0x0).ToString(), "");

            byte[] strBytes = Encoding.UTF8.GetBytes(workText);
            byte[] strKeyBytes = Encoding.UTF8.GetBytes(GetHash(strCryptoKey));

            System.Security.Cryptography.RijndaelManaged rijndael = new System.Security.Cryptography.RijndaelManaged();
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            System.Security.Cryptography.ICryptoTransform cryptoTransform = null;

            cryptoTransform = rijndael.CreateEncryptor(strKeyBytes, IV);

            System.Security.Cryptography.CryptoStream cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream, cryptoTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            cryptoStream.Write(strBytes, 0, strBytes.Length);
            cryptoStream.FlushFinalBlock();

            string encrypted = Convert.ToBase64String(memoryStream.ToArray());

            memoryStream.Close();
            cryptoStream.Close();
            return encrypted;
        }
    }
}
