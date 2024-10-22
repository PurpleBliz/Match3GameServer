using System.Collections.Concurrent;
using System.Net.WebSockets;
using Match3GameServer.Messages;
using Match3GameServer.Messages.Base;
using Match3GameServer.Messages.Client;
using Match3GameServer.Messages.Server;
using Match3GameServer.Models;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class WebSocketService : IWebsocketService
{
    public List<WebSocketClient> ConnectedClients => _connectedClients.Values.ToList();
    
    private readonly ILogger<WebSocketService> _logger;
    private readonly ConcurrentDictionary<int, WebSocketClient> _connectedClients;

    public bool IsStarted { get; set; }
    public event Action<WebSocketClient>? OnClientConnected;
    public event Action<WebSocketClient>? OnClientDisconnected;
    public event Action<WebSocketClient>? OnClientVerifered;

    protected readonly byte[] Buffer = new byte[10 * 1024];
    
    private bool _disposed;

    public WebSocketService
    (
        IHostApplicationLifetime applicationLifetime,
        ILogger<WebSocketService> logger
    )
    {
        _logger = logger;

        _connectedClients = new();
        
        MessageHandlers.RegisterHandler<PlayerInitMessage>(UpgradeClient);
        
        applicationLifetime.ApplicationStopping.Register(OnApplicationQuit);
    }

    private async void OnApplicationQuit()
    {
        _disposed = true;
        
        await CloseServer();
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            try
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                if (webSocket == null || webSocket.CloseStatus.HasValue)
                {
                    _logger.LogWarning(
                        "Failed to accept WebSocket connection or the connection was immediately closed.");

                    context.Response.StatusCode = 400;
                    return;
                }

                var playerId = _connectedClients.Count + 1;

                WebSocketClient client = new(webSocket, playerId);

                _connectedClients.TryAdd(playerId, client);

                OnClientConnected?.Invoke(client);

                _logger.LogInformation("Player {PlayerId} connected via WebSocket", playerId);

                await ListenForMessagesAsync(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling WebSocket connection.");

                context.Response.StatusCode = 500;
            }
        }
        else
        {
            _logger.LogWarning("Received a non-WebSocket request.");

            context.Response.StatusCode = 400;
        }
    }

    private async Task ListenForMessagesAsync(WebSocketClient client)
    {
        try
        {
            while (client.Connection.State == WebSocketState.Open)
            {
                var result =
                    await client.Connection.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var jsonMessage = System.Text.Encoding.UTF8.GetString(Buffer, 0, result.Count);

                    _logger.LogInformation("Player {PlayerId} sent message: {Message}", client.PlayerId, jsonMessage);

                    MessageHandlers.InvokeHandler(client, jsonMessage);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Player {PlayerId} disconnected.", client.PlayerId);
                    await client.Connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed",
                        CancellationToken.None);

                    _connectedClients.TryRemove(client.InternalId, out _);

                    return;
                }
            }
        }
        catch (Exception ex)
        {
            if (client.Connection.State != WebSocketState.Closed && !_disposed)
            {
                _logger.LogError(ex, "Error occurred while receiving data from player {PlayerId}.", client.InternalId);
            }
        }
        finally
        {
            if (_connectedClients.ContainsKey(client.InternalId))
            {
                _connectedClients.TryRemove(client.InternalId, out _);

                OnClientDisconnected?.Invoke(client);
            }

            if (client.Connection.State != WebSocketState.Closed)
            {
                await client.Connection.CloseAsync(WebSocketCloseStatus.InternalServerError, "An error occurred",
                    CancellationToken.None);
            }
        }
    }

    public async Task CloseServer()
    {
        _logger.LogInformation("The procedure for disconnecting clients has begun");
        
        foreach (var client in _connectedClients.Values)
        {
            await client.Connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down",
                CancellationToken.None);
            
            _logger.LogInformation($"The client[{client.InternalId}] has been disconnected");
        }
        
        _connectedClients.Clear();
        
        _logger.LogInformation("The procedure for disconnecting clients has been completed");
    }

    private async void UpgradeClient(WebSocketClient client, PlayerInitMessage message)
    {
        _logger.LogInformation($"UpgradeClient[{client.InternalId}] PlayerId: {message.PlayerId} SID: {message.SId}");
        
        client.Verification(message.PlayerId, message.SId);
        
        OnClientVerifered?.Invoke(client);

        await client.SendMessage(new InitMessage
        {
            Text = $"Hello {message.PlayerId}"
        });
    }
}