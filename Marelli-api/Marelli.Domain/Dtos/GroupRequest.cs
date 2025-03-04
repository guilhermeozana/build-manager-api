namespace Marelli.Domain.Dtos
{
    public class GroupRequest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }
        public string? CompanyImage { get; set; }

        public int[] UserIds { get; set; }
    }
}
