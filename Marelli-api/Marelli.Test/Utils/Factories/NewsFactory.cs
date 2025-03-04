using Marelli.Domain.Entities;

namespace Marelli.Test.Utils.Factories
{
    public class NewsFactory
    {

        public static News GetNews()
        {
            return new News()
            {
                Id = 0,
                Description = "test",
                ImageUrl = "my-imageUrl"
            };
        }
    }
}
