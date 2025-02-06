namespace Content.Server._FTL.HeatSeeking;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HeatSeekingComponent : Component
{
    /// <summary>
    /// How far does this fire a raycast onto?
    /// </summary>
    [DataField("seekRange")]
    public float DefaultSeekingRange = 300f;

    [DataField]
    public Angle WeaponArc = Angle.FromDegrees(360);

    /// <summary>
    /// If null it will default to 100.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Angle? RotationSpeed = 100f;

    /// <summary>
    /// What guidance algorithm should this missile use?
    /// Options are "ProportionalNavigation" and "PurePursuit".
    /// Defaults to "PredictiveGuidance".
    /// </summary>
    [DataField]
    public string GuidanceAlgorithm = "PredictiveGuidance";

    /// <summary>
    /// What is this entity targeting?
    /// </summary>
    [DataField]
    public EntityUid? TargetEntity;

    /// <summary>
    /// How fast does the missile accelerate in m/s/s?
    /// </summary>
    [DataField]
    public float Acceleration = 50f;

    /// <summary>
    /// What is the missiles top speed in m/s?
    /// </summary>
    [DataField]
    public float TopSpeed = 50f;

    /// <summary>
    /// What is the missiles initial speed in m/s?
    /// </summary>
    [DataField]
    public float InitialSpeed = 10f;

    /// <summary>
    /// What is the missiles current speed in m/s?
    /// </summary>
    [DataField]
    public float Speed;

    /// <summary>
    /// What is the missiles field of view in degrees?
    /// </summary>
    [DataField]
    public float FOV = 90f;
}
