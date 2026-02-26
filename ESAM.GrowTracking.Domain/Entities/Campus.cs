using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class Campus : AuditableEntity, IEntity<int>
    {
        private Campus() { }

        public int Id { get; private set; }

        public int BusinessUnitId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string WebSite { get; private set; } = string.Empty;

        public DateTime FoundingDate { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public BusinessUnit BusinessUnit { get; private set; } = null!;

        public List<UserRoleCampus> UserRoleCampuses { get; private set; } = [];

        //public List<CampusAddress> CampusAddresses { get; private set; } = [];

        //public List<CampusPhone> CampusPhones { get; private set; } = [];

        //public List<CampusEmail> CampusEmails { get; private set; } = [];

        //public List<CampusSocialNetwork> CampusSocialNetworks { get; private set; } = [];

        //public List<CampusLogo> CampusLogos { get; private set; } = [];

        public Campus(int id, int businessUnitId, string name, string webSite, DateTime foundingDate, int createdBy)
        {
            Id = id;
            BusinessUnitId = businessUnitId;
            Name = name;
            WebSite = webSite;
            FoundingDate = foundingDate;
            CreateAudit(createdBy);
        }
    }
}