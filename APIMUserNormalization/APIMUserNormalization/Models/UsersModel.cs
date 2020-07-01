// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace APIMUserNormalization.Models
{
    public class UsersModel
    {
        public UserModel[] Users { get; set; }

        public static UsersModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON, typeof(UsersModel)) as UsersModel;
        }
    }
}