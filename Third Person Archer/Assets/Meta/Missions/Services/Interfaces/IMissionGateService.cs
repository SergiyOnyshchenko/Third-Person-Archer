using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMissionGateService
{
    MissionGateResult CheckCampaignGate(MissionContext ctx);
}