using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class UserFactory
    {

        public static User GetUser()
        {
            return new User()
            {
                Id = 0,
                Email = Guid.NewGuid().ToString(),
                Name = "Test",
                Password = "password@123A",
                NewUser = true,
                Role = "ADMIN"
            };
        }

        public static User GetUserWithoutGroup()
        {
            return new User()
            {
                Id = 0,
                Email = Guid.NewGuid().ToString(),
                Name = "Test",
                Password = "password@123A",
                NewUser = true,
                Role = "ADMIN"
            };
        }

        public static UserResponse GetUserResponse()
        {
            return new UserResponse()
            {
                Id = 0,
                Email = "my-email@test.com",
                Name = "Test",
                NewUser = true,
                Role = "ADMIN",
                Groups = new List<Group>() { GroupFactory.GetGroup() },
                Projects = new List<Project>() { ProjectFactory.GetProject() }
            };
        }
    }
}
