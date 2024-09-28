using Robust.Shared.Configuration;

namespace Content.Shared.Crescent.CCvar;

[CVarDefs]
public sealed class CrescentCVars
{
    /// <summary>
    /// Whether or not respawning is enabled.
    /// </summary>
    public static readonly CVarDef<bool> RespawnEnabled =
        CVarDef.Create("sc.respawn.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("sc.respawn.time", 1200.0f, CVar.SERVER | CVar.REPLICATED);
}
