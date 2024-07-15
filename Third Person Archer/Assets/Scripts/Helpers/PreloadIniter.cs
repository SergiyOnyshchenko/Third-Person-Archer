using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreloadIniter : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        yield return new WaitForSeconds(0.2f);
        LevelEventSystem.SendLoadMainMenu();
    }
}
