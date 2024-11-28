using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class SwitchedToCrewHudMessage(bool visible) : BoundUserInterfaceMessage
{
    public bool Visible = visible;
}

public enum EmployeeOptions
{
    Crew,
    Pilot,
    Captain,
}

[Serializable, NetSerializable]
public sealed class TryMakeEmployeeMessage(EmployeeOptions option) : BoundUserInterfaceMessage
{
    public EmployeeOptions chosenOption = option;
}

[Serializable, NetSerializable]
public sealed class CrewInterfaceState
{
    public List<string> RegisteredCrewmembers;
    public List<string> RegisteredPilots;
    public List<string> RegisteredCaptains;
    public string IdName;
    public bool hasId;


    public CrewInterfaceState(List<string> Crew, List<string>Pilots, List<string>Captains, bool hasId,string name)
    {
        RegisteredCrewmembers = Crew;
        RegisteredPilots = Pilots;
        RegisteredCaptains = Captains;
        this.hasId = hasId;
        IdName = name;
    }
}
