namespace CareLink.Application.Dtos.Relatives
{
    public class RelativeDto
    {
        public long Id { get; set; }
        public long GuardianId { get; set; }
        public string GuardianFullName { get; set; }
        public long RelativeId { get; set; }
        public string RelativeFullName { get; set; }
        public string RelationType { get; set; }
        public DateTime AddedAt { get; set; }
    }
}