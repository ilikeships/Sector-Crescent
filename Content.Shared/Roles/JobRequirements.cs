using System.Diagnostics.CodeAnalysis;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles.Jobs;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Humanoid;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Shared.Roles
{
    /// <summary>
    /// Abstract class for playtime and other requirements for role gates.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    [Serializable, NetSerializable]
    public abstract partial class JobRequirement { }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class DepartmentTimeRequirement : JobRequirement
    {
        /// <summary>
        /// Which department needs the required amount of time.
        /// </summary>
        [DataField("department", customTypeSerializer: typeof(PrototypeIdSerializer<DepartmentPrototype>))]
        public string Department = default!;

        /// <summary>
        /// How long (in seconds) this requirement is.
        /// </summary>
        [DataField("time")] public TimeSpan Time;

        /// <summary>
        /// If true, requirement will return false if playtime above the specified time.
        /// </summary>
        /// <value>
        /// <c>False</c> by default.<br />
        /// <c>True</c> for invert general requirement
        /// </value>
        [DataField("inverted")] public bool Inverted;
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class RoleTimeRequirement : JobRequirement
    {
        /// <summary>
        /// What particular role they need the time requirement with.
        /// </summary>
        [DataField("role", customTypeSerializer: typeof(PrototypeIdSerializer<PlayTimeTrackerPrototype>))]
        public string Role = default!;

        /// <inheritdoc cref="DepartmentTimeRequirement.Time"/>
        [DataField("time")] public TimeSpan Time;

        /// <inheritdoc cref="DepartmentTimeRequirement.Inverted"/>
        [DataField("inverted")] public bool Inverted;
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class OverallPlaytimeRequirement : JobRequirement
    {
        /// <inheritdoc cref="DepartmentTimeRequirement.Time"/>
        [DataField("time")] public TimeSpan Time;

        /// <inheritdoc cref="DepartmentTimeRequirement.Inverted"/>
        [DataField("inverted")] public bool Inverted;
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class SpeciesRequirement : JobRequirement
    {
        [DataField("allowedOnly")] public HashSet<ProtoId<SpeciesPrototype>>? AllowedOnly;
        [DataField("notAllowed")] public HashSet<ProtoId<SpeciesPrototype>>? NotAllowed;
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class SexRequirement : JobRequirement
    {
        [DataField("allowed")] public List<Sex> Allowed;
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class FactionRequirement : JobRequirement
    {
        [DataField("factionID")] public string FactionID = "";
    }

    [UsedImplicitly]
    [Serializable, NetSerializable]
    public sealed partial class WealthRequirement : JobRequirement
    {
        [DataField("below")] public int below = 999999999;
        [DataField("above")] public int above = 0;
    }




    public static class JobRequirements
    {
        public static bool TryRequirementsMet(
            JobPrototype job,
            Dictionary<string, TimeSpan>? playTimes,
            [NotNullWhen(false)] out FormattedMessage? reason,
            IEntityManager entManager,
            IPrototypeManager prototypes,
            bool isWhitelisted,
            string? species,
            Sex sex,
            string faction,
            int wealth)
        {
            reason = null;
            if (job.Requirements == null)
                return true;

            foreach (var requirement in job.Requirements)
            {
                if (!TryRequirementMet(requirement, playTimes, out reason, entManager, prototypes, isWhitelisted, species, sex, faction, wealth))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a string with the reason why a particular requirement may not be met.
        /// </summary>
        public static bool TryRequirementMet(
            JobRequirement requirement,
            IReadOnlyDictionary<string, TimeSpan>? playTimes,
            [NotNullWhen(false)] out FormattedMessage? reason,
            IEntityManager entManager,
            IPrototypeManager prototypes,
            bool isWhitelisted,
            string? species,
            Sex sex,
            string faction,
            int wealth)
        {
            reason = null;

            switch (requirement)
            {
                case FactionRequirement factRequirement:
                    if (factRequirement.FactionID != "" && faction != "" && faction != factRequirement.FactionID)
                    {
                        reason = FormattedMessage.FromMarkup($"Your faction is not {factRequirement.FactionID}");
                        return false;
                    }
                    return true;

                case DepartmentTimeRequirement deptRequirement:
                    if (playTimes == null)
                    {
                        return true;
                    }

                    var playtime = TimeSpan.Zero;

                    // Check all jobs' departments
                    var department = prototypes.Index<DepartmentPrototype>(deptRequirement.Department);
                    var jobs = department.Roles;
                    string proto;

                    // Check all jobs' playtime
                    foreach (var other in jobs)
                    {
                        // The schema is stored on the Job role but we want to explode if the timer isn't found anyway.
                        proto = prototypes.Index<JobPrototype>(other).PlayTimeTracker;

                        playTimes.TryGetValue(proto, out var otherTime);
                        playtime += otherTime;
                    }

                    var deptDiff = deptRequirement.Time.TotalMinutes - playtime.TotalMinutes;

                    if (!deptRequirement.Inverted)
                    {
                        if (deptDiff <= 0)
                            return true;

                        reason = FormattedMessage.FromMarkup(Loc.GetString(
                            "role-timer-department-insufficient",
                            ("time", Math.Ceiling(deptDiff)),
                            ("department", Loc.GetString(deptRequirement.Department)),
                            ("departmentColor", department.Color.ToHex())));
                        return false;
                    }
                    else
                    {
                        if (deptDiff <= 0)
                        {
                            reason = FormattedMessage.FromMarkup(Loc.GetString(
                                "role-timer-department-too-high",
                                ("time", -deptDiff),
                                ("department", Loc.GetString(deptRequirement.Department)),
                                ("departmentColor", department.Color.ToHex())));
                            return false;
                        }

                        return true;
                    }

                case OverallPlaytimeRequirement overallRequirement:
                    if (playTimes == null)
                    {
                        return true;
                    }

                    var overallTime = playTimes.GetValueOrDefault(PlayTimeTrackingShared.TrackerOverall);
                    var overallDiff = overallRequirement.Time.TotalMinutes - overallTime.TotalMinutes;

                    if (!overallRequirement.Inverted)
                    {
                        if (overallDiff <= 0 || overallTime >= overallRequirement.Time)
                            return true;

                        reason = FormattedMessage.FromMarkup(Loc.GetString(
                              "role-timer-overall-insufficient",
                              ("time", Math.Ceiling(overallDiff))));
                        return false;
                    }
                    else
                    {
                        if (overallDiff <= 0 || overallTime >= overallRequirement.Time)
                        {
                            reason = FormattedMessage.FromMarkup(Loc.GetString("role-timer-overall-too-high", ("time", -overallDiff)));
                            return false;
                        }

                        return true;
                    }

                case RoleTimeRequirement roleRequirement:
                    if (playTimes == null)
                    {
                        return true;
                    }

                    proto = roleRequirement.Role;

                    playTimes.TryGetValue(proto, out var roleTime);
                    var roleDiff = roleRequirement.Time.TotalMinutes - roleTime.TotalMinutes;
                    var departmentColor = Color.Yellow;

                    if (entManager.EntitySysManager.TryGetEntitySystem(out SharedJobSystem? jobSystem))
                    {
                        var jobProto = jobSystem.GetJobPrototype(proto);

                        if (jobSystem.TryGetDepartment(jobProto, out var departmentProto))
                            departmentColor = departmentProto.Color;
                    }

                    if (!roleRequirement.Inverted)
                    {
                        if (roleDiff <= 0)
                            return true;

                        reason = FormattedMessage.FromMarkup(Loc.GetString(
                            "role-timer-role-insufficient",
                            ("time", Math.Ceiling(roleDiff)),
                            ("job", Loc.GetString(proto)),
                            ("departmentColor", departmentColor.ToHex())));
                        return false;
                    }
                    else
                    {
                        if (roleDiff <= 0)
                        {
                            reason = FormattedMessage.FromMarkup(Loc.GetString(
                                "role-timer-role-too-high",
                                ("time", -roleDiff),
                                ("job", Loc.GetString(proto)),
                                ("departmentColor", departmentColor.ToHex())));
                            return false;
                        }

                        return true;
                    }
                case WhitelistRequirement _: // DeltaV - Whitelist requirement
                    if (isWhitelisted)
                        return true;

                    reason = FormattedMessage.FromMarkup(Loc.GetString("playtime-deny-reason-not-whitelisted"));
                    return false;
                case SpeciesRequirement speciesRequirement:
                    if (species == null)
                    {
                        return true;
                    }

                    if (speciesRequirement.AllowedOnly != null && speciesRequirement.AllowedOnly.Count > 0)
                    {
                        if (species.Length > 0 && speciesRequirement.AllowedOnly.Contains(species))
                        {
                            return true;
                        }
                    }
                    else if (speciesRequirement.NotAllowed != null && speciesRequirement.NotAllowed.Count > 0)
                    {
                        if (species.Length == 0 || !speciesRequirement.NotAllowed.Contains(species))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }

                    reason = FormattedMessage.FromMarkup(Loc.GetString("job-requirement-species-not-allowed", ("species", species)));
                    return false;

                case SexRequirement sexRequirement: // Crescent: Sex restriction
                    if (sexRequirement.Allowed.Contains(sex))
                        return true;

                    reason = FormattedMessage.FromMarkup(Loc.GetString("job-requirement-sex"));
                    return false;
                case WealthRequirement wealthRequirement: // Crescent: Wealth restriction
                    if()

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
