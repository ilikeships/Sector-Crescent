using Content.Server.Access.Components;
using Content.Shared.Access.Components;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server.Access.Systems;

public sealed class IdExaminableSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdExaminableComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, IdExaminableComponent component, ExaminedEvent args)
    {
        var info = GetInfo(uid) ?? Loc.GetString("id-examinable-component-verb-no-id");
        args.PushMessage(FormattedMessage.FromMarkup(info));
    }

    private string? GetInfo(EntityUid uid)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
        {
            // PDA
            if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                TryComp<IdCardComponent>(pda.ContainedId, out var id))
            {
                return GetNameAndJob(id);
            }
            // ID Card
            if (EntityManager.TryGetComponent(idUid, out id))
            {
                return GetNameAndJob(id);
            }
        }
        return null;
    }

    private string GetNameAndJob(IdCardComponent id)
    {
        var jobSuffix = string.IsNullOrWhiteSpace(id.JobTitle) ? string.Empty : $" ({id.JobTitle})";

        var val = string.IsNullOrWhiteSpace(id.FullName)
            ? Loc.GetString("access-id-card-component-owner-name-job-title-text",
                ("jobSuffix", jobSuffix))
            : Loc.GetString("access-id-card-component-owner-full-name-job-title-text",
                ("fullName", id.FullName),
                ("jobSuffix", jobSuffix));

        return val;
    }
}
