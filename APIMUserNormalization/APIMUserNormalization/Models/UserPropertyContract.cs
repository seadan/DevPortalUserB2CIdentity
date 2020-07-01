using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserPropertyContract
    {
        [JsonPropertyName("properties")]
        public Properties Properties { get; set; }

    }
}


