using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Newtonsoft.Json;
using System.IO;

namespace APIMUserNormalization.Models
{
    public class AppSettingsFile
    {
        public AppSettings AppSettings { get; set; }
        private static IConfiguration Configuration { set; get; }

        public static AppSettings ReadFromJsonFile()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAzureAppConfiguration("Endpoint=https://usernormalizationconfiguration.azconfig.io;Id=Do9P-l6-s0:FCswtGBYgfMD7TNh+fMi;Secret=Veq5LbBxfMatEwsrpdAmvQScN6Rp0YTKMe3MbqXQhXY=");
            Configuration = builder.Build();
            return Configuration.Get<AppSettings>();
        }
    }

    public class AppSettings
    {

        [JsonProperty(PropertyName = "AADB2CTenantId")]
        public string AADB2CTenantId { get; set; }

        [JsonProperty(PropertyName = "AADB2CAppId")]
        public string AADB2CAppId { get; set; }

        [JsonProperty(PropertyName = "AADB2CClientSecret")]
        public string AADB2CClientSecret { get; set; }

        [JsonProperty(PropertyName = "AADB2CB2cExtensionAppClientId")]
        public string AADB2CB2cExtensionAppClientId { get; set; }

        [JsonProperty(PropertyName = "APIMSubscriptionIds")]
        public string APIMSubscriptionIds { get; set; }

        [JsonProperty(PropertyName = "APIMResourceGroups")]
        public string APIMResourceGroups { get; set; }

        [JsonProperty(PropertyName = "APIMApiManagementNames")]
        public string APIMApiManagementNames { get; set; }

        [JsonProperty(PropertyName = "APIMPostURL")]
        public string APIMPostURL { get; set; }

        [JsonProperty(PropertyName = "APIMClientID")]
        public string APIMClientID { get; set; }

        [JsonProperty(PropertyName = "APIMClientSecret")]
        public string APIMClientSecret { get; set; }

        [JsonProperty(PropertyName = "APIMResource")]
        public string APIMResource { get; set; }

        [JsonProperty(PropertyName = "APIMTenantId")]
        public string APIMTenantId { get; set; }
    }
}
