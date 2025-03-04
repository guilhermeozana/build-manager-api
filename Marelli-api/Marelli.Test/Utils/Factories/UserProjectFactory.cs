using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class UserProjectFactory
    {
        public static UserProject GetUserProject()
        {
            return new UserProject()
            {
                ProjectId = 1,
                UserId = 1
            };
        }

        public static UserProject GetUserProject(int userId, int projectId)
        {
            return new UserProject()
            {
                ProjectId = projectId,
                UserId = userId
            };
        }
    }
}
