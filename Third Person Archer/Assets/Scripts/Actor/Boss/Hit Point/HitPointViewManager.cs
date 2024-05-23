using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HitPointViewManager : MonoBehaviour
{
    [SerializeField] private HitPointView _prefab;
    [SerializeField] private List<HitPoint> _hitPoints = new List<HitPoint>();
    [SerializeField] private List<HitPointView> _views = new List<HitPointView>();

    private void Start()
    {
        InitHitPoints();
    }

    public void InitHitPoints()
    {
        _hitPoints = FindObjectsOfType<HitPoint>(true).ToList();

        foreach (var hitPoint in _hitPoints)
        {
            HitPointView newHitPoint = Instantiate(_prefab, transform);
            newHitPoint.Init(hitPoint);
            _views.Add(newHitPoint);
        }
    }

}
