#nullable enable
namespace UI.Core
{
    public interface IReceivesArgs<in TArgs>
    {
        /// <summary>Validate before navigation.</summary>
        bool ValidateArgs(TArgs args);
        /// <summary>Apply args when opening (called after instantiate / before show).</summary>
        void ApplyArgs(TArgs args);
    }
}