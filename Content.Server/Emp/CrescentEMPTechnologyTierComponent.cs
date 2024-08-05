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

