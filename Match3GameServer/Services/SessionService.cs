using System.Collections.Concurrent;
using System.Net.Sockets;
using Match3GameServer.Services.Implementation;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;

    private readonly ConcurrentDictionary<int, GameSession> _sessions = new();

    private readonly int _maxSessions;
    private int _sessionCounter;
    private TcpClient _waitingPlayer;
    private int _waitingPlayerId;

    public SessionService
    (
        IConfiguration configuration,
        ILogger<SessionService> logger
    )
    {
        _logger = logger;

        _maxSessions = Convert.ToInt32(configuration["ASPNETCORE_TCP_MAX_SESSIONS"]);
    }

    public void AddPlayer(TcpClient client, int playerId)
    {
        if (_sessions.Count >= _maxSessions)
        {
            client.Close();

            _logger.LogWarning("Server is full, player rejected");

            return;
        }

        if (_waitingPlayer == null)
        {
            _waitingPlayer = client;

            _waitingPlayerId = playerId;

            _logger.LogWarning("Player {PlayerId} is waiting for an opponent", playerId);
        }
        else
        {
            var sessionId = Interlocked.Increment(ref _sessionCounter);

            var session = new GameSession(_waitingPlayer, _waitingPlayerId, client, playerId);

            _sessions[sessionId] = session;

            _waitingPlayer = null;

            _logger.LogInformation("Starting session {SessionId} with Player {WaitingPlayerId} and Player {PlayerId}",
                sessionId, _waitingPlayerId, playerId);
        }
    }
}