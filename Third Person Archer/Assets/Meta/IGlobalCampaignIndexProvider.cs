using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGlobalCampaignIndexProvider
{
    int GetGlobalCampaignIndex(MissionData mission);
}