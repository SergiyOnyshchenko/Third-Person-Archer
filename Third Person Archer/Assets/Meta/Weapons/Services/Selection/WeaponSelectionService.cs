using System.Linq;

namespace Meta.Weapons
{
    public interface IWeaponSelectionService
    {
        /// <summary>Returns the best OWNED weapon id of the required class (or null if none).</summary>
        string ChooseBestOwnedWeapon(WeaponClass requiredClass);

        /// <summary>Same as above but uses mission requirement-aware scoring.</summary>
        string ChooseBestOwnedWeaponFor(MissionRequirement requirement);
    }

    public class WeaponSelectionService : IWeaponSelectionService
    {
        private readonly IWeaponRepository repository;
        private readonly IStatsService stats;
        private readonly WeaponScoringConfig scoring;
        private readonly WeaponDef[] catalog;

        public WeaponSelectionService(IWeaponRepository repository, IStatsService stats, WeaponScoringConfig scoring, WeaponDef[] catalog)
        {
            this.repository = repository;
            this.stats = stats;
            this.scoring = scoring;
            this.catalog = catalog;
        }

        public string ChooseBestOwnedWeapon(WeaponClass requiredClass)
        {
            var state = repository.Load();

            var best = state.Weapons
                .Where(w => w.Owned)
                .Select(w => new { inst = w, def = catalog.FirstOrDefault(d => d.Id == w.WeaponId) })
                .Where(x => x.def != null && x.def.Class == requiredClass)
                .Select(x => new { x.def.Id, score = scoring.ScoreBase(stats.Compute(x.def, x.inst)) })
                .OrderByDescending(x => x.score)
                .FirstOrDefault();

            return best?.Id;
        }

        public string ChooseBestOwnedWeaponFor(MissionRequirement requirement)
        {
            var state = repository.Load();

            var best = state.Weapons
                .Where(w => w.Owned)
                .Select(w => new { inst = w, def = catalog.FirstOrDefault(d => d.Id == w.WeaponId) })
                .Where(x => x.def != null && x.def.Class == requirement.RequiredClass)
                .Select(x => new { x.def.Id, score = scoring.ScoreWithRequirement(stats.Compute(x.def, x.inst), requirement) })
                .OrderByDescending(x => x.score)
                .FirstOrDefault();

            return best?.Id;
        }
    }
}

