using Content.Shared._Crescent.Diplomacy;
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;


namespace Content.Server._Crescent.Diplomacy;

public partial class DiplomacySystem
{
    [Dependency] private readonly IConsoleHost _conHost = default!;

    public void InitializeCommands()
    {
        Logger.Error("Initializing Commands");
        _conHost.RegisterCommand("getfactionrelations", "Gets relations for a given faction", "getfactionrelations <faction ID>",
            GetFactionRelationsCmd);

        _conHost.RegisterCommand("changefactionrelations", "Changes relations between 2 factions", "changefactionrelations <faction 1 ID> <faction 2 ID> <new relation>",
            ChangeFactionRelationsCmd);
    }

    [AdminCommand(AdminFlags.Logs)]
    private void GetFactionRelationsCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("ONE ARGUMENT. ONE FACTION BUDDY!");
            return;
        }

        var dict = GetRelationsForFaction(args[0]);

        foreach (var relation in dict)
        {
            shell.WriteLine(relation.Key + ": " + relation.Value);
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    private void ChangeFactionRelationsCmd(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError("THREE. THERE SHALL BE... THREE ARGUMENTS!");
            return;
        }

        if (!_prototypeManager.TryIndex<DiplomacyPrototype>(args[0], out var _))
        {
            shell.WriteError(args[0] + " is not a faction.");
            return;
        }

        if (!_prototypeManager.TryIndex<DiplomacyPrototype>(args[1], out var _))
        {
            shell.WriteError(args[1] + " is not a faction.");
            return;
        }

        if (!Enum.TryParse(args[2], out Relations relations))
        {
            shell.WriteError(args[2] + " is not a relation.");
            return;
        }

        ChangeRelation(args[0], args[1], relations);
        shell.WriteLine("Relations between " + args[0] + " and " + args[1] + " are now " + GetRelations(args[0], args[1]));
    }

}
