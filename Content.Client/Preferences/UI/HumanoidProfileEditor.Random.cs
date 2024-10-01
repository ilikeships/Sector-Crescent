using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using System.Data.SqlTypes;

namespace Content.Client.Preferences.UI
{
    public sealed partial class HumanoidProfileEditor
    {
        private void RandomizeEverything()
        {



            Profile = HumanoidCharacterProfile.RandomWithSpecies(balance : GetBalance());
            UpdateControls();
            IsDirty = true;


            int GetBalance()
            {
                if (Profile == null)
                {
                    Random b = new Random();
                    return b.Next(HumanoidCharacterProfile.DefaultBalance - 1000, HumanoidCharacterProfile.DefaultBalance + 1000);
                }
                int MONEY_AFTER_DEBT = Profile.BankBalance - 500; //under 0, death fee is 500
                if (Profile.BankBalance > 20000) MONEY_AFTER_DEBT = Profile.BankBalance - ((Profile.BankBalance / 100) * 10); //over 20k, death fee is 10 percent. Minimum is 1 since its an int

                if (MONEY_AFTER_DEBT < -2000)
                {
                    MONEY_AFTER_DEBT = -2000;
                }
                return (MONEY_AFTER_DEBT);
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
