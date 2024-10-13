using System.Linq;
using Content.Shared._Crescent.Diplomacy;
using Content.Shared.GameTicking;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.Diplomacy;

public sealed class DiplomacySystem : EntitySystem
{
    private const string DiplomacyEntityPrototype = "CrescentDiplomacy";

    [Dependency]
    private readonly IPrototypeManager _prototypeManager = default!;

    private EntityUid? _diplomacyEntity;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(InitializeDiplomacy);
        SubscribeLocalEvent<DiplomacyComponent, ComponentInit>(InitializeComponent);
    }

    private void InitializeDiplomacy(RoundStartedEvent ev)
    {
        _diplomacyEntity = Spawn(DiplomacyEntityPrototype, MapCoordinates.Nullspace);
    }

    private void InitializeComponent(EntityUid uid, DiplomacyComponent component, ComponentInit args)
    {
        // see how many different diplomacies we have
        var diplomacies = _prototypeManager.EnumeratePrototypes<DiplomacyPrototype>().ToArray();

        // create a grid sized just correctly to hold all of their opinions of each other
        component.DiplomaticSituation = new Relations[diplomacies.Length, diplomacies.Length];

        // track which factions are indexed where
        int i = 0;
        foreach (var diplomacy in diplomacies)
        {
            component.DiplomacyIndicies.Add(diplomacy.ID, i);
            i++;
        }

        // fill in array with default relations
        int x = 0;
        int y = 0;
        while (y < diplomacies.Length)
        {
            if (x == y)
                component.DiplomaticSituation[x, y] = Relations.Ally;
            else
            {
                component.DiplomaticSituation[x, y] = Relations.Neutral;
            }

            x++;
            if (x == diplomacies.Length)
            {
                x = 0;
                y++;
            }
        }
    }
}
