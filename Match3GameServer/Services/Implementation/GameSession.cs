using System.Net.Sockets;
using System.Text;

namespace Match3GameServer.Services.Implementation;

public class GameSession
{
    private readonly TcpClient _player1;
    private readonly TcpClient _player2;
    private readonly NetworkStream _player1Stream;
    private readonly NetworkStream _player2Stream;
    private readonly int _player1Id;
    private readonly int _player2Id;

    private readonly CancellationTokenSource _cancellationTokenSource;
    
    private readonly Task _sessionTask;

    public GameSession(TcpClient player1, int player1Id, TcpClient player2, int player2Id)
    {
        _player1 = player1;
        _player2 = player2;
        _player1Id = player1Id;
        _player2Id = player2Id;
        _player1Stream = _player1.GetStream();
        _player2Stream = _player2.GetStream();
        _cancellationTokenSource = new CancellationTokenSource();

        // Запуск сессии в отдельном потоке
        _sessionTask = Task.Run(SessionLoop, _cancellationTokenSource.Token);
    }

    public void EndSession()
    {
        _cancellationTokenSource.Cancel();
        
        _player1.Close();
        
        _player2.Close();
    }

    private async Task SessionLoop()
    {
        var buffer = new byte[8192];

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            // Чтение данных от игрока 1
            if (_player1Stream.DataAvailable)
            {
                int bytesRead = await _player1Stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    HandleMessage(_player1Id, message);
                }
            }

            // Чтение данных от игрока 2
            if (_player2Stream.DataAvailable)
            {
                int bytesRead = await _player2Stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    HandleMessage(_player2Id, message);
                }
            }

            await Task.Delay(10); // Небольшая задержка для предотвращения чрезмерного использования процессора
        }
    }

    private void HandleMessage(int playerId, string message)
    {
        Console.WriteLine($"Player {playerId} sent: {message}");
        
        var response = Encoding.UTF8.GetBytes($"Player {playerId} acknowledged: {message}");

        if (playerId == _player1Id)
        {
            _player2Stream.Write(response, 0, response.Length);
        }
        else
        {
            _player1Stream.Write(response, 0, response.Length);
        }
    }
}