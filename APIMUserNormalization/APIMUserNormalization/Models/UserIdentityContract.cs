using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserIdentityContract
    {
        //{"value":[{"provider":"AadB2C","id":"5318e5de-9153-4552-8298-b5fb69e8d596"}],"count":1,"nextLink":null}

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("provider")]
        public string Provider { get; set; }

    }
}

