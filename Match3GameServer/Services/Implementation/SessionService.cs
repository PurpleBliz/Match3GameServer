using System.Collections.Concurrent;
using Match3GameServer.GameLogic.Models;
using Match3GameServer.Messages;
using Match3GameServer.Models;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions;
    private readonly ILogger<SessionService> _logger;
    private List<WebSocketClient> _waitingClients;

    private readonly BoardSettings _boardSettings;
    
    public SessionService(ILogger<SessionService> logger)
    {
        _logger = logger;
        
        _waitingClients = new();
        _sessions = new();
        
        _boardSettings = new BoardSettings
        {
            Width = Convert.ToInt32(Environment.GetEnvironmentVariable("ASPNETCORE_BOARD_WIDTH")),
            Height = Convert.ToInt32(Environment.GetEnvironmentVariable("ASPNETCORE_BOARD_HEIGHT"))
        };
        
        MessageHandlers.RegisterHandler<SwapTileMessage>(SwapTile);
    }

    private void SwapTile(WebSocketClient client, SwapTileMessage message)
    {
        if (_sessions.TryGetValue(client.SId, out var session))
        {
            session.TrySwapTile(client, message);
        }
        else
        {
            _logger.LogWarning("Player {PlayerId} is not in an active session", client.InternalId);
        }
    }

    public async Task AddPlayerAsync(WebSocketClient client)
    {
        var opponent = _waitingClients.Find(c => c.SId == client.SId);

        if (opponent == null)
        {
            _waitingClients.Add(client);
            
            _logger.LogInformation("Player {PlayerId} is waiting for an opponent", client.InternalId);
        }
        else
        {
            var sessionId = client.SId;
            
            var session = new GameSession(client, opponent, _boardSettings);
            
            _sessions[sessionId] = session;

            _logger.LogInformation("Starting session {SessionId} with Player {WaitingPlayerId} and Player {PlayerId}",
                sessionId, client.InternalId, opponent.InternalId);
            
            await session.StartSession();
            
            _waitingClients.Remove(opponent);
        }
    }
}