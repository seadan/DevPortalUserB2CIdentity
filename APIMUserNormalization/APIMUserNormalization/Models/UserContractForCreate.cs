using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserContractForCreate
    {
        [JsonPropertyName("properties")]
        public PropertiesForCreate Properties { get; set; }

    }

    public class PropertiesForCreate
    {

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }


    }

}



