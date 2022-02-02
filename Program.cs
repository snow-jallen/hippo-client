using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HippoClient
{
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
                if(usePrevious)
                {
                    gameClient = GameClient.ReCreateClient(prevConnection);
                }
                else
                {
                    if(getBool("Do you want to delete that saved connection info?"))
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

            Console.WriteLine("Congratulations, you are connected!\nNow use the arrow keys to eat all the things!");
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
                }
            }
        }
    }

    public class GameClient
    {
        private const string PreviousConnectionFile = "previousConnection.txt";
        private readonly string serverAddress;
        private readonly string playerName;
        HttpClient httpClient = new HttpClient();

        private GameClient(string serverAddress, string playerName)
        {
            this.serverAddress = serverAddress;
            this.playerName = playerName;
        }

        public string Token { get; private set; }

        public async Task MoveLeftAsync() => await httpClient.GetAsync($"{serverAddress}/move/left?token={Token}");
        public async Task MoveRightAsync() => await httpClient.GetAsync($"{serverAddress}/move/right?token={Token}");
        public async Task MoveUpAsync() => await httpClient.GetAsync($"{serverAddress}/move/up?token={Token}");
        public async Task MoveDownAsync() => await httpClient.GetAsync($"{serverAddress}/move/down?token={Token}");

        public static async Task<GameClient> CreateClientAsync(string serverAddress, string playerName)
        {
            var client = new GameClient(serverAddress, playerName);

            try
            {
                var response = await client.httpClient.GetStringAsync($"{serverAddress}/join?userName={playerName}");
                client.Token = response;

                saveConnectionInfo(serverAddress, response);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to connect to server.", ex);
            }

            return client;
        }

        internal static GameClient ReCreateClient(PreviousConnection prevConnection)
        {
            var client = new GameClient(prevConnection.ServerUrl, "undetermined");
            client.Token = prevConnection.Token;
            return client;
        }

        public static bool IsPreviousConnectionAvailable => File.Exists(PreviousConnectionFile);

        public static PreviousConnection PreviousConnection
        {
            get
            {
                if (File.Exists(PreviousConnectionFile))
                {
                    var json = File.ReadAllText(PreviousConnectionFile);
                    var connectionInfo = System.Text.Json.JsonSerializer.Deserialize<PreviousConnection>(json);
                    return connectionInfo;
                }
                return null;
            }
        }

        private static void saveConnectionInfo(string serverAddress, string response)
        {
            var connectionInfo = new PreviousConnection(serverAddress, response, DateTime.Now);
            var json = System.Text.Json.JsonSerializer.Serialize(connectionInfo);
            File.WriteAllText(PreviousConnectionFile, json);
        }

        internal static void DeletePreviousConnection()
        {
            if (File.Exists(PreviousConnectionFile))
            {
                File.Delete(PreviousConnectionFile);
            }
        }
    }

    public record PreviousConnection(string ServerUrl, string Token, DateTime LastConnected);
}
