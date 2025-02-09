using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Crescent.Radar;

/// <summary>
/// This handles...
/// </summary>
public sealed class SonarPingSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly RadioDeviceSystem _radioSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MapSystem _maps = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timer = default!;
    private Dictionary<EntityUid, Dictionary<EntityUid, TimeSpan>> receptionList = new();

    private float curTime = 0f;
    private const float pingCheckInterval = 5f;
    private TimeSpan alertCooldown = TimeSpan.FromSeconds(300);

    /// <inheritdoc/>
    public override void Initialize()
    {

    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timer.IsFirstTimePredicted)
            curTime += frameTime;
        {
            var query = EntityQueryEnumerator<RadarConsoleComponent, TransformComponent>();

            var checkDictionary = new Dictionary<EntityUid, TransformComponent>();

            while (query.MoveNext(out var uid, out var console, out var transform))
            {
                if (!_uiSystem.IsUiOpen(uid, RadarConsoleUiKey.Key))
                {
                    continue;
                }

                var range = console.MaxRange / 2;
                var ourPos = _transform.GetWorldPosition(transform);
                if (!receptionList.ContainsKey(uid))
                    receptionList.Add(uid, new Dictionary<EntityUid, TimeSpan>());
                var ourDict = receptionList[uid];
                foreach (var (ent, trans) in checkDictionary)
                {
                    if ((_transform.GetWorldPosition(trans) - ourPos).Length() > range)
                        continue;
                    var targetDict = receptionList[ent];
                    if (!ourDict.ContainsKey(ent))
                    {
                        ourDict.Add(ent, TimeSpan.Zero);
                    }

                    if (!targetDict.ContainsKey(uid))
                    {
                        targetDict.Add(uid, TimeSpan.Zero);
                    }

                    targetDict[uid] = _timer.CurTime;
                    ourDict[ent] = _timer.CurTime;
                }

                checkDictionary.Add(uid, transform);
            }
        }

        if (curTime > pingCheckInterval)
        {
            curTime = 0f;
            var query = EntityQueryEnumerator<RadarConsoleComponent>();
            var worldTime = _timer.CurTime;
            while (query.MoveNext(out var uid, out var comp))
            {
                if (comp.lastAlert - worldTime < TimeSpan.Zero)
                    continue;
                if (!receptionList.ContainsKey(uid))
                    continue;
                _chatSystem.TrySendInGameICMessage(uid, $":d Notice: Mass scanner pings detected in local space!", InGameICChatType.Speak, ChatTransmitRange.Normal);
                comp.lastAlert = worldTime + alertCooldown;
            }
            receptionList.Clear();
            
        }


    }
}
