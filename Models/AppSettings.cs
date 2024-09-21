namespace CanamDistributors.Models
{
    public class AppSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string Environment { get; set; }
        public string Scopes { get; set; }
    }
}
