using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.NamedModules.Components;
using Robust.Server.GameObjects;
using Content.Shared.Shuttles.Components;
using Content.Server.Shuttles.Components;

namespace Content.Server.NamedModules.EntitySystems;

public abstract class NamedModuleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShuttleConsoleComponent, ModuleNamingChangeEvent>(OnNameChange);
    }

    private void OnNameChange(EntityUid consoleUid, ShuttleConsoleComponent comp, ModuleNamingChangeEvent args)
    {
     
        TryComp<NamedModulesComponent>(consoleUid, out var actualComp);
        if (actualComp is null)
            return;
        foreach(var (index, name) in args.NewNames)
        {
            actualComp.ButtonNames[index] = name;
        }
        actualComp.Dirty();
    }

}
