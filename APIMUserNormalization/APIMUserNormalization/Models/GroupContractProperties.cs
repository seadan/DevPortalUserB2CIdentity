using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class GroupContractProperties
    {

        [JsonPropertyName("builtIn")]
        public bool BuiltIn { get; set; }


        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }


    }
}


