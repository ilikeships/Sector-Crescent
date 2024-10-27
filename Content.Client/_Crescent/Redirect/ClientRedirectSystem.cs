using Content.Shared.Crescent.Redirect;
using Robust.Client;

namespace Content.Client.Crescent.Redirect;


public sealed class ClientRedirectSystem : EntitySystem
{


    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RedirectMessage>(OnRedirectMessage);

    }

    private void OnRedirectMessage(RedirectMessage ev)
    {
        IoCManager.Resolve<IGameController>().Redial(ev.RedirectUrl, null);
        return;
    }

}
