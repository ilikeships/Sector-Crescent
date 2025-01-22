using System.Diagnostics.CodeAnalysis;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Content.Shared.Humanoid;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Checks for a job requirement to be met such as playtime.
/// </summary>
public sealed partial class JobRequirementLoadoutEffect : LoadoutEffect
{
    [DataField(required: true)]
    public JobRequirement Requirement = default!;

    public override bool Validate(RoleLoadout loadout, ICommonSession session, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        var playtimes = collection.Resolve<ISharedPlaytimeManager>().GetPlayTimes(session);

        return JobRequirements.TryRequirementMet(Requirement, playtimes, out reason,
            collection.Resolve<IEntityManager>(),
            collection.Resolve<IPrototypeManager>(),
            true, // Frontier: for now we just let assume whitelist? TODO: implement white list
            null,
            Sex.Unsexed,
            "",
            0); // What the fuck is this retarded codeblock where it skips half the checks?
    }
}
