using Content.Shared.Examine;

namespace Content.Shared.Crescent.Vouchers;

public abstract partial class SharedVoucherSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShipVoucherComponent, ExaminedEvent>(OnShipVoucherExamined);
    }

    private void OnShipVoucherExamined(EntityUid uid, ShipVoucherComponent component, ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("ship-voucher-examine", ("ship", component.Ship)));
    }
}
