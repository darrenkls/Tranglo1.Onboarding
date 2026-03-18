using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tranglo1.Onboarding.Application.Attributes;
using Tranglo1.Onboarding.Application.Command;

namespace Tranglo1.Onboarding.Application.Controllers
{

    [ApiController]
    [ApiVersion("1")]
    [ApiKeyAuthentication]
    [Route("api/v{version:apiVersion}/backgroundService")]
    public class BackgroundServiceController : Controller
    {
        private readonly ILogger<BackgroundServiceController> _logger;
        public IMediator Mediator { get; }
        private readonly IMapper _mapper;

        public BackgroundServiceController(ILogger<BackgroundServiceController> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger;
            Mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// For API to send email notificaiton
        /// 
        /// </summary>
        /// <param name="partnerCode"></param>
        /// <param name="partnerSubscriptionCode"></param>
        /// <returns></returns>
        [HttpPost("{partnerCode}/go-live-notification")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [SwaggerOperation(OperationId = nameof(PartnerGoLiveNotification), Tags = new[] { "Partner" })]
        public async Task<ActionResult<bool>> PartnerGoLiveNotification([PartnerCode] long partnerCode, [FromQuery] long partnerSubscriptionCode)
        {
            PartnerGoLiveNotifyCommand command = new PartnerGoLiveNotifyCommand
            {
                PartnerCode = partnerCode,
                PartnerSubscriptionCode = partnerSubscriptionCode
            };
            var result = await Mediator.Send(command);

            if (result.IsFailure)
            {
                ModelState.AddModelError("Error", result.Error);
                _logger.LogError($"[PartnerGoLiveNotification] {result.Error}");
                return ValidationProblem(result.Error);
            }

            //return Ok(result.Value);
            return Ok();
        }
    }
}
