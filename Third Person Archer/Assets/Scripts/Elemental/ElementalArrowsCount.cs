using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class ElementalArrowsCount : SingleProperty<int>
    {
        public void Decrease()
        {
            SetValue(_value - 1);
        }
    }
}