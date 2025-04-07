using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Hides the cursor and locks it to the center of the screen.
    /// </summary>
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Shows the cursor and unlocks it.
    /// </summary>
    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Toggles cursor visibility and lock state.
    /// </summary>
    public void ToggleCursor()
    {
        if (Cursor.visible)
            HideCursor();
        else
            ShowCursor();
    }

    /// <summary>
    /// Static access for convenience.
    /// </summary>
    public static void Show() => Instance?.ShowCursor();
    public static void Hide() => Instance?.HideCursor();
    public static void Toggle() => Instance?.ToggleCursor();
}
