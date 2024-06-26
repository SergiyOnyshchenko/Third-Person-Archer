using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerSkinSelector
{
    PlayerSkinData SkinData { get; }
    event Action<IPlayerSkinSelector> OnSkinSelected;
    ISelector Selector { get; }
}
