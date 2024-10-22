using Match3GameServer.Messages;
using Match3GameServer.Services;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
        
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        
        builder.Configuration.InitLogging();
        
        builder.Services.AddServices();
        
        var app = builder.Build();
        
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        
        loggerFactory.InitMessageHandlers();

        ServicesExtensions.RegisterMessages();
        
        app.MapGrpcService<GameServiceProto>();
        
        app.MapGet("/", () =>
        {
            return "WebSocket server is running. Use '/ws' to connect.";
        });
        
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            app.Logger.LogWarning("Received request: {Path} with method: {Method}", context.Request.Path, context.Request.Method);

            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var websocketService = context.RequestServices.GetRequiredService<IWebsocketService>();
                    
                    await websocketService.HandleWebSocketAsync(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("This endpoint requires a WebSocket connection.");
                }
            }
            else
            {
                await next();
            }
        });
        
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var wsPath = "/ws";
            app.Logger.LogInformation("Checking WebSocket server setup...");
            
            var addresses = app.Urls;
            
            foreach (var address in addresses)
            {
                app.Logger.LogInformation($"WebSocket server is running at {address}{wsPath}");
            }
            
            app.Logger.LogInformation("Listening for WebSocket connections...");
        });
        
        /*BoardController boardController = new();

        if (!File.Exists("Save.json"))
        {
            var board = boardController.GetNewBoard();

            string json = JsonConvert.SerializeObject(board, Formatting.Indented);

            await File.WriteAllTextAsync("Save.json", json);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            
            Console.WriteLine($"Size of JSON data in bytes: {jsonBytes.Length}");
        }
        else
        {
            string json = await File.ReadAllTextAsync("Save.json");

            var board = JsonConvert.DeserializeObject<BoardLayout>(json);

            boardController.SetBoard(board);
        }*/


//boardController.TrySwap(new SwapTileDto(3, 0, 4, 0)); //-
//boardController.TrySwap(new SwapTileDto(0, 8, 1, 8)); //T
//boardController.TrySwap(new SwapTileDto(0, 1, 1, 1)); //Г
//boardController.TrySwap(new SwapTileDto(0, 0, 1, 0)); //-
//boardController.TrySwap(new SwapTileDto(5, 8, 4, 8));

        await app.RunAsync();
    }
}