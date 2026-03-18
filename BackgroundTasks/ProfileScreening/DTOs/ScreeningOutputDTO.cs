using Newtonsoft.Json;
using ProfileScreening.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Domain.Common.SingleScreening;

namespace ProfileScreening.DTOs
{
    public class ScreeningOutputDTO
    {
        public List<ChangeDTO> ChangeDtos { get; set; }
        public int SuccessBusinessProfileScreenCount { get; set; }
        public int ScreeningInputCount { get; set; }
        public List<FailedScreeningInput> FailedNameScreeningInputs { get; set; }
        public List<FailedScreeningInput> FailedGetMatchedEntityScreeningInputs { get; set; }

        public string GetFailedNameScreeningInputsJson()
        {
            return GetStructuredFailedScreeningInputsJson(FailedNameScreeningInputs);
        }

        public string GetFailedGetMatchedEntityScreeningInputsJson()
        {
            return GetStructuredFailedScreeningInputsJson(FailedGetMatchedEntityScreeningInputs);
        }

        #region Private Helper Methods
        private string GetStructuredFailedScreeningInputsJson(List<FailedScreeningInput> failedInputs)
        {
            var structuredFailedInputs = new List<StructuredFailedScreeningInput>();

            foreach (var failedInput in failedInputs)
            {
                var existingEntry = structuredFailedInputs.Find(s => s.OwnershipStructureTypeId == failedInput.OwnershipStructureTypeId);

                if (existingEntry != null)
                {
                    existingEntry.TableIds.Add(failedInput.TableId);
                }
                else
                {
                    structuredFailedInputs.Add(new StructuredFailedScreeningInput
                    {
                        OwnershipStructureTypeId = failedInput.OwnershipStructureTypeId,
                        TableIds = new List<long> { failedInput.TableId }
                    });
                }
            }

            return JsonConvert.SerializeObject(
                structuredFailedInputs.OrderBy(x => x.OwnershipStructureTypeId), 
                Formatting.Indented);
        } 
        #endregion Private Helper Methods
    }
}
