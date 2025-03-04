using Marelli.Domain.Entities;

namespace Marelli.Domain.Dtos
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool? NewUser { get; set; }
        public string? Name { get; set; }
        public ICollection<Group> Groups { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
}
