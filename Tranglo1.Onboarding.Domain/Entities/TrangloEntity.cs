using System.Collections.Generic;
using System.Linq;
using Tranglo1.Onboarding.Domain.Common;

namespace Tranglo1.Onboarding.Domain.Entities
{
    public class TrangloEntity : Enumeration
    {
        public string TrangloEntityCode { get; set; }

        public TrangloEntity() : base() { }

        public TrangloEntity(int id, string name, string code) : base(id, name)
        {
            TrangloEntityCode = code;
        }

        public static IEnumerable<TrangloEntity> GetTrangloEntities()
        {
            return GetAll<TrangloEntity>();
        }

        public static TrangloEntity GetByEntityByTrangloId(string trangloCode)
        {
            return GetAll<TrangloEntity>().FirstOrDefault(x => x.TrangloEntityCode == trangloCode);
        }

        public static readonly TrangloEntity TSB = new TrangloEntity(1, "Tranglo Sdn Bhd", "TSB");
        public static readonly TrangloEntity TPL = new TrangloEntity(2, "Tranglo Pte Ltd", "TPL");
        public static readonly TrangloEntity TEL = new TrangloEntity(3, "Tranglo Europe Ltd", "TEL");
        public static readonly TrangloEntity PTT = new TrangloEntity(4, "PT Tranglo Indonesia", "PTT");
    }
}
