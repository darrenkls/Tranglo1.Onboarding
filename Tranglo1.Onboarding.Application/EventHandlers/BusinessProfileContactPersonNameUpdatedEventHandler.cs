using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Domain.Entities;
using Tranglo1.Onboarding.Domain.Entities.BusinessProfileAggregate.OwnershipManagement;
using Tranglo1.Onboarding.Domain.Events;
using Tranglo1.Onboarding.Domain.Repositories;
using Tranglo1.Onboarding.Application.Common.EventHandlers;

namespace Tranglo1.Onboarding.Application.EventHandlers
{
    class BusinessProfileContactPersonNameUpdatedEventHandler : BaseEventHandler<BusinessProfileContactPersonNameUpdatedEvent>
    {
        private readonly IBusinessProfileRepository _businessProfileRepository;
        private readonly ILogger<BusinessProfileContactPersonNameUpdatedEvent> _logger;

        public BusinessProfileContactPersonNameUpdatedEventHandler(
            IBusinessProfileRepository businessProfileRepository,
            ILogger<BusinessProfileContactPersonNameUpdatedEvent> logger)
        {
            _logger = logger;
            _businessProfileRepository = businessProfileRepository;
        }
        protected override async Task HandleAsync(BusinessProfileContactPersonNameUpdatedEvent @event, CancellationToken cancellationToken)
        {
            var domainEvent = @event;

            // Check if the sync should not be performed
            if (domainEvent.BusinessProfile.SolutionCode == Solution.Connect.Id || domainEvent.BusinessProfile.SolutionCode == Solution.Business.Id)
            {
                // exit without syncing
                return;
            }

            //Check if exist in AuthorisedPerson 
            var authorisedPerson = await _businessProfileRepository.GetAuthorisedPersonByDefaultAsync(domainEvent.BusinessProfile);

            domainEvent.BusinessProfile.RemoveDomainEvent(@event);

            if (authorisedPerson is null)
            {
                //add
                authorisedPerson = new AuthorisedPerson(domainEvent.BusinessProfile, domainEvent.ContactPersonName, AuthorisationLevel.Main,
                                                        null, null, null, null, null, null, null, null, null, null, null, null, true);
                await _businessProfileRepository.AddAuthorisedPersonAsync(authorisedPerson);
            }
            else
            {
                authorisedPerson.BusinessProfile.RemoveDomainEvent(@event);
                //TODO: To relook into cancellation token implementation
                authorisedPerson.FullName = domainEvent.ContactPersonName;
                await _businessProfileRepository.UpdateAuthorisedPerson(authorisedPerson, new CancellationToken());
            }
        }
    }
}
