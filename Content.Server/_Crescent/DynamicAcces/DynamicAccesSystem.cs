using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Crescent.DynamicAcces;

/// <summary>
/// This handles...
/// </summary>
public sealed class DynamicAccesSystem : EntitySystem
{

    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _randomSystem = default!;

    private List<long> generatedKeys = new List<long>();
    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    private Tuple<string, long> generateRandomIdentifier()
    {
        long randomGen = _randomSystem.GetRandom().NextInt64();
        while(!validateIdentifier(randomGen))
            randomGen = _randomSystem.GetRandom().NextInt64();
        return new Tuple<string, long>($"DynamicKey-{randomGen}", randomGen);
    }

    private bool validateIdentifier(long Key)
    {
        return generatedKeys.Contains(Key);
    }

    private bool validateIdentifier(string key)
    {
        if(!key.Contains("DynamicKey-"))
            return false;
        var identifiers = key.Split("DynamicKey-");
        if (identifiers.Length > 1)
            return false;
        key = identifiers[0];
        long number = 0;
        if(!long.TryParse(key, out number))
            return false;
        return validateIdentifier(number);
    }

    private string AddNewAcces(string name)
    {
        var generatedKey = generateRandomIdentifier();
        generatedKeys.Add(generatedKey.Item2);
        var newPrototypeString = $@"
- type: entity
  id: {generatedKey.Item1}
  name: {name}";
        _prototypeManager.LoadString(newPrototypeString);
        return generatedKey.Item1;
    }

    // Returns a equivalent list with all names replaced by the identifier of the new prototypes
    private List<string> AddNewAcces(List<string> Names)
    {
        var returnList = new List<string>();
        var generationList = "";
        foreach (var name in Names)
        {
            var generateKey = generateRandomIdentifier();
            generatedKeys.Add(generateKey.Item2);
            returnList.Add(generateKey.Item1);
            generationList += System.Environment.NewLine;
            generationList += $@"
- type: entity
  id: {generateKey.Item1}
  name: {name}";
        }
        _prototypeManager.LoadString(generationList);
        return returnList;
    }


}
