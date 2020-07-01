using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class Properties
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }


        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("registrationDate")]
        public string RegistrationDate { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("identities")]
        public UserIdentityContract[] Identities { get; set; }

        [JsonPropertyName("groups")]
        public GroupContractCollection Groups { get; set; }
    }
}

