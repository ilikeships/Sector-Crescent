using Content.Server.Radio.Components;
using Content.Server.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Content.Shared.Sound.Components;
using Robust.Server.GameStates;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;
using Content.Shared._Crescent.Broadcaster;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BroadcasterSystem : SharedBroadcasterSystem

{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;



    private List<BroadcastWrapper> broadcastableMessages = new();

    private Dictionary<string, bool> currentlyPlayingOn = new();


    private struct BroadcastWrapper
    {
        public SoundPathSpecifier sound;
        public TimeSpan duration;

        public BroadcastWrapper(SoundPathSpecifier playing, TimeSpan dur)
        {
            sound = playing;
            duration = dur;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        var messages = _prototypeManager.EnumeratePrototypes<BroadcastableMessagePrototype>();
        foreach (var message in messages)
        {
            broadcastableMessages.Add(new BroadcastWrapper(message.announceSound, _audioSystem.GetAudioLength(message.announceSound.Path.Filename)));
        }7l
    }

}
