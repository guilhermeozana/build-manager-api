using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class UserGroupFactory
    {
        public static UserGroup GetUserGroup()
        {
            return new UserGroup()
            {
                GroupId = 1,
                UserId = 1
            };
        }

        public static UserGroup GetUserGroup(int userId, int groupId)
        {
            return new UserGroup()
            {
                GroupId = groupId,
                UserId = userId
            };
        }
    }
}
