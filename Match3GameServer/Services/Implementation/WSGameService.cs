using System.Net.WebSockets;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class WebsocketService : IWebsocketService
{
    private readonly ILogger<WebsocketService> _logger;
    private readonly ISessionService _sessionService;
    private readonly List<WebSocket> _connectedClients = new List<WebSocket>();

    public bool IsStarted { get; set; }

    public WebsocketService
    (
        ILogger<WebsocketService> logger,
        ISessionService sessionService
    )
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        _logger.LogInformation("Entered HandleWebSocketAsync method");

        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation($"{header.Key}: {header.Value}");
        }

        if (context.WebSockets.IsWebSocketRequest)
        {
            _logger.LogInformation("Request is a valid WebSocket request");

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

                _connectedClients.Add(webSocket);
                var playerId = _connectedClients.Count;
                _logger.LogInformation("Player {PlayerId} connected via WebSocket", playerId);
                
                await _sessionService.AddPlayerAsync(webSocket, playerId);
                
                await ListenForMessagesAsync(webSocket, playerId);
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

    private async Task ListenForMessagesAsync(WebSocket webSocket, int playerId)
    {
        var buffer = new byte[1024 * 10];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation("Player {PlayerId} sent message: {Message}", playerId, message);
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Player {PlayerId} disconnected.", playerId);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed",
                        CancellationToken.None);
                    _connectedClients.Remove(webSocket);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while receiving data from player {PlayerId}.", playerId);
        }
        finally
        {
            if (_connectedClients.Contains(webSocket))
            {
                _connectedClients.Remove(webSocket);
            }

            if (webSocket.State != WebSocketState.Closed)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "An error occurred",
                    CancellationToken.None);
            }
        }
    }
}