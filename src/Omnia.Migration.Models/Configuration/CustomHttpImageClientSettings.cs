
namespace Omnia.Migration.Models.Configuration
{
    public class CustomHttpImageClientSettings
    {
        public AuthorizationMethod AuthorizeMethod { get; set; }
        public string Authorization { get; set; }
        public string Token { get; set; }
        public string Cookie { get; set; }
    }

    public enum AuthorizationMethod
    {
        Authorization,
        Token,
        Cookie,
    }
}
