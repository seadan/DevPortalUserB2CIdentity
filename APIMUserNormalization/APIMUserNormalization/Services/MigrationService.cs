using APIMUserNormalization.Models;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APIMUserNormalization.Services
{
    public class MigrationService
    {
        UserCollection[] apimUserCollections;
        IList<User> adB2CUsersAll;
        APIMService[] apims;
        ArrayList userNormalizationList = new ArrayList();

        static AppSettings config;
        bool userSetup = false;


        public MigrationService()
        {

            adB2CUsersAll = new List<User>();
            config = AppSettingsFile.ReadFromJsonFile();

            var subscriptions = config.APIMSubscriptionIds == null ? new String[] { } : config.APIMSubscriptionIds.Split(";");
            var serviceNames = config.APIMApiManagementNames == null ? new String[] { } : config.APIMApiManagementNames.Split(";");
            var resourceGroups = config.APIMResourceGroups == null ? new String[] { } : config.APIMResourceGroups.Split(";");


            if (subscriptions.Length > 0 && subscriptions.Length.Equals(serviceNames.Length) && subscriptions.Length.Equals(resourceGroups.Length))
            {
                apims = new APIMService[serviceNames.Length];
                apimUserCollections = new UserCollection[serviceNames.Length];


                for (int i = 0; i < serviceNames.Length; i++)
                {
                    apims[i] = new APIMService(serviceNames[i], resourceGroups[i], config.APIMTenantId, subscriptions[i]);
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error on Configuration:");
                Console.WriteLine(" API Mgmt Services      : " + serviceNames.Length);
                Console.WriteLine(" API Mgmt RGs           : " + resourceGroups.Length);
                Console.WriteLine(" API Mgmt Subscrpitions : " + subscriptions.Length);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }


        public async Task<bool> PrintMainMenu()
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("API Mgmt and AD B2C Environments list:");
            Console.WriteLine("====================");

            await EnvironmentCheckAsync();
            userNormalizationList.Clear();
            await SetupUserCollections();
            EvaluateUserNormalization();
            PrintUserNormalizationStatus();


            Console.WriteLine("Command  Description");
            Console.WriteLine("====================");


            Console.ForegroundColor = userSetup ? Console.ForegroundColor = ConsoleColor.Gray : Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[A]      Create List of Users");
            if (userSetup)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[B]      Manage Users");
                Console.WriteLine("[C]      Normalize all Users");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[E]      Print Config Attributes");
            Console.WriteLine("[X]      Exit Program Console");
            Console.WriteLine("-------------------------");
            string option = Console.ReadLine();
            bool leaveLoop = false;
            option = option.ToUpper();

            switch (option)
            {
                case "A":
                    Console.WriteLine("This process will take some time, please wait...");
                    await SetupUserCollections();
                    Console.WriteLine("Done...");
                    break;
                case "B":
                    await ManageUsersAsync();
                    Console.WriteLine("Done...");
                    break;
                case "C":
                    await NormalizeAllUsersAsync();
                    Console.WriteLine("Done...");
                    break;
                case "E":
                    PrintConfig();
                    Console.WriteLine("Done...");
                    break;
                case "X":
                    leaveLoop = true;
                    break;
                default:
                    Console.WriteLine("Option not recognized");
                    break;
            }
            Thread.Sleep(2000);
            if (!leaveLoop)
            {
                Console.Write("Press any key to continue...");
                Console.ReadLine();
            }

            return leaveLoop;

        }

        internal ArrayList getUserNormalizationList()
        {
            return userNormalizationList;
        }

        public async Task MigrationServiceSetupAsync()
        {
            await EnvironmentCheckAsync();
            userNormalizationList.Clear();
            await SetupUserCollections();
            EvaluateUserNormalization();
            PrintUserNormalizationStatus();
        }

        public async Task ServiceBootstraping()
        {
            await EnvironmentCheckAsync();
            userNormalizationList.Clear();
            await SetupUserCollections();
            EvaluateUserNormalization();
            PrintUserNormalizationStatus();

        }


        public async Task NormalizeAllUsersAsync()
        {
            foreach (UserNormalization un in userNormalizationList)
            {
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Will normalize " + un.Email);
                await NormalizeUserAsync(un);
                Console.WriteLine(un.Email + " normalized");
                Console.WriteLine("");
            }

        }

        private void PrintUserNormalizationStatus()
        {
            int row = 1;
            string s = string.Empty;
            string sn = "#";
            string sa = "User Email";
            string sb = "[APIMs]";
            string sc = "[Diff Object IdS]";
            string sg = "[Exists in APIM?]";
            string sd = "[AD B2C Enabled]";
            string se = "[Object Id Match]";
            string sf = "[AD B2C Email Match]";

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Evaluation per user");
            Console.WriteLine("{6,2}.- {0,-22} {1,5} {2,5} | {7,15}{3,15}{4,15}{5,15} |", sa, sb, sc, sd, se, sf, sn, sg);
            Console.WriteLine();

            Console.WriteLine("{2,2}.- {0,-40} {2,2}, {2,2} | {1,3}{1,3}{1,3}    |", sa, s, sn);
            foreach (UserNormalization un in userNormalizationList)
            {
                if (!un.IsValidated)
                {
                    un.validateUser();
                }
                if (!un.IsCrossValidated)
                {
                    un.crossValidateUser(apims);
                }

                if (un.IsNormilized)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }


                string s1 = un.Email;
                string s2 = un.ApimsFound.ToString();
                string s3 = un.UniqueIdS.ToString();
                string s4 = string.Empty;
                string s5 = string.Empty;
                string s6 = string.Empty;
                string s7 = string.Empty;
                Console.Write("{6,2}.- {0,-40} {1,2}, {2,2} ", s1, s2, s3, s4, s5, s6, row);
                foreach (UserNormalizationStatus uns in un.UsersStatus)
                {
                    s7 = uns.ExistsInAPIM ? "[x]" : "[_]";
                    s4 = uns.HasADB2C ? "[x]" : "[_]";
                    s5 = uns.IsFoundInADB2C ? "[x]" : "[_]";
                    s6 = uns.IsEmailFoundInADB2C ? "[x]" : "[_]";
                    Console.Write("| {7,3}{3,3}{4,3}{5,3} |", s1, s2, s3, s4, s5, s6, row, s7);
                }

                //Console.WriteLine("{6,2}.- {0,-40} {1,2}, {2,2} | {3,3}{4,3}{5,3} |", s1, s2, s3, s4, s5, s6, row);
                Console.WriteLine("");
                row++;
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();
        }

        public async Task EnvironmentCheckAsync()
        {
            await RePopulateAPIMUserCollection();
            adB2CUsersAll = await GetAADB2CUsersAsync();
            Console.WriteLine(" For AD B2C there are " + adB2CUsersAll.Count + " users");
            Console.WriteLine();
        }

        public async Task RePopulateAPIMUserCollection()
        {
            int i = 0;
            foreach (var apim in apims)
            {
                var userCollection = await apim.GetUsersFromAPIM();
                apimUserCollections[i] = userCollection;
                i++;
                int count = userCollection == null ? 0 : userCollection.count;
                Console.WriteLine(" For APIM " + apim.APIMServiceName + " there are " + count + " users");
            }

        }

        public void PrintConfig()
        {
            Console.WriteLine();
            Console.WriteLine("API Mgmt Tenant Id   : " + config.APIMTenantId);

            Console.WriteLine("API Instances        : " + config.APIMApiManagementNames.Split(";").Length);
            Console.WriteLine("Resource Groups      : " + config.APIMResourceGroups.Split(";").Length);
            Console.WriteLine("API Mgmt Client Id   : " + config.APIMClientID);
            Console.WriteLine("Azure AD B2C Tenant  : " + config.AADB2CTenantId);
            Console.WriteLine("Azure AD Client Id   : " + config.AADB2CAppId);
        }

        public async Task SetupUserCollections()
        {
            //userNormalizationList.Clear();
            int iterIdx = 0;

            foreach (var userCollection in apimUserCollections)
            {

                foreach (var user in userCollection.value)
                {

                    bool hasADB2C = false;
                    bool isFoundInADB2C = false;
                    bool isEmailFoundInADB2C = false;
                    string objectId = string.Empty;
                    string email = user.Properties.Email;

                    var uns = new UserNormalizationStatus();

                    foreach (var identity in user.Properties.Identities)
                    {
                        if (identity.Provider.Equals("AadB2C"))
                        {
                            hasADB2C = true;

                            var b2cUser = await GetAADB2CUserById(identity.Id);
                            if (b2cUser == null)
                            {
                                isFoundInADB2C = false;
                            }
                            else if (DoesB2CUserHasThisEmail(b2cUser, user.Properties.Email))
                            {
                                isFoundInADB2C = true;
                                isEmailFoundInADB2C = true;
                                objectId = identity.Id;
                            }
                            else
                            {
                                isFoundInADB2C = true;
                                isEmailFoundInADB2C = false;
                            }
                            break;
                        }

                    }

                    uns.APIMName = "";
                    uns.HasADB2C = hasADB2C;
                    uns.IsEmailFoundInADB2C = isEmailFoundInADB2C;
                    uns.IsFoundInADB2C = isFoundInADB2C;
                    uns.ObjectId = objectId;
                    uns.ExistsInAPIM = true;
                    if (iterIdx < apims.Length) uns.APIMName = apims[iterIdx].APIMServiceName;

                    UserNormalization un = FindUserNormalizationByEmail(userNormalizationList, email);
                    if (un != null)
                    {
                        un.AddUserNormalizationStatus(uns);
                    }
                    else
                    {
                        un = new UserNormalization();
                        un.Email = email;
                        un.AddUserNormalizationStatus(uns);
                        userNormalizationList.Add(un);
                    }
                }
                iterIdx++;
            }

            userSetup = true;

        }

        private UserNormalization FindUserNormalizationByEmail(ArrayList userNormalizationList, string email)
        {
            foreach (UserNormalization un in userNormalizationList)
            {
                if (un.Email.ToUpper().Equals(email.ToUpper()))
                {
                    return un;
                }
            }
            return null;
        }

        private bool DoesB2CUserHasThisEmail(User b2cUser, string email)
        {

            foreach (var item in b2cUser.Identities)
            {
                if (item.SignInType == "emailAddress")
                {
                    return email.Equals(item.IssuerAssignedId);
                }
            }
            return false;
        }

        private void EvaluateUserNormalization()
        {
            foreach (UserNormalization un in userNormalizationList)
            {
                un.validateUser();
            }
            foreach (UserNormalization un in userNormalizationList)
            {
                un.crossValidateUser(apims);
            }
        }

        private async Task ManageUsersAsync()
        {
            Console.WriteLine("");
            Console.WriteLine("====================");
            Console.WriteLine("Choose your user #");
            string idx = Console.ReadLine().ToUpper();
            int userIdx = int.Parse(idx);
            var un = (UserNormalization)userNormalizationList[userIdx - 1];
            Console.WriteLine("User Email: " + un.Email);

            Console.Write("    Is user normilized?: ");
            if (!un.IsNormilized)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(un.IsNormilized);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    Found in " + un.ApimsFound + " APIM Services");
            Console.WriteLine("    User with " + un.UniqueIdS + " unique Object IdS");
            Console.WriteLine();
            Console.WriteLine("Normalization Plan:");

            string s4 = string.Empty;
            string s5 = string.Empty;
            string s6 = string.Empty;
            foreach (UserNormalizationStatus uns in un.UsersStatus)
            {
                Console.WriteLine("    APIM: " + uns.APIMName + ":");

                if (!uns.ExistsInAPIM) Console.WriteLine("        [APIM] - Create User");
                if (!uns.IsFoundInADB2C) Console.WriteLine("        [ADB2C]- Create User");
                if (!uns.HasADB2C) Console.WriteLine("        [APIM] - Add ADB2C Identity");
                if (!uns.IsEmailFoundInADB2C) Console.WriteLine("        [ADB2C] - Update Properties");
            }

            Console.WriteLine();
            Console.WriteLine("Normalize User? (Y/N)");
            string yn = Console.ReadLine().ToUpper();
            if (yn.ToUpper().Equals("Y"))
            {
                await NormalizeUserAsync(un);
            }

        }

        private void PrintListOfUsers(IList<User> adB2CUsers, ConsoleColor color)
        {
            int i = 1;
            if (adB2CUsers != null)
            {
                foreach (var user in adB2CUsers)
                {
                    Console.ForegroundColor = color;
                    string output = i + "- UserId:" + user.Id + "; User GivenName:" + user.GivenName + "; User Surname:" + user.Surname + "; User State:" + user.State;
                    if (user.Id != null)
                    {
                        foreach (var identity in user.Identities)
                        {
                            output += "; Identity User: " + identity.Issuer + "; Identity SignIn Type: " + identity.SignInType + "; Identity Assigned Id:" + identity.IssuerAssignedId;
                            Console.WriteLine(output);
                        }
                    }
                    i++;
                }
            }
            Console.ResetColor();
        }

        private async Task PrintListOfUsersAsync(UserCollection apimUsers, ConsoleColor color)
        {
            int i = 1;
            if (apimUsers != null && apimUsers.value != null)
            {
                foreach (var user in apimUsers.value)
                {
                    Console.ForegroundColor = color;
                    string output = i + "- " + user.Name + "; " + user.Properties.FirstName + "; " + user.Properties.LastName + "; " + user.Properties.State;
                    if (user.Properties.Identities != null)
                    {
                        foreach (var identity in user.Properties.Identities)
                        {
                            output += "; " + identity.Provider + "; " + identity.Id;
                            Console.WriteLine(output);
                        }
                    }
                    i++;
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Command Description");
            Console.WriteLine("====================");
            Console.WriteLine("[#] - To Migrate User / Update Object Id");
            Console.WriteLine("[X] - To go back to main menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            string idx = Console.ReadLine().ToUpper();
            if (!idx.Equals("X"))
            {
                int iIdx = int.Parse(idx);
                if (iIdx > 0 && iIdx < apimUsers.value.Length + 1)
                {

                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("Command Description");
                    Console.WriteLine("====================");
                    Console.WriteLine("[M] - To Migrate User");
                    Console.WriteLine("[U] - Update Object Id");
                    Console.WriteLine("[X] - To go back to main menu");
                    Console.WriteLine("-------------------------");
                    Console.WriteLine();

                    string idx2 = Console.ReadLine().ToUpper();
                    if (idx2.Equals("M"))
                    {
                        APIMService apimClient = apims[0];
                        Console.WriteLine("Will migrate user: " + iIdx + " - " + apimUsers.value[iIdx - 1].Properties.FirstName);

                        var addedUser = await CreateUserInAzureB2C(apimUsers.value[iIdx - 1]);
                        Console.WriteLine("   User Recently Added: " + addedUser.DisplayName);

                        await apimClient.UpdateUserObjectIdAsync(apimUsers.value[iIdx - 1], addedUser.Id);
                    }
                    else if (idx2.Equals("U"))
                    {
                        APIMService apimClient = apims[0];
                        Console.WriteLine("Will Update Object Id for user: " + iIdx + " - " + apimUsers.value[iIdx - 1].Properties.FirstName);

                        var b2CUser = GetUserInAzureB2CWithEmail(adB2CUsersAll, apimUsers.value[iIdx - 1].Properties.Email);

                        await apimClient.UpdateUserObjectIdAsync(apimUsers.value[iIdx - 1], b2CUser.Id);
                    }
                }

            }
        }


        private async Task CreateUsersInAzureB2CAndSyncObjectId(UserCollection apimUsersToMigrateToB2C)
        {
            APIMService apimClient = apims[0];

            //If we have users with AADB2C identity that do not exists on B2C then create them:
            if (apimUsersToMigrateToB2C != null && apimUsersToMigrateToB2C.value != null)
            {
                foreach (var userToMigrate in apimUsersToMigrateToB2C.value)
                {
                    Console.WriteLine("   [ADB2C] - Creating User: " + userToMigrate.Properties.Email);
                    var addedUser = await CreateUserInAzureB2C(userToMigrate);
                    if (addedUser != null)
                    {
                        Console.WriteLine("   [ADB2C] - User Created: " + addedUser.DisplayName);
                    }

                    Console.WriteLine("   [APIM] - Updating Object: " + userToMigrate.Properties.Email + ", " + addedUser.Id);
                    string result = await apimClient.UpdateUserObjectIdAsync(userToMigrate, addedUser.Id);
                    Console.WriteLine("   [APIM] - Object Id update result: " + result);
                }

            }
        }

        private static User GetUserInAzureB2CWithEmail(IList<User> b2cUsers, string email)
        {
            if (b2cUsers != null)
            {
                if (b2cUsers.Count > 0)
                {
                    foreach (var b2cUser in b2cUsers)
                    {
                        foreach (var item in b2cUser.Identities)
                        {
                            if (item.SignInType == "emailAddress")
                            {
                                if (email.Equals(item.IssuerAssignedId))
                                {
                                    return b2cUser;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static async Task<User> CreateUserInAzureB2C(UserContract user)
        {

            // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(config.AADB2CAppId)
                .WithTenantId(config.AADB2CTenantId)
                .WithClientSecret(config.AADB2CClientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            var addedUser = await UserService.CreateUserFromAPIMToAADB2C(graphClient, config.AADB2CB2cExtensionAppClientId, config.AADB2CTenantId, user);
            return addedUser;
        }


        private static async Task<IList<User>> GetAADB2CUsersAsync()
        {

            // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(config.AADB2CAppId)
                .WithTenantId(config.AADB2CTenantId)
                .WithClientSecret(config.AADB2CClientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            return await UserService.GetAADB2CUsers(graphClient);
        }

        public async Task NormalizeUserAsync(UserNormalization un)
        {
            if (un.IsNormilized)
            {
                return;
            }

            Console.WriteLine("");
            Console.WriteLine("Starting user normalization...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");

            Console.WriteLine("  1) Create user in all APIMs");
            int iter1 = 0;
            UserContract userContract = GetUserContractFor(un.Email);
            if (userContract != null && !IsAzureIdentity(userContract))
            {
                bool userCreated = false;
                foreach (UserNormalizationStatus uns in un.UsersStatus)
                {

                    if (!uns.ExistsInAPIM)
                    {
                        Console.WriteLine("        Creating user in service: " + uns.APIMName);
                        APIMService apimClient = apims[iter1];
                        string response = await apimClient.CreateUserInApim(userContract);
                        Console.WriteLine("        Result: " + response.Substring(0, 30) + (response.Length > 30 ? "..." : ""));
                        Console.WriteLine("");
                        userCreated = true;
                    }
                    iter1++;
                }
                if (userCreated)
                {
                    await RePopulateAPIMUserCollection();
                }

            }
            else
            {
                Console.WriteLine("      User " + userContract.Properties.Email + " not suitable for normalization");
            }

            Console.WriteLine("  2) Create user in ADB2C if needed");
            int iter2 = 0;
            string objectId = string.Empty;


            var b2CUser = GetUserInAzureB2CWithEmail(adB2CUsersAll, un.Email);

            if (b2CUser == null)
            {
                if (userContract != null && !IsAzureIdentity(userContract))
                {
                    Console.WriteLine("        Creating user ADB2C: " + userContract.Properties.Email);
                    b2CUser = await CreateUserInAzureB2C(userContract);
                    if (b2CUser != null)
                    {
                        Console.WriteLine("        User Created: " + b2CUser.DisplayName + " with Object Id " + b2CUser.Id);
                    }
                }

            }

            Console.WriteLine("  3) Update Users Identities in APIM");

            foreach (UserNormalizationStatus uns in un.UsersStatus)
            {
                if (userContract != null && !IsAzureIdentity(userContract))
                {
                    if (b2CUser != null)
                    {
                        APIMService apimClient = apims[iter2];
                        Console.WriteLine("        Updating Object Id in API Management: " + userContract.Properties.Email);

                        var user = GetUserContractFor(uns.APIMName, un.Email);
                        if (user != null)
                        {
                            string result = await apimClient.UpdateUserObjectIdAsync(user, b2CUser.Id);
                            int idx = result == null ? 0 : (result.Length <= 30 ? result.Length : 30);
                            Console.WriteLine("        User Update result: " + result.Substring(0, idx) + (result.Length > 30 ? "..." : ""));
                        }
                    }
                    else
                    {
                        Console.WriteLine("        ERROR While creating the user in B2C");
                    }
                }
                else
                {
                    Console.WriteLine("      User " + userContract.Name + " not suitable for normalization");
                }
                iter2++;
            }
        }



        private bool IsAzureIdentity(UserContract userContract)
        {
            if (userContract.Properties.Identities != null)
            {
                foreach (var identity in userContract.Properties.Identities)
                {
                    if (identity.Provider.Equals("Azure"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private UserContract GetUserContractFor(string email)
        {
            foreach (UserCollection apimUserCollection in apimUserCollections)
            {
                foreach (UserContract userContract in apimUserCollection.value)
                {
                    if (userContract.Properties.Email.Equals(email))
                    {
                        return userContract;
                    }
                }
            }
            return null;
        }

        public UserContract GetUserContractFor(string apimName, string email)
        {
            int i = 0;
            foreach (var apim in apims)
            {
                if (apim.APIMServiceName.Equals(apimName))
                {
                    foreach (var user in apimUserCollections[i].value)
                    {
                        if (user.Properties.Email.Equals(email))
                        {
                            return user;
                        }
                    }
                }
                i++;
            }
            return null;
        }


        private static Task<User> GetAADB2CUserById(string id)
        {
            // Initialize the client credential auth provider
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(config.AADB2CAppId)
                .WithTenantId(config.AADB2CTenantId)
                .WithClientSecret(config.AADB2CClientSecret)
                .Build();
            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            // Set up the Microsoft Graph service client with client credentials
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);

            return UserService.GetUserById(graphClient, id);
        }

    }
}
