using Match3GameServer.Models;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services;

public sealed class RoomService : IHostedService
{
    private readonly IWebsocketService _websocketService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<RoomService> _logger;

    public RoomService(
        IWebsocketService websocketService,
        ISessionService sessionService,
        ILogger<RoomService> logger
    )
    {
        _websocketService = websocketService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Init();

        return Task.CompletedTask;
    }

    private void Init()
    {
        _websocketService.OnClientConnected += HandleOnClientConnected;
        _websocketService.OnClientDisconnected += HandleOnClientDisconnected;
        _websocketService.OnClientVerifered += HandleOnClientVerifered;
    }

    private async void HandleOnClientVerifered(WebSocketClient client)
    {
        await _sessionService.AddPlayerAsync(client);
    }

    private async void HandleOnClientDisconnected(WebSocketClient client)
    {
        
    }

    private async void HandleOnClientConnected(WebSocketClient client)
    {
        
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _websocketService.OnClientConnected -= HandleOnClientConnected;
        _websocketService.OnClientDisconnected -= HandleOnClientDisconnected;
        _websocketService.OnClientVerifered -= HandleOnClientVerifered;

        return Task.CompletedTask;
    }
}
