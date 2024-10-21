using System.Net.WebSockets;
using Match3GameServer.Messages;
using Match3GameServer.Messages.Base;
using Match3GameServer.Messages.Responses;
using Match3GameServer.Models;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class WebSocketService : IWebsocketService
{
    public List<WebSocketClient> ConnectedClients => _connectedClients;
    
    private readonly ILogger<WebSocketService> _logger;
    private readonly ISessionService _sessionService;
    private readonly List<WebSocketClient> _connectedClients;
    private readonly MessageHandlers _messageHandlers;

    public bool IsStarted { get; set; }
    public event Action<WebSocketClient>? OnClientConnected;
    public event Action<WebSocketClient>? OnClientDisconnected;
    
    protected readonly byte[] Buffer = new byte[1024 * 10];

    public WebSocketService
    (
        ILogger<WebSocketService> logger,
        ISessionService sessionService,
        MessageHandlers messageHandlers
    )
    {
        _sessionService = sessionService;
        _logger = logger;
        _messageHandlers = messageHandlers;

        _connectedClients = new();
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

                var playerId = _connectedClients.Count;

                WebSocketClient client = new(webSocket, playerId);

                _connectedClients.Add(client);

                OnClientConnected?.Invoke(client);

                _logger.LogInformation("Player {PlayerId} connected via WebSocket", playerId);

                await _sessionService.AddPlayerAsync(webSocket, playerId);

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

    public async Task SendToPlayer<T>(WebSocketClient client, T message) where T : WebSocketResponse
    {
        var (success, messageId) = _messageHandlers.GetIdByType<T>();

        if (!success)
        {
            _logger.LogError($"Message ID not found for type {typeof(T).Name}");
            
            return;
        }
        
        message.MessageId = messageId;
        
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
        var responseBuffer = System.Text.Encoding.UTF8.GetBytes(json);

        _logger.LogInformation($"Sending message to player: {json}");
        
        await client.Connection.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }


    private async Task ListenForMessagesAsync(WebSocketClient client)
    {
        _messageHandlers.RegisterHandler<PlayerInitMessage>(ActionInit);
        
        try
        {
            while (client.Connection.State == WebSocketState.Open)
            {
                var result = await client.Connection.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var jsonMessage = System.Text.Encoding.UTF8.GetString(Buffer, 0, result.Count);

                    _logger.LogInformation("Player {PlayerId} sent message: {Message}", client.PlayerId, jsonMessage);

                    _messageHandlers.InvokeHandler(client, jsonMessage);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Player {PlayerId} disconnected.", client.PlayerId);
                    await client.Connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed",
                        CancellationToken.None);
                    
                    _connectedClients.Remove(client);
                    
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while receiving data from player {PlayerId}.", client.PlayerId);
        }
        finally
        {
            if (_connectedClients.Contains(client))
            {
                _connectedClients.Remove(client);
                
                OnClientDisconnected?.Invoke(client);
            }

            if (client.Connection.State != WebSocketState.Closed)
            {
                await client.Connection.CloseAsync(WebSocketCloseStatus.InternalServerError, "An error occurred",
                    CancellationToken.None);
            }
        }
    }

    private async void ActionInit(WebSocketClient client, PlayerInitMessage message)
    {
        _logger.LogInformation($"[{message.MessageId}] PlayerId: {message.PlayerId} SID: {message.SId}");

        await SendToPlayer(client, new InitResponse
        {
            Text = "Hello"
        });
    }
}