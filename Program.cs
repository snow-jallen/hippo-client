using System;
using System.Threading.Tasks;

namespace HungryClient;

class Program
{
    static async Task Main(string[] args)
    {
        GameClient gameClient = null;

        if (GameClient.IsPreviousConnectionAvailable)
        {
            var prevConnection = GameClient.PreviousConnection;
            Console.WriteLine($"A previous connection exists to {prevConnection.ServerUrl} on {prevConnection.LastConnected}");
            var usePrevious = getBool("Would you like to use that connection?");
            if (usePrevious)
            {
                gameClient = GameClient.ReCreateClient(prevConnection);
            }
            else
            {
                if (getBool("Do you want to delete that saved connection info?"))
                {
                    GameClient.DeletePreviousConnection();
                }
            }
        }

        if (gameClient == null)
        {
            var serverAddress = args.Length > 0 ? args[0] : getServerAddress();
            var playerName = args.Length > 1 ? args[1] : getPlayerName();
            gameClient = await GameClient.CreateClientAsync(serverAddress, playerName);
        }

        Console.WriteLine("Congratulations, you are connected!\nNow use the arrow keys to eat all the things!\n(Press [Esc] to exit)");
        await makeMoves(gameClient);
    }

    private static bool getBool(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt + " (Y/N)");
            var input = Console.ReadLine().ToUpper();
            if (input.Contains("Y"))
                return true;
            if (input.Contains("N"))
                return false;
        }
    }

    private static string getServerAddress()
    {
        var defaultAddresses = new[]{
                "https://hungrygame.azurewebsites.net",
                "http://144.17.48.37",
                "http://localhost:5291"
            };
        var input = getString($"To which server would you like to connect?\n1) {defaultAddresses[0]}\n2) {defaultAddresses[1]} (GRSC 143 instructor PC)\n3) Other (you enter your own)", 1);
        switch (input)
        {
            case "0":
            case "1":
                var index = int.Parse(input) - 1;
                return defaultAddresses[index];
            default:
                return getString("Please enter the custom server address:", 4);
        }
    }

    private static string getString(string prompt, int minLength)
    {
        while (true)
        {
            Console.WriteLine("\n" + prompt);
            var input = Console.ReadLine();
            if (input.Length >= minLength)
            {
                return input;
            }
            Console.WriteLine($"Please enter at least {minLength} character{(minLength > 1 ? "s" : "")}");
        }
    }

    private static string getPlayerName()
    {
        return getString("Please enter your name", 3);
    }

    private static async Task makeMoves(GameClient gameClient)
    {
        while (true)
        {
            var k = Console.ReadKey();
            switch (k.Key)
            {
                case ConsoleKey.LeftArrow:
                    await gameClient.MoveLeftAsync();
                    break;
                case ConsoleKey.RightArrow:
                    await gameClient.MoveRightAsync();
                    break;
                case ConsoleKey.UpArrow:
                    await gameClient.MoveUpAsync();
                    break;
                case ConsoleKey.DownArrow:
                    await gameClient.MoveDownAsync();
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }
}
