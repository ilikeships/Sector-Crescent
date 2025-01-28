using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
using Content.Shared.Sound;
using Robust.Shared.Audio;

namespace Content.Server.Factory.Components
{
    [RegisterComponent]
    public sealed partial class FactoryComponent : Component
    {
        [ViewVariables]
        public List<EntityUid> Inserted = new();

        [ViewVariables]
        public int InsertCount = 0;

        [ViewVariables]
        public bool Powered;

        [ViewVariables]
        public bool Active = true;

        [ViewVariables]
        public int ProductionCap = 1;

        [ViewVariables]
        public int Produced = 0;


        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("sound", required: true)]
        public SoundSpecifier? SoundOnProduce;

        [DataField]
        public ProtoId<SinkPortPrototype> Toggle = "Toggle";

        /// <summary>
        /// recipes , takes an entityID and references another to convert into
        /// recipes:
        ///     recipeName:
        ///         recipe:
        ///             inputs:
        ///                 ore:
        ///                     count
        ///                 ore2:
        ///                     count2
        ///             outputs:
        ///                 thing:
        ///                     count1
        ///                 thing2:
        ///                     count2
        ///       
        ///         OutpustList
        /// </summary>
        [DataField("recipes")]
        public List<ProtoId<FactoryRecipe>> Recipes = new();

        public ProtoId<FactoryRecipe>? ChosenRecipe;


    }
}
