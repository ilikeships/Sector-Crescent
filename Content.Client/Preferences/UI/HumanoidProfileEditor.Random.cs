using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Client.Preferences.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private void RandomizeEverything()
        {



            Profile = HumanoidCharacterProfile.Random(balance : GetBalance());
            UpdateControls();
            IsDirty = true;


            int GetBalance()
            {
                if (Profile == null)
                {
                    return HumanoidCharacterProfile.DefaultBalance;
                }
                return (Profile.BankBalance - 20000);
            }
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
