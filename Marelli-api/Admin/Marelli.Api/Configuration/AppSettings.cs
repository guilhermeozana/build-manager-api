namespace Marelli.Api.Configuration;

public class AppSettings
{
    public string? Secret { get; set; }
    public int ExpiracaoHoras { get; set; }
    public string? Emissor { get; set; }
    public string? ValidoEm { get; set; }
    public string? PastaImagens { get; set; }
}