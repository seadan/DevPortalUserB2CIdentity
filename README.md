# DevPortalUserB2CIdentity

Hello, this project is intended to help to integrate how users integrate to Azure APIM Developer Portal through Azure AD B2C.

In the code you will find how to move users from a Basic authentication to a Azure AD B2C authentication by:

    1) Creating the user in Azure AD B2C
    2) Adding the B2C identity to the user (adding the Object Id)
   
Before diving into those methods, we need first to enable the Azure AD B2C authentication in Developer portal, for that, this reference has all the information needed: https://docs.microsoft.com/en-us/azure/api-management/api-management-howto-aad-b2c

For "Creating the user in Azure AD B2C" you can call the following method:APIMUserNormalization.Services.CreateUserFromAPIMToAADB2C
For "Adding the B2C identity to the user" you can call the following method: APIMUserNormalization.Services.UpdateUserObjectIdAsync


Now, in order to use this solution you'll need to do the following:

1) Create the App Configuration Service: https://docs.microsoft.com/en-us/azure/azure-app-configuration/overview
2) Make sure you have the following keys created:
    AADB2CAppId: This is the app register in B2C in order to have Graph API permissions
    AADB2CB2cExtensionAppClientId: This is given by a fix property in B2C
    AADB2CClientSecret: The secret of the app created
    AADB2CTenantId: Your tenant Id
    APIMApiManagementNames: The name of your API management, you can add more than one, separated by semicolon
    APIMClientID: The service principal created to use your API Management Rest API
    APIMClientsecret: The service principal key
    APIMPostURL: Post URL to call API Management: https://login.microsoftonline.com/{tenant}/oauth2/token
    APIMResource: https://management.azure.com/
    APIMResourceGroups: The resource group of your API Management, you can add more than one, separated by semicolon
    APIMSubscriptionIds: Your subscription ID, you can add more than one, separated by semicolon
    APIMTenantId: Your APIM tenant Ids, you can add more than one, separated by semicolon
3) Add the string to the Configuration Service in your code: 
    This should be added to the following part of your class APIMUserNormalization.Models.AppSettingsFile
    Change this part of your code: builder.AddAzureAppConfiguration("Endpoint=https://usernormalizationconfiguration.azconfig.io;Id=Do9P-l6-s0:FCswtGBYgfMD7TNh+fMi;Secret=Veq5LbBx.....");
4) You'll have to choose which project to run:
  a) ApimUserConsole: Run this project to run in a console approach.
  b) APIMUserNormalization: Run this project to run in an Azure Service.
  

Thanks.
