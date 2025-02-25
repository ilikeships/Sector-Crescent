using System.Linq;
using Content.Server._Crescent.DynamicAcces;
using Content.Shared._Crescent;
using Content.Shared._Crescent.DynamicCodes;
using Content.Shared.Shipyard;
using Robust.Shared.Prototypes;

namespace Content.Server.Shipyard.Systems;

public sealed partial class ShipyardSystem : SharedShipyardSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DynamicCodeSystem _code = default!;

    public void InitializeDynamicAcces(EntityUid grid, HashSet<DynamicCodeHolderComponent>? replicateTo, ProtoId<ShipDynamicIdPrototype> preset)
    {
        if(!_prototypeManager.TryIndex(preset, out ShipDynamicIdPrototype? proto))
            return;
        var dict = _code.addDynamicCodes(proto.ShipIds.ToHashSet(), grid);
        if (replicateTo is null)
            return;
        foreach (var component in replicateTo)
        {
            foreach (var (key, code) in dict)
            {
                _code.AddKeyToComponent(component, code, key);
            }
        }

        var targetObjects = new HashSet<Entity<DynamicCodeHolderComponent>>();
        _lookup.GetGridEntities(grid, targetObjects);
        foreach (var target in targetObjects)
        {
            foreach (var (key, code) in dict)
            {
                if (!target.Comp.mappedCodes.ContainsKey(key))
                    continue;
                // wipe the slate to remove pre genned codes from before or other bullshit
                target.Comp.mappedCodes[key].Clear();
                target.Comp.mappedCodes[key].Add(code);

            }
        }
    }
}
