using System.Collections.Generic;

namespace Tranglo1.Onboarding.Domain.Common.SingleScreening
{
    public class ComplianceScreeningOutputDTO
    {
        /// <summary>
        /// For RBA Screening Service usage.
        /// </summary>
        public List<SingleScreeningListResultOutputDTO> SingleScreeningListResultOutputDTOs { get; set; } = new List<SingleScreeningListResultOutputDTO>();

        /// <summary>
        /// For email notification usage.
        /// </summary>
        public List<ChangeDTO> ChangeDTOs { get; set; } = new List<ChangeDTO>();

        /// <summary>
        /// Number of screening inputs processed.
        /// </summary>
        public int ScreeningInputCount { get; set; } = 0;

        /// <summary>
        /// Screening Input Ids that failed for name screening.
        /// </summary>
        public List<FailedScreeningInput> FailedNameScreeningInputs { get; set; } = new List<FailedScreeningInput>();

        /// <summary>
        /// Screening Input Ids that failed to get matched entity.
        /// </summary>
        public List<FailedScreeningInput> FailedGetMatchedEntityScreeningInputs { get; set; } = new List<FailedScreeningInput>();
    }

    public class FailedScreeningInput
    {
        public long OwnershipStructureTypeId { get; set; }
        public long TableId { get; set; }
    }
}
