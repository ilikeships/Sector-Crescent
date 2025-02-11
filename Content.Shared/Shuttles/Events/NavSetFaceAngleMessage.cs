
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
public class SetTargetPositionFace : BoundUserInterfaceMessage
{
    public Angle TargetAngle;
}
