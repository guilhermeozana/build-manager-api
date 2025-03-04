namespace Marelli.Domain.Entities
{
    public class LogAndArtifact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int BuildId { get; set; }
        public string FileLogS3BucketLocation { get; set; }
        public string FileOutS3BucketLocation { get; set; }
    }
}
