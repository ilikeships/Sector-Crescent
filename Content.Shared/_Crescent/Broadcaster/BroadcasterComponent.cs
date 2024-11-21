using Robust.Shared.Serialization;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BroadcasterComponent : Component
{
    [DataField("range")]
    public float Range = 15f;

    /// <summary>
    /// Can only receive broadcasts from a console with the same outpost set.
    /// </summary>
    [DataField("outpost")]
    public string? Outpost;
}

public sealed partial class BroadcastingConsoleComponent : Component
{
    [DataField("outpost")]
    public string? Outpost;

    public int currentlyPlaying = -1;

    public List<string>? availableAnnouncements;
}

[Serializable, NetSerializable]
public sealed class BroadcasterConsoleState : BoundUserInterfaceState
{

}

[Serializable, NetSerializable]
public sealed class BroadcasterBroadcastMessage : BoundUserInterfaceMessage
{
}


[NetSerializable, Serializable]
public enum BroadcasterUIKey
{
    Key,
}


