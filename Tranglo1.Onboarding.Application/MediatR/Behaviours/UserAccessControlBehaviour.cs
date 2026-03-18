using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.DomainServices;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.Constant;
using Tranglo1.Onboarding.Application.Common.Exceptions;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.Onboarding.Application.MediatR;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Services.Identity;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;
using ValidationException = FluentValidation.ValidationException;

namespace MediatR
{
    public class UserAccessControlBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly TrangloUserManager _userManager;
        private readonly AccessControlManager _accessControlManager;
        private readonly BusinessProfileService _businessProfileService;
        private readonly PartnerService _partnerService;
        private readonly ApplicationUserService _applicationUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBusinessProfileContext _businessProfileContext;
        private readonly IPartnerContext _partnerContext;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ITrangloRoleRepository _trangloRoleRepository;
        private readonly ITrangloEntityContext _trangloEntityContext;
        private readonly IIdentityContext identityContext;
        private readonly IRoleCodeContext _roleCodeContext;
        private readonly IConfiguration _config;
        public IMediator Mediator { get; }

        public UserAccessControlBehaviour(
                TrangloUserManager userManager,
                AccessControlManager accessControlManager,
                BusinessProfileService businessProfileService,
                PartnerService partnerService,
                ApplicationUserService applicationUserService,
                IHttpContextAccessor httpContextAccessor,
                ITrangloRoleRepository trangloRoleRepository,
                IBusinessProfileContext businessProfileContext,
                ITrangloEntityContext trangloEntityContext,
                IApplicationUserRepository applicationUserRepository,
                IIdentityContext identityContext,
                IPartnerContext partnerContext,
                IRoleCodeContext roleCodeContext,
                IMediator mediator,
                IConfiguration config
            )
        {
            _userManager = userManager;
            _partnerService = partnerService;
            _accessControlManager = accessControlManager;
            _applicationUserService = applicationUserService;
            _businessProfileService = businessProfileService;
            _httpContextAccessor = httpContextAccessor;
            _trangloEntityContext = trangloEntityContext;
            _trangloRoleRepository = trangloRoleRepository;
            _businessProfileContext = businessProfileContext;
            _applicationUserRepository = applicationUserRepository;
            _partnerContext = partnerContext;
            this.identityContext = identityContext;
            _roleCodeContext = roleCodeContext;
            Mediator = mediator;
            _config = config;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var _CurrentUser = identityContext.CurrentUser;

            if (_CurrentUser?.Identity?.IsAuthenticated == true)
            {
                var _Sub = _CurrentUser.GetSubjectId();

                if (_Sub.HasValue)
                {
                    var _solution = _CurrentUser.GetSolutionCode();
                    var roleCodeStrings = _CurrentUser.Claims.Where(x => x.Type.Contains("role_code"));

                    List<string> roleCodes = new List<string>();

                    foreach (var roleCode in roleCodeStrings)
                    {
                        var rolesSplit = roleCode.Value.Split(",");
                        roleCodes.AddRange(rolesSplit);
                    }
                    var permissionAttribute = PermissionAttributeRetrieval.GetPermissionAttribute<TRequest>();
                    var firstName = EntityAttributeRetrieval.GetAttribute(typeof(PartnerRegistrationInputDTO)
                     .GetMember(nameof(PartnerRegistrationInputDTO.Entity)).First());
                    if (permissionAttribute != null) //bypass for business first until ACL is done
                    {
                        var permissionInfo = _accessControlManager.GetPermissionInfoFromAttribute(permissionAttribute);

                        if (permissionInfo != null)
                        {
                            RequestContext.IsUacRequest = true;
                            GetUserAccessControlCheckQuery query = new GetUserAccessControlCheckQuery
                            {
                                RoleCodes = roleCodes,
                                PermissionCode = permissionInfo.PermissionInfoCode,
                                UserSolutionClaim = _solution.HasNoValue ? null : _solution.Value,
                                PermissionPortalCodes = permissionAttribute.PortalCodes
                            };

                            var resultIsPermissionOK = await Mediator.Send(query);
                            RequestContext.IsUacRequest = false;

                            if (resultIsPermissionOK.IsFailure || !resultIsPermissionOK.Value)
                            {
                                throw new ForbiddenException($"User account: {_Sub.Value} is not allowed to access the particular action");
                            }
                        }
                    }

                    //CustomerUser applicationUser = await _userManager.FindByIdAsync(_Sub.Value) as CustomerUser;
                    ApplicationUser applicationUser = await _userManager.FindByIdAsync(_Sub.Value);

                    if (applicationUser.LockoutEnd != null && applicationUser.AccountStatus == AccountStatus.Blocked && DateTimeOffset.Compare(applicationUser.LockoutEnd.Value, new DateTimeOffset(DateTime.UtcNow)) > 0)
                    {
                        throw new ForbiddenException($"User account: {_Sub.Value} is blocked");
                    }

                    Maybe<int> profileid = _businessProfileContext.CurrentProfileId;
                    Maybe<long> partnerId = _partnerContext.CurrentProfileId;
                    Maybe<string> trangloEntityCode = _trangloEntityContext.CurrentTrangloEntity;
                    Maybe<string> roleCodeContext = _roleCodeContext.CurrentRoleCode;
                    if (profileid.HasValue)
                    {
                        var businessProfileList = await _businessProfileService.GetBusinessProfilesByBusinessProfileCodeAsync(profileid.Value);
                        if (businessProfileList.Value.FirstOrDefault() == null)
                        {
                            throw new NotFoundException();
                        }

                        if (applicationUser is CustomerUser customerUser) //Only check if he is a customer user
                        {
                            var customerUserBusinessProfileResult = await _businessProfileService.GetCustomerUserBusinessProfilesAsync(customerUser, profileid.Value);
                            if (customerUserBusinessProfileResult.IsFailure)
                            {
                                //Customer User should only be allowed to view its own assigned business profile
                                throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access business profile code {profileid.Value}");
                            }

                            var _ProfileIsActive = _CurrentUser.IsBusinessProfileActive(profileid.Value);

                            if (_ProfileIsActive.HasNoValue || _ProfileIsActive.Value == false)
                            {
                                //If we have a business profile Id under current context, then
                                //the current user must have the active mapping too.
                                throw new ForbiddenException($"User account: {_Sub.Value} with business profile code {profileid.Value} is blocked");
                            }
                        }

                        if (applicationUser is TrangloStaff trangloStaff)
                        {
                            var TrangloStaffResult = await _businessProfileService.UserHasTrangloEntity(trangloStaff, profileid.Value);
                            if (TrangloStaffResult is false)
                            {
                                throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access entities from Partner Code: {profileid.Value}");
                            }
                        }
                    }
                    if (partnerId.HasValue)
                    {
                        if (applicationUser is CustomerUser customerUser) //Only check if he is a customer user
                        {
                            var PartnerRegistrationResult = await _partnerService.GetPartnerRegistrationAsync(customerUser, partnerId.Value);
                            if (PartnerRegistrationResult.IsFailure)
                            {
                                //Customer User should only be allowed to view its own assigned business profile
                                throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access entities from Partner Code: {partnerId.Value}");
                            }
                        }
                        if (applicationUser is TrangloStaff trangloStaff)
                        {
                            var TrangloStaffResult = await _partnerService.UserHasTrangloEntity(trangloStaff, partnerId.Value);
                            if (TrangloStaffResult is false)
                            {
                                throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access entities from Partner Code: {partnerId.Value}");
                            }
                        }
                    }
                    if (roleCodeContext.HasValue)
                    {
                        if (!roleCodes.Contains(roleCodeContext.Value))
                        {
                            throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access role code {roleCodeContext.Value}");
                        }
                    }
                    if (trangloEntityCode.HasValue)
                    {
                        if (applicationUser is TrangloStaff trangloStaff)
                        {
                            var TrangloStaffResult = await _applicationUserService.UserHasTrangloEntity(trangloStaff, trangloEntityCode.Value);
                            if (TrangloStaffResult is false)
                            {
                                throw new ForbiddenException($"User account: {_Sub.Value} is not authorized to access entities from Tranglo Entity Code: {trangloEntityCode.Value}");
                            }
                        }
                    }
                }
            }

            return await next();
        }
    }
}
