using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Server._Crescent.ProximityFuse

[RegisterComponent]
public sealed partial class ProximityFuseComponent : Component
{
    [DataField]
    public float MaxRange = 10f;

    [DataField]
    public float MinRange = 1f;
}
