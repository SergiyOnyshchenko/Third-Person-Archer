using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ElementalData", menuName = "Data/Elemental")]
public class ElementalData : ScriptableObject
{
    [field: SerializeField] public ElementalType Type { get; private set; }
    [field: SerializeField] public int ArrowCount { get; private set; }
    public UnityEvent OnArrowCountModifyed = new UnityEvent();
    public UnityEvent<ElementalData> OnActivated = new UnityEvent<ElementalData>();

    private void Awake()
    {
        Load();
    }

    private void OnEnable()
    {
        Load();
    }

    public void AddArrows(int count)
    {
        ArrowCount += count;
        OnArrowCountModifyed?.Invoke();

        Save();
    }

    public void RemoveArrow()
    {
        if (ArrowCount == 0)
            return;

        ArrowCount--;;
        OnArrowCountModifyed?.Invoke();

        Save();
    }

    public void SendActivationEvent()
    {
        if (ArrowCount <= 0)
            return;

        OnActivated?.Invoke(this);
    }

    public void Save()
    {
        PlayerPrefs.SetInt("elemental_arrow_count_" + Type.ToString(), ArrowCount);
    }

    public void Load()
    {
        ArrowCount = PlayerPrefs.GetInt("elemental_arrow_count_" + Type.ToString());
    }
}
