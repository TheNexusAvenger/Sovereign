using System.Reflection;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Sovereign.Discord.Configuration;

namespace Sovereign.Discord.Discord;

public class Bot
{
    /// <summary>
    /// Client used for Discord.
    /// </summary>
    private readonly DiscordSocketClient _client;
    
    /// <summary>
    /// Interaction service used for commands.
    /// </summary>
    private readonly InteractionService _interactionService;

    /// <summary>
    /// Creates a Bot.
    /// </summary>
    public Bot()
    {
        var config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.None,
        };
        this._client = new DiscordSocketClient(config);
        this._interactionService = new InteractionService(this._client.Rest);
    }
    
    /// <summary>
    /// Starts the Discord bot.
    /// </summary>
    public async Task StartAsync()
    {
        // Initialize the bot.
        Logger.Info("Starting Discord bot.");
        this._client.Log += (message) =>
        {
            Logger.Debug(message.ToString());
            return Task.CompletedTask;
        };
        this._client.JoinedGuild += async (guild) => await this._interactionService.RegisterCommandsToGuildAsync(guild.Id);
        this._client.Ready += this.ClientReadyHandler;
        
        // Initialize the interaction service (commands).
        await this._interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        this._client.InteractionCreated += async interaction =>
        {
            var context = new SocketInteractionContext(this._client, interaction);
            await this._interactionService.ExecuteCommandAsync(context, null);
        };
        
        // Start the bot.
        await this._client.LoginAsync(TokenType.Bot, Configurations.GetConfiguration<DiscordConfiguration>().Discord!.Token);
        await this._client.StartAsync();
    }

    /// <summary>
    /// Handles the bot being ready.
    /// </summary>
    private async Task ClientReadyHandler()
    {
        // Register the guild commands.
        // Guild commands load much faster than global commands.
        foreach (var guild in this._client.Guilds)
        {
            await this._interactionService.RegisterCommandsToGuildAsync(guild.Id);
        }
    }
}