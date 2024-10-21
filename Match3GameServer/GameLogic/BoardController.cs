using System.Text;
using Match3GameServer.GameLogic.Models;
using Match3GameServer.Messages;

namespace Match3GameServer.GameLogic;

public sealed class BoardController
{
    private List<int> _list = new() //TODO: Swap to GUID
    {
        1, 2, 3, 4, 5, 6
    };

    private BoardLayout _boardLayout;
    private Random _random = new();
    private StringBuilder _logText = new();
    
    public void SetBoard(BoardLayout board)
    {
        _boardLayout = board;

        BoardLogging("SetBoard");
    }

    public BoardLayout GetNewBoard(int width, int height)
    {
        BoardLayout boardLayout = new(8, 10); //TODO:Get in ENV

        int[] previousTop = new int[boardLayout.Width];

        int previousLeft = default;

        for (int y = 0; y < boardLayout.Height; y++)
        {
            for (int x = 0; x < boardLayout.Width; x++)
            {
                List<int> possibleItems = new(_list);

                if (y > 0)
                {
                    possibleItems.Remove(previousTop[x]);
                }

                if (x > 0)
                {
                    if (previousLeft != default)
                    {
                        possibleItems.Remove(previousLeft);
                    }
                }

                int itemId = possibleItems[_random.Next(0, possibleItems.Count)];

                boardLayout.TileLayouts[x, y] = new TileLayout(x, y, itemId);

                previousTop[x] = itemId;
                previousLeft = itemId;
            }
        }

        _boardLayout = boardLayout;

        BoardLogging("GetNewBoard");
        
        return boardLayout;
    }

    public void TrySwap(SwapTileMessage swapTile)
    {
        //TODO: Session boards

        if (!IsNeighbour(swapTile))
        {
            return; //TODO: responce
        }
        
        BoardLogging("TrySwap");

        var combination = CheckCombination(SwapTilePair(swapTile));

        if (!combination.IsCombination)
        {
            return; //TODO: responce
        }

        foreach (var item in combination.Combination)
        {
            item.SetNewId(0);
        }

        BoardLogging("after remove");

        FillEmptyTiles(combination.Combination);

        BoardLogging("after fill");
        
        bool hasCombination;
        do
        {
            hasCombination = false;
        
            var repeatedCombinations = CheckCombination(GetAllTiles());

            BoardLogging("After check CheckCombination");
            
            if (repeatedCombinations.IsCombination)
            {
                hasCombination = true;

                foreach (var item in repeatedCombinations.Combination)
                {
                    item.SetNewId(0);
                }

                BoardLogging("After check all GOMNO");
                
                FillEmptyTiles(repeatedCombinations.Combination);
            }
        
        } while (hasCombination);

        BoardLogging("Final board state");
    }

    private List<TileLayout> SwapTilePair(SwapTileMessage swapTile)
    {
        TileLayout from = _boardLayout.TileLayouts[swapTile.FromXPosition, swapTile.FromYPosition];
        TileLayout to = _boardLayout.TileLayouts[swapTile.ToXPosition, swapTile.ToYPosition];

        to.SetNewPosition(swapTile.FromXPosition, swapTile.FromYPosition);
        from.SetNewPosition(swapTile.ToXPosition, swapTile.ToYPosition);

        _boardLayout.TileLayouts[swapTile.FromXPosition, swapTile.FromYPosition] = to;
        _boardLayout.TileLayouts[swapTile.ToXPosition, swapTile.ToYPosition] = from;
        
        return new List<TileLayout>
        {
            from,
            to
        };
    }

    private List<TileLayout> GetAllTiles()
    {
        var allTiles = new List<TileLayout>();
        
        for (int x = 0; x < _boardLayout.Width; x++)
        {
            for (int y = 0; y < _boardLayout.Height; y++)
            {
                allTiles.Add(_boardLayout.TileLayouts[x, y]);
            }
        }
        
        return allTiles;
    }

    private void FillEmptyTiles(List<TileLayout> combination)
    {
        var verticalTiles = combination
            .GroupBy(tile => tile.BoardXPosition)
            .Where(group => group.Count() > 2)
            .SelectMany(group => group.OrderByDescending(tile => tile.BoardYPosition))
            .ToList();

        var horizontalTiles = combination
            .GroupBy(tile => tile.BoardYPosition)
            .Where(group => group.Count() >= 2)
            .SelectMany(group => group.OrderBy(tile => tile.BoardXPosition))
            .ToList();

        var commonTiles = verticalTiles.Intersect(horizontalTiles).ToList();

        horizontalTiles.RemoveAll(tile => commonTiles.Contains(tile));

        if (horizontalTiles.Count > 3)
        {
            var groupedByX = horizontalTiles
                .GroupBy(tile => tile.BoardXPosition)
                .ToList();

            int maxY = groupedByX.SelectMany(group => group).Max(tile => tile.BoardYPosition);

            var result = groupedByX
                .SelectMany(group => group)
                .Where(tile => tile.BoardYPosition == maxY)
                .ToList();

            horizontalTiles = horizontalTiles
                .Where(tile => tile.BoardYPosition == maxY || !result.Any(r => r.BoardXPosition == tile.BoardXPosition))
                .ToList();
        }

        var color = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine("verticalTiles Group:");
        Console.WriteLine(string.Join(", ", verticalTiles.Select(t => $"[{t.BoardXPosition}, {t.BoardYPosition}]")));

        Console.WriteLine("horizontalTiles Group:");
        Console.WriteLine(string.Join(", ", horizontalTiles.Select(t => $"[{t.BoardXPosition}, {t.BoardYPosition}]")));

        Console.ForegroundColor = color;

        int count = 0;

        foreach (var horizontalItem in horizontalTiles)
            FillColumn(horizontalItem.BoardXPosition, horizontalItem.BoardYPosition, ref count);

        if (verticalTiles.Count > 0)
            FillColumn(verticalTiles[0].BoardXPosition, verticalTiles[0].BoardYPosition, ref count);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Count in cycle = {count}");
        Console.ForegroundColor = color;
        
        BoardLogging("After new alg");
    }

    private void FillColumn(int xPosition, int startPosition, ref int counter)
    {
        int zeroCount = 1;
            
        for (int i = startPosition - 1; i >= 0; i--)
        {
            var currentTile = _boardLayout.TileLayouts[xPosition, i];

            if (currentTile.ItemId == 0)
            {
                zeroCount++;
                continue;
            }

            var targetYPosition = i + zeroCount;
            var nullTile = _boardLayout.TileLayouts[xPosition, targetYPosition];
            var beforeId = nullTile.ItemId;
                
            _boardLayout.TileLayouts[xPosition, nullTile.BoardYPosition].SetNewId(currentTile.ItemId);
            _boardLayout.TileLayouts[xPosition, currentTile.BoardYPosition].SetNewId(beforeId);
                
            if (i == 0)
            {
                for (int j = zeroCount - 1; j >= 0; j--)
                {
                    _boardLayout.TileLayouts[xPosition, j].SetNewId(GetNewId(_boardLayout.TileLayouts[xPosition, j]));

                    counter++;
                }
                
                break;
            }

            counter++;
        }
    }

    private int GetNewId(TileLayout currentTile)
    {
        var availableIds = new List<int>(_list);
        
        int x = currentTile.BoardXPosition;
        int y = currentTile.BoardYPosition;
        
        var offsets = new (int xOffset, int yOffset)[]
        {
            (-1, 0), // Left
            (1, 0),  // Right
            (0, -1), // Bottom
            (0, 1)   // Under
        };
        
        foreach (var (xOffset, yOffset) in offsets)
        {
            int neighborId = GetTileId(x + xOffset, y + yOffset);
            availableIds.Remove(neighborId);
        }
        
        return availableIds[_random.Next(availableIds.Count)];
    }
    
    private bool IsNeighbour(SwapTileMessage swapTile)
    {
        return Math.Abs(swapTile.FromXPosition - swapTile.ToXPosition) +
            Math.Abs(swapTile.FromYPosition - swapTile.ToYPosition) == 1;
    }

    private (List<TileLayout> Combination, bool IsCombination) CheckCombination(List<TileLayout> tiles)
    {
        HashSet<TileLayout> combination = new();

        foreach (var tile in tiles)
        {
            var vertical = GetNeighbours(tile.BoardXPosition, tile.BoardYPosition, true);
            var horizontal = GetNeighbours(tile.BoardXPosition, tile.BoardYPosition, false);

            if (vertical.Count > 0 && horizontal.Count == 0)
            {
                combination.UnionWith(vertical);
            }

            if (horizontal.Count > 0 && vertical.Count == 0)
            {
                combination.UnionWith(horizontal);
            }

            if (horizontal.Count > 0 && vertical.Count > 0)
            {
                combination.UnionWith(vertical);
                combination.UnionWith(horizontal);
            }

            if (horizontal.Count > 0 || vertical.Count > 0)
            {
                combination.Add(tile);
            }

            CheckCombinationLogging(tile, combination.ToList());
        }

        return (combination.ToList(), combination.Count > 0);
    }

    private List<TileLayout> GetNeighbours(int startX, int startY, bool isVertical)
    {
        List<TileLayout> combination = new();

        TileLayout startTile = _boardLayout.TileLayouts[startX, startY];
        
        int startPosition = isVertical ? startY : startX;
        int border = isVertical ? _boardLayout.Height : _boardLayout.Width;

        int direction = -1;
        int currentStep = startPosition + direction;
        
        for (int i = 0; i < 2; i++)
        {
            while (currentStep >= 0 && currentStep < border &&
                   GetTileByDirection(isVertical, startTile, currentStep).IsIdentical)
            {
                var currentItem = GetTileByDirection(isVertical, startTile, currentStep);
                
                NeighboursLogging(isVertical, direction, currentItem.Tile);
                
                combination.Add(currentItem.Tile);

                currentStep += direction;
            }
            
            direction *= -1;
            currentStep = startPosition + direction;
        }

        if (combination.Count < 2)
            combination.Clear();

        return combination;
    }

    private (TileLayout Tile, bool IsIdentical) GetTileByDirection(bool isVertical, TileLayout startTile, int step)
    {
        TileLayout tile = isVertical
            ? _boardLayout.TileLayouts[startTile.BoardXPosition, step]
            : _boardLayout.TileLayouts[step, startTile.BoardYPosition];
        
        return (tile, tile.ItemId == startTile.ItemId);
    }

    private int GetTileId(int x, int y)
    {
        return !OnBoard(x, y) ? default : _boardLayout.TileLayouts[x, y].ItemId;
    }

    private bool OnBoard(int x, int y)
    {
        return x >= 0 && x < _boardLayout.Width && y >= 0 && y < _boardLayout.Height;
    }
    
    private void BoardLogging(string header)
    {
        _logText.Clear();

        _logText.Append($"=================={header.ToUpper()}====================");
        
        for (int y = 0; y < _boardLayout.Height; y++)
        {
            _logText.Append($"\r\n{y} => |");
            
            for (int x = 0; x < _boardLayout.Width; x++)
                _logText.Append($"{_boardLayout.TileLayouts[x, y].ItemId}|");
        }
        
        _logText.Append($"\r\n=================={header.ToUpper()}====================");
        
        Console.WriteLine(_logText);
    }

    private void CheckCombinationLogging(TileLayout tile, List<TileLayout> combination)
    {
        _logText.Clear();

        _logText.Append(
            $"[CheckCombinationLogging]: Cell[{tile.BoardXPosition}:{tile.BoardYPosition}] [ID: {tile.ItemId}] => ");

        foreach (var item in combination)
        {
            _logText.Append($"[ID:{item.ItemId}][{item.BoardXPosition}:{item.BoardYPosition}]");

            if (item != combination.Last())
            {
                _logText.Append(", ");
            }
        }

        Console.WriteLine(_logText);
    }

    private void NeighboursLogging(bool isVertical, int direction, TileLayout tile)
    {
        _logText.Clear();

        _logText.Append($"[NeighboursLogging]: Cell[{tile.BoardXPosition}:{tile.BoardYPosition}] [ID: {tile.ItemId}] => Direction: ");
        
        if (isVertical)
        {
            _logText.Append(direction < 0 ? "Vertical Up" : "Vertical Down");
        }
        else
        {
            _logText.Append(direction < 0 ? "Horizontal Left" : "Horizontal Right");
        }
        
        Console.WriteLine(_logText);
    }
}