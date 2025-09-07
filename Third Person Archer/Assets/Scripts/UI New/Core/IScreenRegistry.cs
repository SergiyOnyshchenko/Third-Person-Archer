#nullable enable
using UnityEngine;

namespace UI.Core
{
    public interface IScreenRegistry
    {
        bool TryGetPrefab(string screenId, out ScreenView prefab);
    }
}
