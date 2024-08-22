using Content.Shared.SpaceBiomes;
using Robust.Shared.Prototypes;
using Content.Client.Audio;
using Robust.Client.Graphics;

namespace Content.Client.SpaceBiomes;

public sealed class SpaceBiomeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protMan = default!;
    [Dependency] private readonly IOverlayManager _overMan = default!;
    [Dependency] private readonly ContentAudioSystem _audioSys = default!;

    private SpaceBiomeTextOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<SpaceBiomeSwapMessage>(OnSwap);
        _overlay = new();
        _overMan.AddOverlay(_overlay);
    }

    private void OnSwap(SpaceBiomeSwapMessage ev)
    {
        _audioSys.ForceUpdateAmbientMusic();
        SpaceBiomePrototype biome = _protMan.Index<SpaceBiomePrototype>(ev.Biome);
        _overlay.Text = biome.Name;
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / biome.Name.Length);
    }
}