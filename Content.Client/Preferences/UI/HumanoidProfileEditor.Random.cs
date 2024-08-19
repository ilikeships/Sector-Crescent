using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using System.Data.SqlTypes;

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
                    Random b = new Random();
                    return HumanoidCharacterProfile.DefaultBalance + b.Next(0, 1000);
                }

                int moneyAfterDebt = Profile.BankBalance - 20000;
                if (moneyAfterDebt < -2000)
                {
                    moneyAfterDebt = -2000;
                }
                return (moneyAfterDebt);
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
