using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Exists just to listen to a single event. What a life.
/// </summary>
[NetworkedComponent, RegisterComponent] // must be networked to properly predict adding & removal
public sealed partial class SpeedModifiedByContactComponent : Component
{
}

/// <summary>
/// Raised on an entity to check if it has a max contact slowdown.
/// </summary>
[ByRefEvent]
public record struct GetSpeedModifierContactCapEvent() : IInventoryRelayEvent
{
    SlotFlags IInventoryRelayEvent.TargetSlots => ~SlotFlags.POCKET;

    public float MaxSprintSlowdown = 0f;

    public float MaxWalkSlowdown = 0f;

    public void SetIfMax(float valueSprint, float valueWalk)
    {
        MaxSprintSlowdown = MathF.Max(MaxSprintSlowdown, valueSprint);
        MaxWalkSlowdown = MathF.Max(MaxWalkSlowdown, valueWalk);
    }
}
