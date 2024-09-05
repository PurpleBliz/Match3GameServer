using System.Net;
using System.Net.Sockets;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer.Services.Implementation;

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly TcpListener? _tcpListener;
    private readonly ISessionService _sessionService;

    public GameService
    (
        IConfiguration configuration,
        ILogger<GameService> logger,
        ISessionService sessionService
    )
    {
        _sessionService = sessionService;
        _logger = logger;

        string ip = configuration["ASPNETCORE_TCP_IP"] ?? throw new InvalidOperationException();
        int port = Convert.ToInt32(configuration["ASPNETCORE_TCP_PORT"]);
        
        if (string.IsNullOrEmpty(ip) || port <= 0)
        {
            _logger.LogError("Ip or Port variable is not validate");

            return;
        }
        
        _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
    }

    public void StartServer()
    {
        if (!CheckAvailable()) return;
        
        _tcpListener?.Start();

        _logger.LogInformation("TCP Server started");

        ListenForClients();
    }

    public void StopServer()
    {
        if (!CheckAvailable()) return;
        
        _tcpListener?.Stop();
        
        _logger.LogInformation("Server stopped");
    }
    
    private void ListenForClients()
    {
        int playerIdCounter = 1;

        while (true)
        {
            var client = _tcpListener?.AcceptTcpClient();
            
            int playerId = playerIdCounter++;

            _logger.LogInformation("Player {PlayerId} connected", playerId);

            if (client != null) _sessionService.AddPlayer(client, playerId);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private bool CheckAvailable()
    {
        if (_tcpListener == null)
        {
            _logger.LogError("The TCP Listener was not initialized");
        }

        return _tcpListener != null;
    }
}