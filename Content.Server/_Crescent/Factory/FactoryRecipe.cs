namespace Content.Server.Factory.Components
{
    [Serializable]
    [DataDefinition]
    public sealed partial class FactoryRecipe
    {
        [DataField("inputs")]
        public Dictionary<string, int> Inputs = new();

        [DataField("outputs")]
        public Dictionary<string, int> Outputs = new();
    }
}
