namespace Meta.Weapons
{
    public interface IMissionPreparationService
    {
        /// <summary>
        /// Ensures a weapon of the required class is equipped; auto-equips the best owned choice.
        /// Returns the equipped weapon id or null if none available.
        /// </summary>
        string AutoEquipBestFor(MissionRequirement requirement);
    }

    public class MissionPreparationService : IMissionPreparationService
    {
        private readonly IWeaponSelectionService selection;
        private readonly IEquipmentService equipment;

        public MissionPreparationService(IWeaponSelectionService selection, IEquipmentService equipment)
        {
            this.selection = selection;
            this.equipment = equipment;
        }

        public string AutoEquipBestFor(MissionRequirement requirement)
        {
            var bestId = selection.ChooseBestOwnedWeaponFor(requirement);
            if (string.IsNullOrEmpty(bestId)) return null;

            equipment.Equip(bestId);
            return bestId;
        }
    }
}

