using Match3GameServer.GameLogic;
using Match3GameServer.GameLogic.Models;
using Match3GameServer.Messages;
using Match3GameServer.Messages.Client;
using Match3GameServer.Messages.Server;
using Match3GameServer.Models;

namespace Match3GameServer.Services;

public class GameSession
{
    private readonly WebSocketClient _firstPlayer;
    private readonly WebSocketClient _secondPlayer;

    private readonly Dictionary<WebSocketClient, BoardController> _boardControllers;
    
    private readonly Task _sessionTask;
    private readonly BoardSettings _boardSettings;

    public GameSession(WebSocketClient firstPlayer, WebSocketClient secondPlayer, BoardSettings boardSettings)
    {
        _firstPlayer = firstPlayer;
        _secondPlayer = secondPlayer;
        _boardSettings = boardSettings;
        
        _boardControllers = new();
        
        _boardControllers.Add(_firstPlayer, new BoardController());
        _boardControllers.Add(_secondPlayer, new BoardController());
    }

    public void EndSession()
    {
       
    }

    public async Task StartSession()
    {
        foreach (var client in _boardControllers.Keys)
        {
            var board = _boardControllers[client].GetNewBoard(_boardSettings.Width, _boardSettings.Height);

            await client.SendMessage<BoardLayoutMessage>(new BoardLayoutMessage()
            {
                Width = board.Width,
                Height = board.Height,
                TileLayouts = board.TileLayouts
            });
        }
    }

    public void TrySwapTile(WebSocketClient client, SwapTileMessage message)
    {
        //TODO: Create logic
    }
}