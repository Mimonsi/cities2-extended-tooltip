using Colossal.Entities;
using ExtendedTooltip.Systems;
using Game.Prefabs;
using Game.UI.Tooltip;
using Unity.Entities;
using UnityEngine;

namespace ExtendedTooltip.TooltipBuilder
{
	public class VehiclePropertiesTooltipBuilder : TooltipBuilderBase
	{
		public VehiclePropertiesTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created VehiclePropertiesTooltipBuilder.");
		}

		public void Build(Entity entity, TooltipGroup tooltipGroup)
		{
			// Get prefab to access CarData
			if (m_EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
			{
				if (m_EntityManager.TryGetComponent(prefabRef.m_Prefab, out CarData data))
				{
					var maxSpeedLabel = m_CustomTranslationSystem.GetTranslation("vehicleproperties.maxspeed",    "Max Speed");
					var accelerationLabel = m_CustomTranslationSystem.GetTranslation("vehicleproperties.acceleration", "Acceleration");
					var brakingLabel = m_CustomTranslationSystem.GetTranslation("vehicleproperties.braking",     "Braking");

					var propertiesTooltip = new StringTooltip
					{
						icon = "Media/Game/Icons/AdvancedElectricity.svg",
						value = $"{maxSpeedLabel}: {UnitHelper.FormatSpeedLimit(data.m_MaxSpeed)}, " +
						        $"{accelerationLabel}: {data.m_Acceleration}, " +
						        $"{brakingLabel}: {data.m_Braking}"
					};

					tooltipGroup.children.Add(propertiesTooltip);
				}
			}
		}
	}
}
