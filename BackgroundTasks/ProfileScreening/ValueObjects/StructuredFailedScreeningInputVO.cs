using System.Collections.Generic;

namespace ProfileScreening.ValueObjects
{
    public class StructuredFailedScreeningInput
    {
        public long OwnershipStructureTypeId { get; set; }
        public List<long> TableIds { get; set; } = new List<long>();
    }
}
