using Robust.Shared.Prototypes;

namespace Content.Shared.SpaceBiomes;

//RANE MAKE THIS SHIT WORK WITH PROCGEN PLEASE
[Prototype("crescentFactionBiome")]
public sealed class SpaceBiomePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)] //can't be automatically validated since parallax prototype is available only for clients
    public string Parallax = "";

    [DataField(required: true)]
    public string Name = "";

    /// <summary>
    /// Time of interpolation between current parallax and a new one in seconds
    /// Does not include time to load new parallax textures
    /// </summary>
    [DataField(required: true)]
    public int SwapDuration;
}
