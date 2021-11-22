using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HippoClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverAddress = args.Length > 0 ? args[0] : getServerAddress();
            var playerName = args.Length > 1 ? args[1] : getPlayerName();
            var gameClient = await GameClient.CreateClientAsync(serverAddress, playerName);

            Console.WriteLine("Congratulations, you are connected!\nNow use the arrow keys to eat all the things!");
            await makeMoves(gameClient);
        }

        private static string getServerAddress()
        {
            var defaultAddresses = new[]{
                "https://hungry-hungry-hippos.herokuapp.com",
                "http://144.17.s48.37",
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
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to connect to server.", ex);
            }

            return client;
        }
    }
}
