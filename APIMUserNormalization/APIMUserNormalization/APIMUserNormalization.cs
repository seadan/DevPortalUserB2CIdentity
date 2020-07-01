using System;
using System.Threading.Tasks;
using APIMUserNormalization.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace APIMUserNormalization
{
    public static class APIMUserNormalization
    {
        
        [FunctionName("APIMUserNormalization")]
        public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger logger)
        {
            MigrationService migration = new MigrationService();
            migration.PrintConfig();
            await migration.MigrationServiceSetupAsync();
            await migration.NormalizeAllUsersAsync();
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
