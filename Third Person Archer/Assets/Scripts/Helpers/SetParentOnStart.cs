using UnityEngine;

public class SetParentOnStart : MonoBehaviour
{
    [Header("Target parent to set on Start")]
    [SerializeField] private Transform _newParent;

    [Header("Options")]
    [Tooltip("Keep world position the same after reparenting.")]
    [SerializeField] private bool _worldPositionStays = true;

    private void Start()
    {
        if (_newParent == null)
        {
            Debug.LogWarning($"{nameof(SetParentOnStart)} on {gameObject.name}: No parent assigned.");
            return;
        }

        transform.SetParent(_newParent, _worldPositionStays);
    }
}


