using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HungryClient;

public record PreviousConnection(string ServerUrl, string Token, DateTime LastConnected);
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
