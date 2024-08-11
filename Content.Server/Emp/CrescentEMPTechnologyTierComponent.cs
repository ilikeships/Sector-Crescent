namespace Content.Server.Emp;

/// <summary>
/// Affects how long a grid (ship or station) is affected by an EMP. just add it as a component (CrescentEMPTechnologyTier) and add TechLevel as a field then follow with : LowTech or whatnot. add it on the grid via mapping
/// </summary>
[RegisterComponent]
[Access(typeof(EmpSystem))]
public sealed partial class CrescentEMPTechnologyTierComponent : Component
{


    [DataField]
    [AutoNetworkedField]
    public float EMPMultiplier = 1;

[DataField]
    [AutoNetworkedField]
    public float PowerLossMarkiplier = 1;

}



