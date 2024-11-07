using Content.Shared.Preferences;

namespace Content.Client.Preferences.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private void RandomizeEverything()
        {
            Profile = HumanoidCharacterProfile.RandomWithSpecies(faction: Profile!.Faction is null ? "" : Profile.Faction);
            UpdateControls();
            IsDirty = true;
        }

        private void RandomizeName()
        {
            if (Profile == null) return;
            var name = HumanoidCharacterProfile.GetName(Profile.Species, Profile.Gender);
            SetName(name);
            UpdateNameEdit();
        }
    }
}
