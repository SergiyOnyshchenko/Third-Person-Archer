using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeRenderer : MonoBehaviour
{
    [SerializeField] private Transform[] _points;
    private LineRenderer _lineRenderer;

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        Vector3[] positions = new Vector3[_points.Length];

        for (int i = 0; i < _points.Length; i++)
            positions[i] = _points[i].position;

        _lineRenderer.SetPositions(positions);
    }
}
