namespace Marelli.Domain.Entities
{
    public class FileVerify
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int BuildId { get; set; }
        public bool IsFileOk { get; set; }
        public string FileZipS3BucketLocation { get; set; }
        public string Filename { get; set; }
    }
}
