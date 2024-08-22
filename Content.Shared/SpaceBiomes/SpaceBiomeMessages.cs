using Robust.Shared.Serialization;

namespace Content.Shared.SpaceBiomes;

[Serializable, NetSerializable]
public sealed class SpaceBiomeSwapMessage : EntityEventArgs
{
    public string Biome = "";
}