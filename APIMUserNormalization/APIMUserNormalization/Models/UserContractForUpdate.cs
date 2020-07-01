using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserContractForUpdate
    {
        [JsonPropertyName("properties")]
        public PropertiesForUpdate Properties { get; set; }


    }

    public class PropertiesForUpdate
    {

        [JsonPropertyName("identities")]
        public UserIdentityContract[] Identities { get; set; }

        internal void AddIdentity(string v, string id)
        {
            if (Identities == null)
            {
                Identities = new UserIdentityContract[1] { new UserIdentityContract { Id = id, Provider = v } };
            }
            else
            {
                List<UserIdentityContract> identityList = new List<UserIdentityContract>();
                for (int i = 0; i < Identities.Length; i++)
                {
                    identityList.Add(Identities[i]);
                }
                identityList.Add(new UserIdentityContract() { Id = id, Provider = v });
                Identities = identityList.ToArray();
            }

        }

    }
}


