using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Bouncer.Diagnostic;
using Bouncer.State;
using Bouncer.Web.Server.Model;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Sovereign.Discord.Configuration;
using Sovereign.Discord.Web.Server.Model;

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
            GatewayIntents = GatewayIntents.Guilds,
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
            Logger.Info(message.ToString());
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

    /// <summary>
    /// Performs a health check on the Discord bot.
    /// </summary>
    /// <returns>Result of the health check.</returns>
    public async Task<DiscordHealthCheckResult> PerformHealthCheckAsync()
    {
        // Set the status based on the Discord bot status.
        Logger.Debug("Performing health check.");
        var healthCheck = new DiscordHealthCheckResult();
        if (this._client.ConnectionState != ConnectionState.Connected)
        {
            healthCheck.Status = HealthCheckResultStatus.Down;
            healthCheck.Discord.Status = HealthCheckResultStatus.Down;
            Logger.Warn($"Discord bot is currently {this._client.ConnectionState}.");
        }
        else
        {
            Logger.Debug("Discord bot is currently connected.");
        }
        
        // Check the status of the Sovereign API.
        try
        {
            var client = new HttpClient();
            var baseUrl = Environment.GetEnvironmentVariable("SOVEREIGN_BANS_API_BASE_URL") ?? "http://localhost:8000";
            var response = await client.GetAsync($"{baseUrl}/health");
            if (!response.IsSuccessStatusCode)
            {
                healthCheck.Status = HealthCheckResultStatus.Down;
                healthCheck.Sovereign.Status = HealthCheckResultStatus.Down;
                Logger.Warn("Sovereign API returned a down status.");
            }
            else
            {
                Logger.Debug("Sovereign API returned a up status.");
            }
        }
        catch (Exception e)
        {
            healthCheck.Status = HealthCheckResultStatus.Down;
            healthCheck.Sovereign.Status = HealthCheckResultStatus.Down;
            Logger.Error($"Failed to perform health check on the Sovereign API.\n{e}");
        }
        
        // Set the bot status.
        try
        {
            if (healthCheck.Status == HealthCheckResultStatus.Up)
            {
                Logger.Debug("Setting Discord bot health check status to up.");
                await this._client.SetStatusAsync(UserStatus.Online);
            }
            else
            {
                Logger.Debug("Setting Discord bot health check status to down.");
                await this._client.SetStatusAsync(UserStatus.DoNotDisturb);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to set Discord bot status.\n{e}");
        }
        
        // Return the status.
        return healthCheck;
    }
}