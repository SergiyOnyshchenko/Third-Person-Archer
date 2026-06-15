using UnityEngine;

public class HidePoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    public void Occupy() => IsOccupied = true;
    public void Release() => IsOccupied = false;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.6f, "sv_icon_dot3_pix16_gizmo", true);
    }
#endif
}
