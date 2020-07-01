using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;


namespace APIMUserNormalization.Models
{
    public class GroupContractCollection
    {
        [JsonPropertyName("value")]
        public GroupContract[] value { get; set; }

        [JsonPropertyName("count")]
        public int count { get; set; }

    }
}
