using System.Linq;
using Content.Server._Crescent.Helpers;
using Content.Shared._Crescent;
using Content.Shared._Crescent.DynamicCodes;
using FastAccessors;
using Microsoft.CodeAnalysis;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Crescent.DynamicAcces;

/// <summary>
/// This handles...
/// </summary>
public sealed class DynamicCodeSystem : SharedDynamicCodeSystem
{
    [Dependency] private readonly CrescentHelperSystem _helpers = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transforms = default!;
    public required Random _randomGenerator;
    // Keeps track of instances for every key. If you do not implement support keys wont be recycled.
    private Dictionary<int, int> instancesPerKey = new();
    private HashSet<int> existingKeys = new();
    private HashSet<int> freeKeys = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        _randomGenerator = _random.GetRandom();
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentAdd>(onAdd);
        SubscribeLocalEvent<DynamicCodeHolderComponent, ComponentRemove>(onRemove);
        SubscribeLocalEvent<DynamicAccesGridInitializer, ComponentAdd>(onAdd);
    }


    private void onAdd(EntityUid grid, DynamicAccesGridInitializer component, ComponentAdd eventHandler)
    {
        var prototype = _prototypes.Index(component.accesMapping);
        var codeHolder= new DynamicCodeHolderComponent();
        HashSet<Entity<DynamicCodeHolderComponent>> targets = new();
        foreach(var (key, targetProtos) in prototype.accesIdentifierToEntity)
        {
            var code = retrieveKey();
            AddKeyToComponent(codeHolder, code, key);
            foreach (var prototypeId in targetProtos)
            {
                if (!_prototypes.TryIndex(prototypeId, out var _))
                {
                    Logger.Error($"Could not find prototype {prototypeId} for AccesMapping {component.accesMapping}");
                    continue;
                }

                foreach (var target in targets)
                {
                    var meta = MetaData(target);
                    if (meta.EntityPrototype is not null && meta.EntityPrototype != prototypeId)
                        continue;
                    AddKeyToComponent(target.Comp, code, null);
                }
            }

        }
        RemComp<DynamicAccesGridInitializer>(grid);

    }
    private void onAdd(EntityUid owner, DynamicCodeHolderComponent component, ref ComponentAdd args)
    {
        foreach (var key in component.codes)
        {
            if (!instancesPerKey.ContainsKey(key))
                instancesPerKey.Add(key, 0);
            if (!existingKeys.Contains(key))
                existingKeys.Add(key);
            instancesPerKey[key]++;
        }
    }

    private void onRemove(EntityUid owner, DynamicCodeHolderComponent component, object? args)
    {
        foreach (var key in component.codes)
        {
            instancesPerKey[key]--;
            if (instancesPerKey[key] <= 0)
            {
                instancesPerKey.Remove(key);
                freeKeys.Add(key);
            }
        }
    }

    public void AddKeyToComponent(DynamicCodeHolderComponent component, int key, string? identifier)
    {
        component.codes.Add(key);
        if (identifier is null)
            return;
        if(!component.mappedCodes.ContainsKey(identifier))
            component.mappedCodes.Add(identifier, new HashSet<int>());
        component.mappedCodes[identifier].Add(key);
    }

    public Dictionary<string, int> addDynamicCodes(HashSet<string> identifiers, EntityUid entity)
    {
        DynamicCodeHolderComponent comp = new DynamicCodeHolderComponent();
        Dictionary<string, int> returnDict = new();
        foreach (var id in identifiers)
        {
            var key = retrieveKey();
            AddKeyToComponent(comp, key, id);
            returnDict.Add(id, key);
        }
        AddComp(entity, comp, true);
        return returnDict;
    }

    public void RemoveKeyFromComponent(DynamicCodeHolderComponent component, int key, string? identifier)
    {
        string? containedIn = null;
        if (identifier is not null && component.mappedCodes[identifier].Contains(key))
            containedIn = identifier;
        else
        {
            foreach (var (id, keyList) in component.mappedCodes)
            {
                if (!keyList.Contains(key))
                    continue;
                containedIn = id;
                break;
            }
        }

        if (containedIn is null)
            return;

        component.codes.Remove(key);
        component.mappedCodes[containedIn].Remove(key);

        instancesPerKey[key]--;
        if (instancesPerKey[key] <= 0)
        {
            instancesPerKey.Remove(key);
            releaseKey(key);

        }

    }
    public bool isKeyValid(int key)
    {
        return existingKeys.Contains(key);
    }

    public void releaseKey(int key)
    {
        existingKeys.Remove(key);
        freeKeys.Add(key);

    }

    public int retrieveKey()
    {
        var key = 0;
        if (freeKeys.Any())
        {
            key = freeKeys.First();
            freeKeys.Remove(key);
        }
        else while (true)
        {
            key = _randomGenerator.Next();
            if (!existingKeys.Contains(key))
                break;
        }

        existingKeys.Add(key);
        return key;

    }
}
