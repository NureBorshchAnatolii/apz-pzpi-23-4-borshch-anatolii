namespace CareLink.Domain.Entities.SubEntities
{
    public abstract class SubEntity : BaseEntity
    {
        public string Name { get; set; } = null!;
    }
}