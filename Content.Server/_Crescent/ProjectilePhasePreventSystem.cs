using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._Crescent;
using Content.Shared.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
public sealed class ProjectilePhasePreventerSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _phys = default!;
    [Dependency] private readonly SharedTransformSystem _trans = default!;
    // im so sorry , SPCR 2025
    ConcurrentQueue<CollisionEventArgsHolder> eventQueue = new();

    internal sealed class RaycastBucket
    {
        public EntityUid owner;
        public EntityUid? shooter;
        public Vector2 start;
        public Vector2 end;
        public string fixtureKey;
        public Robust.Shared.Physics.Dynamics.Fixture fixture;
        public PhysicsComponent physComp;
        public ProjectilePhasePreventComponent phaseComp;
        public int collisionMask;
        public MapId map;

        public RaycastBucket(string key, Robust.Shared.Physics.Dynamics.Fixture fix, PhysicsComponent component, ProjectilePhasePreventComponent phase)
        {
            fixtureKey = key;
            fixture = fix;
            physComp = component;
            phaseComp = phase;
        }
    }

    internal sealed class CollisionEventArgsHolder
    {
        public EntityUid owner;
        public EntityUid target;
        public string ownerFixtureKey;
        public string targetFixtureKey;
        public Robust.Shared.Physics.Dynamics.Fixture ownerFixture;
        public Robust.Shared.Physics.Dynamics.Fixture targetFixture;
        public Vector2 hitPosition;
        public StartCollideEvent collideEvent;

        public CollisionEventArgsHolder(string ownerKey,string targetKey, Robust.Shared.Physics.Dynamics.Fixture ownerFix, Robust.Shared.Physics.Dynamics.Fixture targetFix)
        {
            ownerFixtureKey = ownerKey;
            targetFixtureKey = targetKey;
            ownerFixture = ownerFix;
            targetFixture = targetFix;
        }

    }
    internal sealed class RaycastThreadBucketHolder
    {
        public float medianCastArea;
        public EntityQuery<PhysicsComponent> physQuery;
        public EntityQuery<FixturesComponent> fixtureQuery;
        public List<RaycastBucket> buckets = new();
    }

    internal const float surfaceLimitPerThread = 10000f;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ProjectilePhasePreventComponent, MoveEvent>(OnMove);
    }

    private void OnInit(EntityUid uid, ProjectilePhasePreventComponent comp, ref MapInitEvent args)
    {
        comp.start = _trans.GetWorldPosition(uid);
    }
    private void OnMove(EntityUid uid, ProjectilePhasePreventComponent comp, ref MoveEvent args)
    {
        if (args.NewPosition != EntityCoordinates.Invalid)
            comp.end = _trans.ToMapCoordinates(args.NewPosition).Position;
        else
            comp.end = Vector2.Zero;
    }
    ValueTask ProcessBucketAsync(RaycastThreadBucketHolder bucket, CancellationToken cancellationToken)
    {
        foreach (var raycast in bucket.buckets)
        {
            var owner = raycast.owner;
            if (TerminatingOrDeleted(owner))
                continue;
            var start = raycast.start;
            var end = raycast.end;
            var angle = (end - start);
            var map = raycast.map;
            var physComp = raycast.physComp;
            CollisionRay ray = new CollisionRay(start, angle, physComp.CollisionMask);

            foreach (var obj in _phys.IntersectRay(map, ray, angle.Length(), owner, false))
            {
                if (obj.HitEntity == raycast.shooter)
                    continue;
                //if (TerminatingOrDeleted(obj.HitEntity))
                //    continue;
                if (!bucket.physQuery.TryGetComponent(obj.HitEntity, out var targPhysComp))
                    continue;
                if (!bucket.fixtureQuery.TryGetComponent(obj.HitEntity, out var targFixtComp))
                    continue;
                var ev = new StartCollideEvent(owner, obj.HitEntity, raycast.fixtureKey,
                    targFixtComp.Fixtures.Keys.First(), raycast.fixture, targFixtComp.Fixtures.Values.First(), physComp,
                    targPhysComp, obj.HitPos);
                CollisionEventArgsHolder holder = new(raycast.fixtureKey, targFixtComp.Fixtures.Keys.First(),
                    raycast.fixture, targFixtComp.Fixtures.Values.First());
                holder.owner = owner;
                holder.target = obj.HitEntity;
                holder.hitPosition = obj.HitPos;
                holder.collideEvent = ev;
                eventQueue.Enqueue(holder);
            }


            raycast.phaseComp.start = end;
        }
        return ValueTask.CompletedTask;
    }

    public override void Update(float frametime)
    {
        var enumerator =
            EntityQueryEnumerator<ProjectilePhasePreventComponent, PhysicsComponent, FixturesComponent,
                ProjectileComponent>();
        var fixtureQuery = GetEntityQuery<FixturesComponent>();
        var physQuery = GetEntityQuery<PhysicsComponent>();
        var metaQuery = GetEntityQuery<MetaDataComponent>();
        var threadBuckets = new List<RaycastThreadBucketHolder>();
        var fillingBucket = new RaycastThreadBucketHolder();
        fillingBucket.fixtureQuery = fixtureQuery;
        fillingBucket.physQuery = physQuery;

        while (enumerator.MoveNext(out var owner, out var phaseComp, out var physComp, out var fixtComp,
                   out var projComp))
        {

            var map = _trans.GetMapId(owner);
            if (map == MapId.Nullspace)
            {
                RemComp<ProjectilePhasePreventComponent>(owner);
                continue;
            }


            if (fillingBucket.medianCastArea > surfaceLimitPerThread)
            {
                threadBuckets.Add(fillingBucket);
                //Logger.Error($"Added bucket with {fillingBucket.buckets.Count} rays and {fillingBucket.medianCastArea} surface");
                fillingBucket = new RaycastThreadBucketHolder();
                fillingBucket.fixtureQuery = fixtureQuery;
                fillingBucket.physQuery = physQuery;
            }

            var start = phaseComp.start;
            var end = phaseComp.end;
            var angle = (end - start);
            var length = angle.Length();
            var surfaceArea = length * length;
            //Logger.Error($"Processing path: starting at {start.X}, {start.Y} and ending at {end.X}, {end.Y}");
            var bucket = new RaycastBucket(fixtComp.Fixtures.Keys.First(), fixtComp.Fixtures.Values.First(), physComp, phaseComp)
            { end = end, start = start };
            bucket.owner = owner;
            bucket.shooter = projComp.Shooter;
            bucket.start = phaseComp.start;
            bucket.end = phaseComp.end;
            bucket.map = map;
            //Logger.Error($"Surface area is {surfaceArea}");
            fillingBucket.medianCastArea += (float) surfaceArea;
            fillingBucket.buckets.Add(bucket);
        }

        if (!threadBuckets.Contains(fillingBucket))
        {
            //Logger.Error($"Added bucket with {fillingBucket.buckets.Count} rays and {fillingBucket.medianCastArea} surface");
            threadBuckets.Add(fillingBucket);
        }

        eventQueue = new();
        //Logger.Error($"Processing {threadBuckets.Count} buckets");
        Parallel.ForEachAsync(threadBuckets, ProcessBucketAsync);
        Logger.Error($"Processing {eventQueue.Count} events!");
        while (eventQueue.TryDequeue(out var eventData))
        {
            RaiseLocalEvent(ref eventData.collideEvent);
            RemComp<ProjectilePhasePreventComponent>(eventData.owner);
        }


    }
}
