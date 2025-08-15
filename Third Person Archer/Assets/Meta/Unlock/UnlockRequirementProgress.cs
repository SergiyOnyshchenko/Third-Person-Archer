using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UnlockRequirementProgress
{
    public MissionType Type;
    public int Completed;
    public int Required;

    public float Progress => Mathf.Clamp01((float)Completed / Required);

    public UnlockRequirementProgress(MissionType type, int completed, int required)
    {
        Type = type;
        Completed = completed;
        Required = required;
    }
}