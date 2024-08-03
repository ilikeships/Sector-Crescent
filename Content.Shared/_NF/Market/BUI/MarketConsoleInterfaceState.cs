using Robust.Shared.Serialization;

namespace Content.Shared._NF.Market.BUI;

[NetSerializable, Serializable]
public sealed class MarketConsoleInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// The player's balance
    /// </summary>
    public int Balance;

    /// <summary>
    /// The market modifier to apply on to the price.
    /// 0.1 makes prices 10% of their original value.
    /// </summary>
    public float MarketModifier;

    /// <summary>
    /// Data to display
    /// </summary>
    public List<MarketData> MarketDataList;

    /// <summary>
    /// The currently stored cart data
    /// </summary>
    public List<MarketData> CartDataList;

    /// <summary>
    /// The sum of the current cart
    /// </summary>
    public int CartBalance;

    /// <summary>
    /// are the buttons enabled
    /// </summary>
    public bool Enabled;


    public int CostOfTransaction

    public MarketConsoleInterfaceState(int _balance, float _marketModifier, List<MarketData> _marketDataList, List<MarketData> _cartDataList, int _cartBalance, bool _enabled, int _costOfTransaction)
    {
        Balance = _balance;
        MarketModifier = _marketModifier;
        MarketDataList = _marketDataList;
        CartDataList = _cartDataList;
        CartBalance = _cartBalance;
        Enabled = _enabled;
        CostOfTransaction = _costOfTransaction;
    }
}
