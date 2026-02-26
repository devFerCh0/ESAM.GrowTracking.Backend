namespace ESAM.GrowTracking.Domain.Abstractions
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; private set; }

        public int CreatedBy { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public int? UpdatedBy { get; private set; }

        protected void CreateAudit(int createdBy)
        {
            CreatedBy = createdBy;
        }

        public void UpdateAudit(DateTime updatedAt, int updatedBy)
        {
            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }
    }
}