using System;
using System.Collections.Generic;

[Serializable]
public class UnlockCondition
{
    public List<UnlockRequirement> Requirements = new();
    public int UnlockAfterIndex = 0;
    public UnlockLogicType LogicType = UnlockLogicType.Any;
}