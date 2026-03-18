using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Tranglo1.Onboarding.Application.Command;
using Tranglo1.Onboarding.Application.CustomerUserList.Commands;
using Tranglo1.Onboarding.Application.DTO;
using Tranglo1.Onboarding.Application.DTO.BusinessProfile;
using Tranglo1.Onboarding.Application.DTO.CommentAndReviewRemarks;
using Tranglo1.Onboarding.Application.DTO.ComplianceOfficers;
using Tranglo1.Onboarding.Application.DTO.CustomerUser;
using Tranglo1.Onboarding.Application.DTO.Declarations;
using Tranglo1.Onboarding.Application.DTO.Documentation;
using Tranglo1.Onboarding.Application.DTO.KYCAdminManagement.AdminManagement;
using Tranglo1.Onboarding.Application.DTO.LicenseInformation;
using Tranglo1.Onboarding.Application.DTO.Partner;
using Tranglo1.Onboarding.Application.DTO.Partner.PartnerRegistration;
using Tranglo1.Onboarding.Application.Models;
using Tranglo1.Onboarding.Application.Queries;

namespace Tranglo1.Onboarding.Application.Mappers
{
	internal class DtoToCommandProfile : Profile
	{
		public DtoToCommandProfile()
		{
			CreateMap<ForgotPasswordModel, RequestResetPasswordCommand>();
			CreateMap<CreatePasswordModel, VerifyCustomerUserResetPasswordCommand>();
			CreateMap<LoginInputModel, ValidateUserPasswordCommand>();
			CreateMap<RegisterInputModel, RegisterCustomerUserCommand>();
			CreateMap<RegisterWithRegistryCodeInputModel, RegisterCustomerUserWithCodeCommand>();

			CreateMap<InviteUserInputDTO, InviteUserCommand>();
			CreateMap<InviteePasswordVerificationViewModel, InviteePasswordVerificationCommand>();
			CreateMap<UnlockUserInputDTO, UnlockUserCommand>();
			CreateMap<LockUserInputDTO, LockUserCommand>();
			//CreateMap<BusinessProfileInputDTO, CreateBusinessProfileCommand>();
			CreateMap<BusinessProfileInputDTO, UpdateBusinessProfileCommand>()
				.ForMember(dto => dto.ContactNumber, opt => opt.MapFrom(domain => domain.ContactNumber))
				.ForMember(dto => dto.ContactNumberCountryISO2, opt => opt.MapFrom(domain => domain.ContactNumberCountryISO2))
				.ForMember(dto => dto.DialCode, opt => opt.MapFrom(domain => domain.DialCode));
			CreateMap<LicenseInformationInputDTO, SaveLicenseInformationCommand>();
			CreateMap<LicenseInformationInputDTO, UpdateLicenseInformationCommand>();
			//.AfterMap<RecaptchaTokenSetter>();
			CreateMap<ComplianceOfficersInputDTO, SaveCoInformationCommand>()
				.ForMember(dto => dto.ContactNum, opt => opt.MapFrom(domain => domain.ContactNumber))
				.ForMember(dto => dto.ContactNumberCountryISO2, opt => opt.MapFrom(domain => domain.ContactNumberCountryISO2))
				.ForMember(dto => dto.DialCode, opt => opt.MapFrom(domain => domain.DialCode));


			CreateMap<ComplianceOfficersInputDTO, UpdateCoInformationCommand>();
			CreateMap<CommentAndReviewRemarksInputDTO, SaveCommentsCommand>();
			CreateMap<CommentAndReviewRemarksInputDTO, SaveReviewRemarksCommand>();
			CreateMap<TrangloEntityBlockStatusInputDTO, UpdateTrangloStaffBlockStatusCommand>();
			CreateMap<TrangloStaffUserUpdateInputDTO, UpdateTrangloStaffAssignmentCommand>();
			CreateMap<VerifyCustomerUserEmailModel, VerifyCustomerUserEmailCommand>()
				.ForMember(
				command => command.Token,
				mapping => mapping.MapFrom(
					dto => Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token))));

			CreateMap<DeclarationsInputDTO, CreateDeclarationCommand>();

			CreateMap<DeclarationsInputDTO, UpdateDeclarationInformationCommand>();
			CreateMap<DocumentInfoInputDTO, UpdateDocumentInfoCommand>();
			CreateMap<DocumentCategoryInfoInputDTO, UpdateDocumentCategoriesInfoCommand>();
			CreateMap<PartnerRegistrationInputDTO, SavePartnerRegistrationCommand>();
			CreateMap<ResendInvitationInputDTO, ResendInvitationCommand>();

			CreateMap<PartnerAgreementStatusInputDTO, UpdatePartnerAgreementStatusCommand>();
			CreateMap<SignedPartnerAgreementInputDTO, UpdateSignedPartnerAgreementCommand>();
			CreateMap<HelloSignDocumentInputDTO, SaveHelloSignDocumentCommand>();
			CreateMap<PartnerAPISettingsInputDTO, SavePartnerAPISettingsCommand>();
			CreateMap<PartnerAPISettingsInputDTO, UpdateAPIPartnerSettingsCommand>();
			CreateMap<WhitelistIPAddressInputDTO, SaveWhitelistIPAddressCommand>();
			CreateMap<WhitelistIPAddressInputDTO, UpdatePendingWhitelistIPCommand>();
			CreateMap<PartnerAPISettingsInputDTO, UpdatePendingConfigureCallbackURLCommand>();
			//.ForMember(dto => dto.Staging, opt => opt.MapFrom(domain => domain.Staging))
			//            .ForMember(dto => dto.Production, opt => opt.MapFrom(domain => domain.Production));
			//CreateMap<PartnerAPISettingsInputDTO, PartnerAPISettings>();


		}
	}
}
