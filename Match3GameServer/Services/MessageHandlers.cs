using Match3GameServer.Messages.Base;
using Match3GameServer.Models;

namespace Match3GameServer.Services;

public class MessageHandlers
{
    private readonly Dictionary<Type, List<Action<WebSocketClient, WebsocketMessage>>> _handlers;
    private readonly Dictionary<int, Type> _messages;
    private readonly Dictionary<Type, int> _responses;
    private readonly ILogger<MessageHandlers> _logger;

    public MessageHandlers(ILogger<MessageHandlers> logger)
    {
        _handlers = new();
        _messages = new();
        _responses = new();
        _logger = logger;
    }

    /// <summary>
    /// Registers a message handler for a specific message type.
    /// </summary>
    /// <param name="action">Method to execute when the message is received.</param>
    /// <typeparam name="T">The WebsocketMessage type that the handler will process.</typeparam>
    public void RegisterHandler<T>(Action<WebSocketClient, T> action) where T : WebsocketMessage
    {
        var type = typeof(T);

        if (!_handlers.ContainsKey(type))
        {
            _handlers[type] = new();
        }

        Action<WebSocketClient, WebsocketMessage> wrappedAction = (connection, msg) => action(connection, (T)msg);

        _handlers[type].Add(wrappedAction);
    }

    /// <summary>
    /// Registers the message type by associating it with an integer identifier.
    /// </summary>
    /// <param name="messageId">An integer representing the message type.</param>
    /// <typeparam name="T">The WebsocketMessage type being registered.</typeparam>
    public void RegisterMessage<T>(int messageId)
    {
        var type = typeof(T);

        var result = _messages.TryAdd(messageId, type);

        if (!result)
        {
            _logger.LogWarning(
                $"There was an error when registering the type, perhaps such a message already exists: {messageId}");
        }
    }
    /// <summary>
    /// Registers a response type and associates it with a unique response ID.
    /// </summary>
    /// <param name="responseId">An integer representing the response ID to be associated with the type.</param>
    /// <typeparam name="T">The response type to be registered.</typeparam>
    public void RegisterResponse<T>(int responseId)
    {
        var type = typeof(T);

        var result = _responses.TryAdd(type, responseId);

        if (!result)
        {
            _logger.LogWarning(
                $"There was an error when registering the type, perhaps such a message already exists: {responseId}");
        }
    }

    /// <summary>
    /// Retrieves the response ID associated with a given type.
    /// </summary>
    /// <typeparam name="T">The type for which the response ID is needed.</typeparam>
    /// <returns>A tuple containing a success flag and the associated response ID if found, or 0 if not.</returns>
    public (bool Success, int ResponseId) GetIdByType<T>()
    {
        var type = typeof(T);

        if (!_responses.TryGetValue(type, out int responseId))
        {
            _logger.LogWarning("Response type not found in the registry.");

            return (false, 0);
        }

        return (true, responseId);
    }

    /// <summary>
    /// Invokes the appropriate handler based on the received message type.
    /// </summary>
    /// <param name="client">The WebSocket client that sent the message.</param>
    /// <param name="jsonMessage">The raw JSON message received from the client.</param>
    public void InvokeHandler(WebSocketClient client, string jsonMessage)
    {
        var baseMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<WebsocketMessage>(jsonMessage);

        if (baseMessage == null || !_messages.TryGetValue(baseMessage.MessageId, out var type))
        {
            _logger.LogError($"Invalid or unrecognized message type: {baseMessage?.MessageId}");

            return;
        }

        var message = (WebsocketMessage?)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonMessage, type);

        if (message == null || !_handlers.TryGetValue(type, out var handlers))
        {
            _logger.LogError("Message is null or no handlers found for this message type.");

            return;
        }

        foreach (var handler in handlers)
        {
            handler.Invoke(client, message);
        }
    }
}