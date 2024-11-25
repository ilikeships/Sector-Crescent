using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Components
{
    /// <summary>
    /// Interact with to start piloting a shuttle.
    /// </summary>
    [NetworkedComponent]
    public abstract partial class SharedShuttleConsoleComponent : Component
    {
        public static string DiskSlotName = "disk_slot";

        public static string IdSlotName = "id_slot";

        public List<string> Crewmember = new();
        public List<string> Pilots = new();
        public List<string> Captains = new();
    }

    

    [Serializable, NetSerializable]
    public enum ShuttleConsoleUiKey : byte
    {
        Key,
    }
}
