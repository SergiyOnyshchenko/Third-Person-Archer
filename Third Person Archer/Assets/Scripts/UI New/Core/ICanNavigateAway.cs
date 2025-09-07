#nullable enable
namespace UI.Core
{
    /// <summary>Implement on controllers that sometimes block leaving (e.g., unsaved form).</summary>
    public interface ICanNavigateAway
    {
        bool CanNavigateAway(out string? reason);
    }
}
