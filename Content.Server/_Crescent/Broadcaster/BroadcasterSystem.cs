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
using static Content.Shared.Access.Components.IdCardConsoleComponent;
using Content.Shared.Access.Components;
using Robust.Server.GameObjects;
using Robust.Server.Player;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>W
public sealed partial class BroadcasterSystem : SharedBroadcasterSystem

{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;



    private List<BroadcastWrapper> broadcastableMessages = new();

    private Dictionary<string, int> currentlyPlayingOn = new();


    private struct BroadcastWrapper
    {
        public SoundPathSpecifier sound;
        public TimeSpan duration;
        public string name;
        public string outpost;

        public BroadcastWrapper(SoundPathSpecifier playing, TimeSpan dur, string name, string outpost)
        {
            sound = playing;
            duration = dur;
            this.name = name;
            this.outpost = outpost;
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        var messages = _prototypeManager.EnumeratePrototypes<BroadcastableMessagePrototype>();
        foreach (var message in messages)
        {
            if (message.Outpost is null)
                continue;
            broadcastableMessages.Add(new BroadcastWrapper(message.announceSound, _audioSystem.GetAudioLength(message.announceSound.Path.Filename), message.Name, message.Outpost));
            if (!currentlyPlayingOn.ContainsKey(message.Outpost))
            {
                currentlyPlayingOn.Add(message.Outpost, -1);
            }
        }
        SubscribeLocalEvent<BroadcastingConsoleComponent, ComponentStartup>(RequestAvailableBroadcasts);
        SubscribeLocalEvent<BroadcastingConsoleComponent, BroadcasterBroadcastMessage>(PlayBroadcast);
    }

    private Dictionary<int, string> buildBroadcastListForState(string outpost)
    {

        var dict = new Dictionary<int, string>();

        for (int i = 0; i < broadcastableMessages.Count; i++)
        {
            var message = broadcastableMessages[i];
            if (message.outpost != outpost)
                continue;
            dict.Add(i, message.name);
        }

        return dict;

    }

    public void RequestAvailableBroadcasts(EntityUid uid, BroadcastingConsoleComponent comp, ref ComponentStartup args)
    {
        if (comp.Outpost is null)
            return;
        var newState = new BroadcasterConsoleState(buildBroadcastListForState(comp.Outpost), currentlyPlayingOn[comp.Outpost]);
        _userInterface.SetUiState(uid, BroadcasterUIKey.Key, newState);
    }

    public void PlayBroadcast(EntityUid uid, BroadcastingConsoleComponent comp, ref BroadcasterBroadcastMessage args)
    {
        if (args.indexForBroadcast > broadcastableMessages.Count)
            return;
        var message = broadcastableMessages[args.indexForBroadcast];
        if (message.outpost != comp.Outpost)
            return;
        if (currentlyPlayingOn[message.outpost] != -1)
            return;
        var comps = EntityManager.GetAllComponents(typeof(BroadcasterComponent));
        foreach (var broadcaster in comps)
        {
            _lookup.Get

        }


    }

}
