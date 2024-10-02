using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Roles;

[Prototype("faction")]
public sealed partial class FactionPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    ///     A description string to display in the character menu as an explanation of the department's function.
    /// </summary>
    [DataField("description", required: true)]
    public string Description = default!;

    /// <summary>
    ///     A color representing this department to use for text.
    /// </summary>
    [DataField("color", required: true)]
    public Color Color = default!;

    [ViewVariables(VVAccess.ReadWrite),
     DataField("departments", customTypeSerializer: typeof(PrototypeIdListSerializer<DepartmentPrototype>))]
    public List<string> Departments = new();

    /// <summary>
    /// Departments with a higher weight sorted before other departments in UI.
    /// </summary>
    [DataField("weight")]
    public int Weight { get; private set; } = 0;

    /// <summary>
    /// Frontier - whether or not to show this faction. Defaults to no.
    /// </summary>
    [DataField("enabled")]
    public bool Enabled = false;
}

/// <summary>
/// Sorts <see cref="FactionPrototype"/> appropriately for display in the UI,
/// respecting their <see cref="FactionPrototype.Weight"/>.
/// </summary>
public sealed class FactionUIComparer : IComparer<FactionPrototype>
{
    public static readonly FactionUIComparer Instance = new();

    public int Compare(FactionPrototype? x, FactionPrototype? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        var cmp = -x.Weight.CompareTo(y.Weight);
        if (cmp != 0)
            return cmp;
        return string.Compare(x.ID, y.ID, StringComparison.Ordinal);
    }
}
