using System.Collections.Concurrent;
using System.Net.WebSockets;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<int, GameSession> _sessions = new();
    private readonly ILogger<SessionService> _logger;
    private WebSocket? _waitingPlayer;
    private int _waitingPlayerId;

    public SessionService(ILogger<SessionService> logger)
    {
        _logger = logger;
    }

    public async Task AddPlayerAsync(WebSocket webSocket, int playerId)
    {
        if (_waitingPlayer == null)
        {
            _waitingPlayer = webSocket;
            _waitingPlayerId = playerId;

            _logger.LogInformation("Player {PlayerId} is waiting for an opponent", playerId);
        }
        else
        {
            var sessionId = _sessions.Count + 1;
            var session = new GameSession(_waitingPlayer, _waitingPlayerId, webSocket, playerId);
            _sessions[sessionId] = session;

            _waitingPlayer = null;

            _logger.LogInformation("Starting session {SessionId} with Player {WaitingPlayerId} and Player {PlayerId}",
                sessionId, _waitingPlayerId, playerId);
        }
    }
}