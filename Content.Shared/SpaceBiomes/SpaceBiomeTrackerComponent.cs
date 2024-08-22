using Robust.Shared.GameStates;

namespace Content.Shared.SpaceBiomes;

//attached to the player
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpaceBiomeTrackerComponent : Component
{
    //for ambience rule
    [AutoNetworkedField]
    public string Biome;

    //server only
    public SpaceBiomeSourceComponent? Source;
}