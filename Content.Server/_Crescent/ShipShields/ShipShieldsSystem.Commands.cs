
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;


namespace Content.Server._Crescent.ShipShields;
public partial class ShipShieldsSystem
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void InitializeCommands()
    {
        _conHost.RegisterCommand("shieldentity", "Create a shield around an entity", "shieldentity <uid>",
            ShieldEntityCmd);
    }

    [AdminCommand(AdminFlags.Debug)]
    public void ShieldEntityCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (!EntityUid.TryParse(args[0], out var uid))
        {
            shell.WriteError("Couldn't parse entity.");
            return;
        }

        var shield = ShieldEntity(uid);

        shell.WriteLine("Created shield " + shield);
    }
}
