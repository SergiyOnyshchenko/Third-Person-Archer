using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Economy
{
    public enum CurrencyType
    {
        Cash,
        Gold,
        KillTags, 
        
        // Class-specific tokens:
        BowToken       = 10,
        CrossbowToken  = 11,
        SpearToken     = 12,
        ShurikenToken  = 13,
        BoomerangToken = 14,
    }
}