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
       
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
