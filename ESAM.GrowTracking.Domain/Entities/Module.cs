using ESAM.GrowTracking.Domain.Abstractions;

namespace ESAM.GrowTracking.Domain.Entities
{
    public class Module : IEntity<int>
    {
        private Module() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public bool IsDeleted { get; private set; }

        public List<Permission> Permissions { get; private set; } = [];

        public Module(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}