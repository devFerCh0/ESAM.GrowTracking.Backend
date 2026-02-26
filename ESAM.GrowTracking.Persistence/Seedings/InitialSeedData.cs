using ESAM.GrowTracking.Domain.Catalogs;
using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ESAM.GrowTracking.Persistence.Seedings
{
    public static class InitialSeedData
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var module1 = new Module(1, "Académico");
            modelBuilder.Entity<Module>().HasData(module1);

            var permission1 = new Permission(1, 1, "Agregar Proyectos");
            var permission2 = new Permission(2, 1, "Agregar Calificación");
            var permission3 = new Permission(3, 1, "Ver Calificaciones");
            modelBuilder.Entity<Permission>().HasData(permission1, permission2, permission3);

            var workProfile1 = new WorkProfile(1, "Gestor", WorkProfileType.WithRoles);
            var workProfile2 = new WorkProfile(2, "Docente", WorkProfileType.OnlyWorkProfile);
            var workProfile3 = new WorkProfile(3, "Estudiante", WorkProfileType.OnlyWorkProfile);
            modelBuilder.Entity<WorkProfile>().HasData(workProfile1, workProfile2, workProfile3);

            var workProfilePermission1 = new WorkProfilePermission(2, 1, false, 1);
            var workProfilePermission2 = new WorkProfilePermission(2, 2, true, 1);
            var workProfilePermission3 = new WorkProfilePermission(2, 3, true, 1);
            var workProfilePermission4 = new WorkProfilePermission(3, 1, false, 1);
            var workProfilePermission5 = new WorkProfilePermission(3, 2, false, 1);
            var workProfilePermission6 = new WorkProfilePermission(3, 3, true, 1);
            modelBuilder.Entity<WorkProfilePermission>().HasData(workProfilePermission1, workProfilePermission2, workProfilePermission3, workProfilePermission4, workProfilePermission5, workProfilePermission6);

            var role1 = new Role(1, "Coordinador de T. I.", 1);
            modelBuilder.Entity<Role>().HasData(role1);

            var rolePermission1 = new RolePermission(1, 1, true, 1);
            var rolePermission2 = new RolePermission(1, 2, true, 1);
            var rolePermission3 = new RolePermission(1, 3, true, 1);
            modelBuilder.Entity<RolePermission>().HasData(rolePermission1, rolePermission2, rolePermission3);

            var businessUnits1 = new BusinessUnit(1, "ESAM", "ESAM", "https://esam.edu.bo/", new DateTime(2000, 01, 01), 1);
            modelBuilder.Entity<BusinessUnit>().HasData(businessUnits1);

            var campus1 = new Campus(1, 1, "ESAM Sucre 2", "https://esam.edu.bo/Sucre2", new DateTime(2022, 01, 01), 1);
            var campus2 = new Campus(2, 1, "ESAM Monteagudo", "https://esam.edu.bo/Monteagudo", new DateTime(2022, 01, 01), 1);
            modelBuilder.Entity<Campus>().HasData(campus1, campus2);

            var person1 = new Person(1, "Luis Fernando", "Flores", "Padilla", "5681003", IdentityDocumentType.IdentityCard, Gender.Man, MaritalStatus.Married, 1);
            var person2 = new Person(2, "Efrain", "Chiri", "Nina", "13071262", IdentityDocumentType.IdentityCard, Gender.Man, MaritalStatus.Single, 1);
            modelBuilder.Entity<Person>().HasData(person1, person2);

            // Password user1: BodoqueManchas2025.
            // Password user2: 13071262
            var user1 = new User(1, "lflorespadilla", "luis.flores@esam.edu.bo", "vUcZ/OlrC75ZxlRRcYQyWw==", "Z7eBIXKE/zRbqjjTQxBblU7PPgL2PEripZFO2uXn0I8=", "2bb48cdd-afbd-48f7-ab11-0cd74eea240e", 1);
            var user2 = new User(2, "echirinina", "efrain.chiri@esam.edu.bo", "fyJIWA4KGwOZTuLLPKyZlg==", "b2J0LbtVmAE85Y3MJYtjVWcA6eNsgtJT4NGxZQgqxjg=", "2f01a267-92db-4703-99f5-5b995167d3bd", 1);
            modelBuilder.Entity<User>().HasData(user1, user2);

            var userRoleCampus1 = new UserRoleCampus(1, 1, 1, 1);
            var userRoleCampus2 = new UserRoleCampus(1, 1, 2, 1);
            var userRoleCampus3 = new UserRoleCampus(2, 1, 1, 1);
            var userRoleCampus4 = new UserRoleCampus(2, 1, 2, 1);
            modelBuilder.Entity<UserRoleCampus>().HasData(userRoleCampus1, userRoleCampus2, userRoleCampus3, userRoleCampus4);

            var userWorkProfile1 = new UserWorkProfile(1, 1, 1);
            var userWorkProfile2 = new UserWorkProfile(1, 2, 1);
            var userWorkProfile3 = new UserWorkProfile(1, 3, 1);
            var userWorkProfile4 = new UserWorkProfile(2, 1, 1);
            var userWorkProfile5 = new UserWorkProfile(2, 2, 1);
            var userWorkProfile6 = new UserWorkProfile(2, 3, 1);
            modelBuilder.Entity<UserWorkProfile>().HasData(userWorkProfile1, userWorkProfile2, userWorkProfile3, userWorkProfile4, userWorkProfile5, userWorkProfile6);
        }
    }
}