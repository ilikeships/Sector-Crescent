using Content.Server.Radio.Components;
using Content.Server.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Content.Shared.Sound.Components;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BroadcasterSystem : SharedBroadphaseSyste
m

{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

}
