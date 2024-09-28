using System.Linq;
using Discord.Interactions;
using Sovereign.Discord.Discord.Shim;

namespace Sovereign.Discord.Discord;

public abstract class ExtendedInteractionModuleBase : InteractionModuleBase
{
    /// <summary>
    /// Context used for tests.
    /// </summary>
    private IInteractionContextWrapper? _overrideContextWrapper;

    /// <summary>
    /// Sets an override for the context wrapper.
    /// Intended to be set in unit tests.
    /// </summary>
    /// <param name="contextWrapper"></param>
    public void SetOverrideContextWrapper(IInteractionContextWrapper? contextWrapper)
    {
        this._overrideContextWrapper = contextWrapper;
    }

    /// <summary>
    /// Returns the context used for handling interactions.
    /// </summary>
    /// <returns>The context used for handling interactions.</returns>
    public IInteractionContextWrapper GetContext()
    {
        return this._overrideContextWrapper ?? new InteractionContextWrapper(this.Context);
    }
    
    /// <summary>
    /// Returns the domain for the current server (if one is configured).
    /// </summary>
    /// <returns>The domain for the server, or null if none is configured.</returns>
    public string? GetDomain()
    {
        var context = this.GetContext();
        var configuration = context.GetConfiguration();
        var guildId = context.DiscordGuildId;
        return configuration.Discord?.Servers?.FirstOrDefault(entry => entry.Id == guildId)?.Domain;
    }
}