namespace Marelli.Infra.Configuration;

public class AppSettings
{
    public string? Secret { get; set; }
    public int HoursExpiration { get; set; }
    public string? Sender { get; set; }
    public string? UrlHost { get; set; }
    public string? ImagesFolder { get; set; }
}