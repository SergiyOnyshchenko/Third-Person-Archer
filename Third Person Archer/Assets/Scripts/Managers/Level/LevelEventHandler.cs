using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventHandler : MonoBehaviour
{
    public void SendLoadNextLevel()
    {
        LevelEventSystem.SendLoadNextLevel();
    }

    public void SendReloadLevel()
    {
        LevelEventSystem.SendReloadLevel();
    }
}
