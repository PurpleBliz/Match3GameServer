using System.Net.WebSockets;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class WSGameService : IGameService
{
    private readonly ILogger<WSGameService> _logger;
    private readonly ISessionService _sessionService;
    private readonly List<WebSocket> _connectedClients = new List<WebSocket>();

    public bool IsStarted { get; set; }

    public WSGameService
    (
        ILogger<WSGameService> logger,
        ISessionService sessionService
    )
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _connectedClients.Add(webSocket);

            var playerId = _connectedClients.Count;
            _logger.LogInformation("Player {PlayerId} connected via WebSocket", playerId);
            
            await _sessionService.AddPlayerAsync(webSocket, playerId);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
}