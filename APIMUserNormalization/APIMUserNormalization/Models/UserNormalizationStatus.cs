using System;
using System.Collections.Generic;
using System.Text;

namespace APIMUserNormalization.Models
{
    public class UserNormalizationStatus
    {

        //private string email = string.Empty;
        //private bool hasADB2C = false;
        //private bool isFoundInADB2C = false;
        //private bool isEmailFoundInADB2C = false;
        //private string objectId = string.Empty;


        public bool ExistsInAPIM { get; set; }

        public bool HasADB2C { get; set; }
        public bool IsFoundInADB2C { get; set; }
        public bool IsEmailFoundInADB2C { get; set; }
        public string ObjectId { get; set; }

        public string APIMName { get; set; }

    }
}
