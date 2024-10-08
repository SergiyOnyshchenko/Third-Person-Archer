using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSequenceView : MonoBehaviour
{
    [SerializeField] private LevelSequenceViewInstance _prefab;
    [SerializeField] private Transform _layout;

    private void Start()
    {
        LevelSequenceData sequenceData = LevelManager.Instance.CurrentLevel.Sequence;

        if(sequenceData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        LevelData[] levels = sequenceData.Levels;
        LevelData currentLevel = LevelManager.Instance.CurrentLevel;

        for (int i = 0; i < levels.Length; i++)
        {
            LevelSequenceViewInstance instance = Instantiate(_prefab, _layout);

            bool isBoss = false;
            bool isCurrent = false;

            if (sequenceData.HasBossLevel && levels[i] == sequenceData.BossLevel)
                isBoss = true;

            if (currentLevel == levels[i])
                isCurrent = true;

            instance.Init(i + 1, isBoss, isCurrent);
        }
    }
}
