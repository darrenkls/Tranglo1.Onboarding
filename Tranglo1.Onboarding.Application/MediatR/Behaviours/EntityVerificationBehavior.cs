using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
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
using Tranglo1.Onboarding.Application.Common.Exceptions;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.Onboarding.Application.Queries;
using Tranglo1.Onboarding.Application.Services.Identity;
using Tranglo1.Onboarding.Infrastructure.Services;
using Tranglo1.UserAccessControl;

namespace Tranglo1.Onboarding.Application.MediatR.Behaviours
{
    public class EntityVerificationBehavior
    {
        public static Result TrangloEntityChecking(List<TrangloStaffEntityAssignment> staff, string entityCode)
        {
            foreach (var item in staff)
            {
                if (item.TrangloEntity.Equals(entityCode))
                {
                    return Result.Success(staff);
                }
            }
            return Result.Failure("Entity list do not match");
        }
    }
}
