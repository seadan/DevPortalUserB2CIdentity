using APIMUserNormalization.ConsoleView;
using APIMUserNormalization.Models;
using System.Threading.Tasks;

namespace ApimUserConsole
{
    class Program
    {
        static AppSettings config = AppSettingsFile.ReadFromJsonFile();

        static async Task Main(string[] args)
        {
            UserConsole userConsole = new UserConsole();

            bool breakFlag = false;

            while (!breakFlag)
            {
                breakFlag = await userConsole.PrintMainMenu();
                //var subs = await subsMigrationService.ListAPIMSubscriptions();
                //await subsMigrationService.NormalizeAllSubscriptionsAsync();
                //breakFlag = true;
            }

        }

    }
}
