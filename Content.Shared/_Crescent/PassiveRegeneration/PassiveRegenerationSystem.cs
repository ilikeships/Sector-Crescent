using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Shared._Crescent.PassiveRegeneration;

/// <summary>
/// This handles...
/// </summary>
public sealed class PassiveRegenerationSystem : EntitySystem
{

    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    private float accumulator = 0f;
    /// <inheritdoc/>
    public override void Initialize()
    {
        
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        accumulator += frameTime;
        if (accumulator > 1f)
        {

        }
    }
}
