using ExtendedTooltip.Systems;

using Game.Prefabs;
using Game.UI.Tooltip;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace ExtendedTooltip.TooltipBuilder
{
	public class ParkTooltipBuilder : TooltipBuilderBase
	{
		public ParkTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created ParkTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, Entity prefab, TooltipGroup tooltipGroup)
		{
			var model = Mod.Settings;

			if (model.ShowParkMaintenance == false)
			{
				return;
			}

			var maintenance = 0;
			if (UpgradeUtils.TryGetCombinedComponent(m_EntityManager, selectedEntity, prefab, out ParkData parkData))
			{
				var componentData = m_EntityManager.GetComponentData<Game.Buildings.Park>(selectedEntity);
				maintenance = Mathf.CeilToInt(math.select(componentData.m_Maintenance / (float)parkData.m_MaintenancePool, 0f, parkData.m_MaintenancePool == 0) * 100f);
			}

			StringTooltip parkTooltip = new()
			{
				icon = "Media/Game/Icons/ParkMaintenance.svg",
				value = $"{m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.PARK_MAINTENANCE", "Maintenence")}: {maintenance}%",
				color = maintenance <= 40 ? TooltipColor.Error : maintenance <= 60 ? TooltipColor.Warning : TooltipColor.Success,
			};
			if (Mod.Settings.DisableTooltipIcons)
				parkTooltip.icon = null;
			tooltipGroup.children.Add(parkTooltip);
		}
	}
}
