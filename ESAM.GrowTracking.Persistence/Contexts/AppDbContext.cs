using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Commons.Exceptions;
using ESAM.GrowTracking.Persistence.Seedings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ESAM.GrowTracking.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(ILogger<AppDbContext> logger, DbContextOptions<AppDbContext> options) : base(options)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            _logger = logger;
        }

        //public DbSet<AcademicDegree> AcademicDegrees { get; set; }

        //public DbSet<AcademicInstitute> AcademicInstitutes { get; set; }

        //public DbSet<AcademicTitle> AcademicTitles { get; set; }

        public DbSet<BlacklistedAccessTokenTemporary> BlacklistedAccessTokensTemporary { get; set; }

        public DbSet<BlacklistedAccessTokenPermanent> BlacklistedAccessTokensPermanent { get; set; }

        public DbSet<BlacklistedRefreshToken> BlacklistedRefreshTokens { get; set; }

        public DbSet<BusinessUnit> BusinessUnits { get; set; }

        //public DbSet<BusinessUnitLogo> BusinessUnitLogos { get; set; }

        public DbSet<Campus> Campuses { get; set; }

        //public DbSet<CampusAddress> CampusAddresses { get; set; }

        //public DbSet<CampusEmail> CampusEmails { get; set; }

        //public DbSet<CampusLogo> CampusLogos { get; set; }

        //public DbSet<CampusPhone> CampusPhones { get; set; }

        //public DbSet<CampusSocialNetwork> CampusSocialNetworks { get; set; }

        //public DbSet<Country> Countries { get; set; }

        //public DbSet<Locality> Localities { get; set; }

        //public DbSet<Manager> Managers { get; set; }

        public DbSet<Module> Modules { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<Person> People { get; set; }

        //public DbSet<PersonAcademicRecord> PersonAcademicRecords { get; set; }

        //public DbSet<PersonAddress> PersonAddresses { get; set; }

        //public DbSet<PersonBirthday> PersonBirthdays { get; set; }

        //public DbSet<PersonEmail> PersonEmails { get; set; }

        //public DbSet<PersonPhone> PersonPhones { get; set; }

        //public DbSet<PersonPhoto> PersonPhotos { get; set; }

        //public DbSet<PersonSocialNetwork> PersonSocialNetworks { get; set; }

        //public DbSet<ReadAudit> ReadAudits { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<RolePermission> RolePermissions { get; set; }

        //public DbSet<State> States { get; set; }

        //public DbSet<Student> Students { get; set; }

        //public DbSet<Teacher> Teachers { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserDevice> UsersDevices { get; set; }

        public DbSet<UserPhoto> UserPhotos { get; set; }

        public DbSet<UserRoleCampus> UserRoleCampuses { get; set; }

        public DbSet<UserSession> UserSessions { get; set; }

        public DbSet<UserSessionRefreshToken> UserSessionRefreshTokens { get; set; }

        public DbSet<UserSessionUserWorkProfileSelected> UserSessionUserWorkProfilesSelected { get; set; }

        public DbSet<UserSessionUserWorkProfileSelectedUserRoleCampusSelected> UserSessionUserWorkProfileSelectedUserRoleCampusesSelected { get; set; }

        public DbSet<UserWorkProfile> UserWorkProfiles { get; set; }

        public DbSet<WorkProfile> WorkProfiles { get; set; }

        public DbSet<WorkProfilePermission> WorkProfilePermissions { get; set; }
        
        //public DbSet<WriteAudit> WriteAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _logger.LogDebug("Construyendo el modelo de EF Core");
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Seed();
        }
    }
}