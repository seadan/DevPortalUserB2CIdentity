using APIMUserNormalization.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace APIMUserNormalization.Models
{
    public class UserNormalization
    {

        public UserNormalization()
        {
            UsersStatus = new ArrayList();
            UniqueIdS = 0;
            IsValidated = false;
            IsCrossValidated = false;
        }

        public string Email { get; set; }
        public bool IsNormilized { get; set; }

        public ArrayList UsersStatus { get; set; }

        public bool IsValidated { get; set; }

        public bool IsCrossValidated { get; set; }

        public int ApimsFound { get; set; }

        public int UniqueIdS { get; set; }

        public void AddUserNormalizationStatus(UserNormalizationStatus user)
        {
            foreach (UserNormalizationStatus iterUser in UsersStatus)
            {
                if (user.APIMName.Equals(iterUser.APIMName))
                {
                    iterUser.ExistsInAPIM = user.ExistsInAPIM;
                    iterUser.HasADB2C = user.HasADB2C;
                    iterUser.IsEmailFoundInADB2C = user.IsEmailFoundInADB2C;
                    iterUser.IsFoundInADB2C = user.IsFoundInADB2C;
                    iterUser.ObjectId = user.ObjectId;
                }
            }
            UsersStatus.Add(user);
        }

        public void validateUser()
        {
            bool tempStatus = true;
            string tempObjectId = string.Empty;

            foreach (UserNormalizationStatus userIter in UsersStatus)
            {
                if (UniqueIdS == 0)
                {
                    if (userIter.ObjectId != null)
                    {
                        UniqueIdS = 1;
                        if (!userIter.ObjectId.Equals(""))
                        {
                            tempObjectId = userIter.ObjectId;
                        }

                    }
                }
                if (userIter.ObjectId != null && !userIter.ObjectId.Equals(tempObjectId))
                {
                    UniqueIdS++;
                }
                tempObjectId = userIter.ObjectId;

                if (userIter.HasADB2C && userIter.IsFoundInADB2C && userIter.IsEmailFoundInADB2C && tempStatus)
                {
                    tempStatus = true;
                }
                else
                {
                    tempStatus = false;
                }
            }
            IsNormilized = tempStatus && (UniqueIdS == 1);
            IsValidated = true;
        }

        public void crossValidateUser(APIMService[] apims)
        {
            ApimsFound = 0;
            if (!IsValidated)
            {
                validateUser();
            }
            int index = 0;
            foreach (var apim in apims)
            {
                bool found = false;
                foreach (UserNormalizationStatus userIter in UsersStatus)
                {
                    if (apim.APIMServiceName.Equals(userIter.APIMName))
                    {
                        found = true;
                        ApimsFound = ApimsFound + 1;
                    }
                }
                if (!found)
                {
                    UserNormalizationStatus newUNS = new UserNormalizationStatus { APIMName = apim.APIMServiceName, ExistsInAPIM = false, HasADB2C = false, IsEmailFoundInADB2C = false, IsFoundInADB2C = false, ObjectId = null };
                    UsersStatus.Insert(index, newUNS);
                    validateUser();
                }
                index++;
            }
            IsCrossValidated = true;
        }

    }
}
