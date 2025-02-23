using Content.Server._Crescent.DynamicAcces;
using Content.Shared._Crescent;
using Content.Shared.Shipyard;

namespace Content.Server.Shipyard.Systems;

public sealed partial class ShipyardSystem : SharedShipyardSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DynamicCodeSystem _code = default!;

    public void InitializeDynamicAcces(EntityUid grid, HashSet<DynamicCodeHolderComponent>? replicateTo)
    {
        var dict = _code.addDynamicCodes(_crescent.EmployeeAccesNamesList, grid);
        if (replicateTo is null)
            return;
        foreach (var component in replicateTo)
        {
            foreach (var (key, code) in dict)
            {
                _code.AddKeyToComponent(component, code, key);
            }
        }
    }
}
