using System.Collections.Frozen;
using System.Linq;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Crescent.Radar;
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
    private Dictionary<EntityUid, HashSet<EntityUid>> receptionList = new();

    private float curTime = 0f;
    private const float pingCheckInterval = 5f;
    private TimeSpan alertCooldown = TimeSpan.FromSeconds(300);

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RadarConsoleComponent, MapInitEvent>(OnRadarInit);
    }

    public void OnRadarInit(EntityUid owner, RadarConsoleComponent component, ref MapInitEvent args)
    {
        EnsureComp<RadarPingerComponent>(owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timer.IsFirstTimePredicted)
            curTime += frameTime;
        var query = EntityQueryEnumerator<RadarConsoleComponent, RadarPingerComponent, TransformComponent>();
        var checking = EntityManager.GetAllComponents(typeof(RadarConsoleComponent), false).ToDictionary();
        while (query.MoveNext(out var uid, out var radar, out var pinger, out var transform))
        { 
            var ourRange = radar.MaxRange / 2;
            var ourPos = _transform.GetWorldPosition(transform);
            if (!receptionList.ContainsKey(uid))
            {
                receptionList.Add(uid, new HashSet<EntityUid>());
            }

            var ourHash = receptionList[uid];

            foreach (var (key, _) in checking)
            {
                var targetTrans = Transform(key);
                // dont care about inactives
                if (!_uiSystem.IsUiOpen(key, RadarConsoleUiKey.Key))
                    continue;
                if ((_transform.GetWorldPosition(targetTrans) - ourPos).Length() > ourRange)
                    continue;
                ourHash.Add(key);
            }

        }
        if (curTime > pingCheckInterval)
        {
            curTime = 0f;
            
            var worldTime = _timer.CurTime;
            foreach(var (key, set) in receptionList)
            {
                if (!TryComp<RadarConsoleComponent>(key, out var comp))
                    continue;
                //if (worldTime - comp.lastAlert < TimeSpan.Zero)
                //    continue;
                if (!set.Any())
                    continue;
                _chatSystem.TrySendInGameICMessage(key, $":d Notice: Mass scanner pings detected in local space!", InGameICChatType.Speak, ChatTransmitRange.Normal);
                comp.lastAlert = worldTime + alertCooldown;
            }
            receptionList.Clear();

        }


    }
}
