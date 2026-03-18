using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.BusinessDeclaration;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.Services.SignalR;

namespace Tranglo1.Onboarding.Application.Command
{

    internal class ChangeCustomerTypeCommand : BaseCommand<Result>
    {
        public int BusinessProfileCode { get; set; }
        public long AdminSolution { get; set; }
        public ChangeCustomerTypeInputDTO InputDTO { get; set; }
        public string LoginId { get; set; }

        public override Task<string> GetAuditLogAsync(Result result)
        {
            if (result.IsSuccess)
            {
                string _description = $"Change Customer Type handling for BusinessProfileCode: [{BusinessProfileCode}]";
                return Task.FromResult(_description);
            }

            return Task.FromResult<string>(null);
        }
    }

    internal class ChangeCustomerTypeCommandHandler : IRequestHandler<ChangeCustomerTypeCommand, Result>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly BusinessProfileService _businessProfileService;
        private readonly ILogger<ChangeCustomerTypeCommandHandler> _logger;
        private readonly TrangloUserManager _trangloUserManager;
        private readonly SignalRMessageService _signalRNotificationService;

        public ChangeCustomerTypeCommandHandler(IBusinessProfileRepository businessProfileRepository, 
            BusinessProfileService businessProfileService, 
            ILogger<ChangeCustomerTypeCommandHandler> logger, 
            TrangloUserManager trangloUserManager,
            SignalRMessageService signalRNotificationService
            )
        {
            _businessProfileRepository = businessProfileRepository;
            _businessProfileService = businessProfileService;
            _logger = logger;
            _trangloUserManager = trangloUserManager;
            _signalRNotificationService = signalRNotificationService;
        }

        public async Task<Result> Handle(ChangeCustomerTypeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var adminUser = await _trangloUserManager.FindByIdAsync(request.LoginId);
                var adminSolution = await _businessProfileRepository.GetSolutionByCodeAsync(request.AdminSolution);

                if (adminSolution.Id != Solution.Business.Id)
                {
                    // Change Customer Type handling
                    if (request.InputDTO.NewCustomerTypeCode.HasValue && request.InputDTO.CurrentCustomerTypeCode.HasValue)
                    {
                        var newCustomerTypeCode = request.InputDTO.NewCustomerTypeCode.Value;
                        var currentCustomerTypeCode = request.InputDTO.CurrentCustomerTypeCode.Value;

                        var changeCustomerTypeHandling = await _businessProfileService.ChangeCustomerTypeHandling(request.BusinessProfileCode, newCustomerTypeCode, currentCustomerTypeCode, adminUser.Id);
                        if (changeCustomerTypeHandling.IsFailure)
                        {
                            return Result.Failure($"Failed to change Customer Type for BusinessProfileCode: {request.BusinessProfileCode}. {changeCustomerTypeHandling.Error}");
                        }
                        else if(changeCustomerTypeHandling.Value is true) // returns true if redo declaration
                        {
                            // Broadcast redo log off alert
                            await _signalRNotificationService.RedoBusinessDeclarationLogOffAlert(request.BusinessProfileCode);
                        }
                        else if (changeCustomerTypeHandling.Value is false) // returns false if non redo declaration
                        {
                            // Broadcast non redo log off alert
                            await _signalRNotificationService.NonRedoBusinessDeclarationLogOffAlert(request.BusinessProfileCode);
                        }
                    }
                }
                else
                {
                    return Result.Failure($"Change customer type is only available for Tranglo Business. Current Admin Solution: {adminSolution.Name}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ex.Message);
            }
        }
    }
}