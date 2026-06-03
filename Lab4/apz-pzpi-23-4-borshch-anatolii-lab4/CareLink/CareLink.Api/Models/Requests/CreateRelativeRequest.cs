namespace CareLink.Api.Models.Requests
{
    public class CreateRelativeRequest
    {
        public long RelativeId { get; set; }
        public long RelationTypeId { get; set; }
    }
}