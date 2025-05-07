using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtentions 
{
    public static bool IntToBool(int value)
    {
        return value != 0; 
    }

    // Convert bool to int
    public static int BoolToInt(bool value)
    {
        return value ? 1 : 0;
    }
}
