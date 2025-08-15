using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMissionSelectorInjectable 
{
    void InjectMissionSelector(MissionSelector selector);
}
