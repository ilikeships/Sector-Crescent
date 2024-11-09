using System.Numerics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Crescent.InheritGridVelocity;

/// <summary>
/// This handles...
/// </summary>
public sealed class InheritGridVelocitySystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    /// <inheritdoc/>
    ///
    private ISawmill mill = Logger.GetSawmill("igs");
    public override void Initialize()
    {
        SubscribeLocalEvent<InheritGridVelocityComponent, EntParentChangedMessage>(onGridChange);
    }

    private void onGridChange(EntityUid uid, InheritGridVelocityComponent comp, EntParentChangedMessage args)
    {
        mill.Info($"{uid} got EntChanged from {args.OldParent}");
        if (args.Transform.GridUid is null)
        {
            if (!TryComp<PhysicsComponent>(uid, out var physics))
                return;
            if (!TryComp<PhysicsComponent>(args.OldParent, out var parentPhysics))
                return;
            Vector2 linearVec =
                _physics.GetLinearVelocity((EntityUid) args.OldParent, new Vector2(0, 0), parentPhysics);
            mill.Info($"{uid} , applying linear velocity of {linearVec.X} and {linearVec.Y} ");
            _physics.SetLinearVelocity(uid, linearVec);

        }
    }
}
