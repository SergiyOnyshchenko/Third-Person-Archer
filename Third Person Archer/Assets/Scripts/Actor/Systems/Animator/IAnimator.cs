using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimator
{
    public void SetTrigger(string name);
    public void SetInteger(string name, int value);
    public void SetFloat(string name, float value);
    public void SetBool(string name, bool value);
    public void SwapAnimatorController(RuntimeAnimatorController animatorController);
}
