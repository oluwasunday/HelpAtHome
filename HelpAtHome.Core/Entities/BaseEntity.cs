namespace HelpAtHome.Core.Entities
{
    /// <summary>
    /// Base class for all non-identity domain entities.
    /// User does NOT inherit this — it inherits IdentityUser<Guid> instead.
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
