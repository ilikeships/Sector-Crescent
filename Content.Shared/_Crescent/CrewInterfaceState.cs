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
    public string IdName;
    public bool hasId;
    public bool isCrew;
    public bool isPilot;
    public bool isCaptain;


    public CrewInterfaceState(bool isCrew, bool isPilot, bool isCaptain, bool hasId,string name)
    {
        this.hasId = hasId;
        IdName = name;
        this.isCaptain = isCaptain;
        this.isCrew = isCrew;
        this.isPilot = isPilot;
    }
}
