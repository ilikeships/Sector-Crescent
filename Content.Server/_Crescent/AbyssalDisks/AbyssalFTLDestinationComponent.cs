namespace Content.Server._Crescent.AbyssalDisks;

[RegisterComponent]
public sealed partial class AbyssalFTLDestinationComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Cooldown = 300f;

    public float Ticker;
}
