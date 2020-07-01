using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace APIMUserNormalization.Models
{
    public class UserCollection
    {
        [JsonPropertyName("nextLink")]
        public string nextLink { get; set; }

        [JsonPropertyName("value")]
        public UserContract[] value { get; set; }

        [JsonPropertyName("count")]
        public int count { get; set; }

        public void AddUserContract(UserContract user)
        {

            if (value == null)
            {
                value = new UserContract[1] { user };
                count = 1;
            }
            else
            {
                List<UserContract> userList = new List<UserContract>();
                for (int i = 0; i < value.Length; i++)
                {
                    userList.Add(value[i]);
                }
                userList.Add(user);
                value = userList.ToArray();
                count = value.Length;
            }
        }

        internal bool RemoveUserContract(string id)
        {
            bool found = false;
            if (value == null)
            {
                return false;
            }
            else
            {
                List<UserContract> userList = new List<UserContract>();
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i].Id != id)
                    {
                        userList.Add(value[i]);
                    }
                    else
                    {
                        found = true;
                    }
                }
                value = userList.ToArray();
                count = value.Length;
            }

            return found;
        }
    }
}

