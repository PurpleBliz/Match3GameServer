using System.Net.WebSockets;
using System.Text;

namespace Match3GameServer.Services;

public class GameSession
{
    private readonly WebSocket _player1;
    private readonly WebSocket _player2;
    private readonly int _player1Id;
    private readonly int _player2Id;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sessionTask;

    public GameSession(WebSocket player1, int player1Id, WebSocket player2, int player2Id)
    {
        _player1 = player1;
        _player2 = player2;
        _player1Id = player1Id;
        _player2Id = player2Id;
        _cancellationTokenSource = new CancellationTokenSource();
        
        _sessionTask = Task.Run(SessionLoop, _cancellationTokenSource.Token);
    }

    public void EndSession()
    {
        _cancellationTokenSource.Cancel();
        _player1.Dispose();
        _player2.Dispose();
    }

    private async Task SessionLoop()
    {
        var buffer = new byte[8192];

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            // Чтение данных от игрока 1
            if (_player1.State == WebSocketState.Open)
            {
                var result = await _player1.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(_player1Id, message);
                }
            }

            // Чтение данных от игрока 2
            if (_player2.State == WebSocketState.Open)
            {
                var result = await _player2.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(_player2Id, message);
                }
            }

            await Task.Delay(10);
        }
    }

    private async void HandleMessage(int playerId, string message)
    {
        Console.WriteLine($"Player {playerId} sent: {message}");

        var response = Encoding.UTF8.GetBytes($"Player {playerId} acknowledged: {message}");

        if (playerId == _player1Id && _player2.State == WebSocketState.Open)
        {
            await _player2.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
        else if (_player1.State == WebSocketState.Open)
        {
            await _player1.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
    }
}