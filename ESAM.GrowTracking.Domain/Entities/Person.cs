using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Catalogs;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class Person : AuditableEntity, IEntity<int>
    {
        private Person() { }

        public int Id { get; private set; }

        public string FirstName { get; private set; } = string.Empty;

        public string LastName { get; private set; } = string.Empty;

        public string? SecondLastName { get; private set; }

        public string IdentityDocument { get; private set; } = string.Empty;

        public IdentityDocumentType IdentityDocumentType { get; private set; }

        public Gender Gender { get; private set; }

        public MaritalStatus MaritalStatus { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        //public Manager Manager { get; private set; } = null!;

        //public Teacher Teacher { get; private set; } = null!;

        //public Student Student { get; private set; } = null!;

        //public PersonBirthday PersonBirthday { get; private set; } = null!;

        //public List<PersonAddress> PersonAddresses { get; private set; } = [];

        //public List<PersonEmail> PersonEmails { get; private set; } = [];

        //public List<PersonPhone> PersonPhones { get; private set; } = [];

        //public List<PersonPhoto> PersonPhotos { get; private set; } = [];

        //public List<PersonSocialNetwork> PersonSocialNetworks { get; private set; } = [];

        //public List<PersonAcademicRecord> PersonAcademicRecords { get; private set; } = [];

        public Person(int id, string firstName, string lastName, string? secondLastName, string identityDocument, IdentityDocumentType identityDocumentType, Gender gender, MaritalStatus maritalStatus, int createdBy)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SecondLastName = secondLastName;
            IdentityDocument = identityDocument;
            IdentityDocumentType = identityDocumentType;
            Gender = gender;
            MaritalStatus = maritalStatus;
            CreateAudit(createdBy);
        }
    }
}