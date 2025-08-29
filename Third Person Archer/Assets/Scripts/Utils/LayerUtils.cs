using UnityEngine;

public static class LayerUtils
{
    public static void SetLayerRecursively(GameObject obj, int layer)
    {
        if (!obj) return;

        var trs = obj.GetComponentsInChildren<Transform>(includeInactive: true);
        for (int i = 0; i < trs.Length; i++)
            trs[i].gameObject.layer = layer;
    }

    public static void SetLayerRecursively(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogError($"Layer '{layerName}' does not exist.");
            return;
        }
        SetLayerRecursively(obj, layer);
    }
}
