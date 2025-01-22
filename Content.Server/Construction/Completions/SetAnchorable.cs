using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.EntitySystems;
using JetBrains.Annotations;

namespace Content.Server.Construction.Completions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SetAnchorable : IGraphAction
    {
        [DataField("anchor")] public bool canLock { get; private set; } = true;
        [DataField("unanchor")] public bool canUnlock { get; private set; } = true;

        public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
        {
            var anchorable = entityManager.GetComponent<AnchorableComponent>(uid);
            anchorable.Flags = AnchorableFlags.None;
            if (canLock)
                anchorable.Flags |= AnchorableFlags.Anchorable;
            if (canUnlock)
                anchorable.Flags |= AnchorableFlags.Unanchorable;

        }
    }
}
