namespace Content.Server.Emp;

/// <summary>
/// Affects 
/// </summary>
[RegisterComponent]
[Access(typeof(EmpSystem))]
public sealed partial class CrescentEMPTechnologyTierComponent : Component
{
    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float Range = 1.0f;



    /// <summary>
    /// How long it disables targets in seconds
    /// </summary>
    [DataField("disableDuration"), ViewVariables(VVAccess.ReadWrite)]
    public float DisableDuration = 60f;

    [DataField]
    [AutoNetworkedField]
    public TechnologyLevel TechLevel = TechnologyLevel.MidTech;

    public float GetEMPDurationMultiplier{
        get{
            switch (TechLevel){
                case TechnologyLevel.LowTech:
                return 0.8f;
                break;
                case TechnologyLevel.MidTech:
                return 1.0f;
                break;
                case TechnologyLevel.HiTech:
                return 1.5f;
                break;
                case TechnologyLevel.ClarkeTech:
                return 0.2f;
                break;
            }
            return 1f;
        }
    } 

}




public enum TechnologyLevel : byte // Crescenteroni 
{
    LowTech,
    MidTech,
    HiTech,
    ClarkeTech
}

