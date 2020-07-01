using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserIdentityCollection
    {
        //{"value":[{"provider":"AadB2C","id":"5318e5de-9153-4552-8298-b5fb69e8d596"}],"count":1,"nextLink":null}
        [JsonPropertyName("count")]
        public int count { get; set; }

        [JsonPropertyName("nextLink")]
        public string nextLink { get; set; }

        [JsonPropertyName("value")]
        public UserIdentityContract[] value { get; set; }

    }
}

