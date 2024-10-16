using Match3GameServer.Logging;
using Match3GameServer.Services;
using Match3GameServer.Services.Implementation;
using Match3GameServer.Services.Interfaces;

namespace Match3GameServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
        
        builder.Services.AddGrpc();

        var loggingTagsSection = builder.Configuration.GetSection("Logging:Tags");
        Logging.TagOptions.INFO = loggingTagsSection.GetValue<bool>("INFO");
        Logging.TagOptions.DEBUG = loggingTagsSection.GetValue<bool>("DEBUG");
        Logging.TagOptions.ERROR = loggingTagsSection.GetValue<bool>("ERROR");
        Logging.TagOptions.WARN = loggingTagsSection.GetValue<bool>("WARN");
        Logging.TagOptions.CRITICAL = loggingTagsSection.GetValue<bool>("CRITICAL");

        builder.Services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new TurboLoggerProvider());
        });

        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IWebsocketService, WebsocketService>();
        
        var app = builder.Build();
        
        app.MapGrpcService<GameServiceProto>();
        
        /*app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");*/

        app.MapGet("/", () =>
        {
            app.Logger.LogInformation("Received request at root endpoint");
            return "WebSocket server is running. Use '/ws' to connect.";
        });
        
        app.UseWebSockets();

        app.Use(async (context, next) =>
        {
            app.Logger.LogInformation("Received request: {Path} with method: {Method}", context.Request.Path, context.Request.Method);

            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    app.Logger.LogInformation("Attempting to upgrade to WebSocket...");
                    var gameService = context.RequestServices.GetRequiredService<IWebsocketService>();
                    await gameService.HandleWebSocketAsync(context);
                }
                else
                {
                    app.Logger.LogWarning("Non-WebSocket request received at /ws");
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