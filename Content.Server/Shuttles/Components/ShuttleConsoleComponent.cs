using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Access;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Shuttles.Components
{
    public enum ShuttleConsoleAccesState
    {
        // Always logged in on NotDynamic , since we dont have dynamic acces reader
        NotDynamic, 
        NoAcces,
        PilotAcces,
        CaptainAcces,
    };

    [RegisterComponent]
    public sealed partial class ShuttleConsoleComponent : SharedShuttleConsoleComponent
    {
        [ViewVariables]
        public readonly List<EntityUid> SubscribedPilots = new();

        /// <summary>
        /// How much should the pilot's eye be zoomed by when piloting using this console?
        /// </summary>
        [DataField("zoom")]
        public Vector2 Zoom = new(1.5f, 1.5f);

        /// <summary>
        /// Should this console have access to restricted FTL destinations?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite), DataField("whitelistSpecific")]
        public List<EntityUid> FTLWhitelist = new List<EntityUid>();

        /// <summary>
        /// For EMP to allow keeping the shuttle off
        /// </summary>
        [DataField("enabled")]
        public bool MainBreakerEnabled = true;

        /// <summary>
        ///     While disabled by EMP
        /// </summary>
        [DataField("timeoutFromEmp", customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan TimeoutFromEmp = TimeSpan.Zero;

        [DataField("disableDuration"), ViewVariables(VVAccess.ReadWrite)]
        public float DisableDuration = 60f;

        public ShuttleBoundUserInterfaceState? LastUpdatedState = null;

        [DataField("targetIdSlot")]
        public ItemSlot targetIdSlot = default!;

        public ShuttleConsoleAccesState accesState = ShuttleConsoleAccesState.NotDynamic;

    }
}
