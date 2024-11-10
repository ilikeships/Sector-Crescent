using System.Diagnostics.CodeAnalysis;
using Content.Shared.Access.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;

namespace Content.Server._Crescent.Helpers;

/// <summary>
/// This handles... helpers! 
/// </summary>
public sealed class CrescentHelperSystem : EntitySystem
{

    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public bool GetPlayerId(EntityUid uid,[NotNullWhen(true)] out IdCardComponent? idCardUid)
    {
        if (_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
        {
            // PDA
            if (EntityManager.TryGetComponent(idUid, out PdaComponent? pda) &&
                TryComp<IdCardComponent>(pda.ContainedId, out var id))
            {
                idCardUid = id;
                return true;
            }
            // ID Card
            if (EntityManager.TryGetComponent(idUid, out id))
            {
                idCardUid = id;
                return true;
            }
        }

        idCardUid = null;
        return false;
    }
}
