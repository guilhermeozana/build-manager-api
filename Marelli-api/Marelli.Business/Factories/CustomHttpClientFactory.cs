namespace Marelli.Business.Factories
{
    public class CustomHttpClientFactory : ICustomHttpClientFactory
    {
        public HttpClient GetHttpClient()
        {
            return new HttpClient();
        }
    }
}
