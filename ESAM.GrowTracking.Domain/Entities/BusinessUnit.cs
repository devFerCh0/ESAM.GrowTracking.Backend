using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class BusinessUnit : AuditableEntity, IEntity<int>
    {
        private BusinessUnit() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Abbreviation { get; private set; } = string.Empty;

        public string WebSite { get; private set; } = string.Empty;

        public DateTime FoundingDate { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public List<Campus> Campuses { get; private set; } = [];

        //public List<BusinessUnitLogo> BusinessUnitLogos { get; private set; } = [];

        public BusinessUnit(int id, string name, string abbreviation, string webSite, DateTime foundingDate, int createdBy)
        {
            Id = id;
            Name = name;
            Abbreviation = abbreviation;
            WebSite = webSite;
            FoundingDate = foundingDate;
            CreateAudit(createdBy);
        }
    }
}