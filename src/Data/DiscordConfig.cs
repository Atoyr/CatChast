using Medoz.CatChast.Clients;

namespace Medoz.CatChast.Data;

/// <summary>
/// </summary>
public record DiscordConfig(ulong? DefaultChannelId, bool UseSpeaker, uint? Speaker)
{
}