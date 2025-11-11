using Colossal.Entities;
using ExtendedTooltip.Systems;
using Game.UI.Tooltip;
using Unity.Entities;
using UnityEngine;

namespace ExtendedTooltip.TooltipBuilder
{
	public class SpeedTooltipBuilder : TooltipBuilderBase
	{

		public SpeedTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created SpeedTooltipBuilder.");
		}

		public void Build(Entity entity, TooltipGroup tooltipGroup)
		{
			var speed = GetSpeed(entity);
			if (speed == 0)
				return;
			
			var rawSpeed = GetSpeed(entity);
			if (!Mod.Settings.ShowActualSpeed)
				rawSpeed = rawSpeed / 2;
			var speedString = UnitHelper.FormatSpeedLimit(rawSpeed); //Do some processing. The ingame speed is actually twice the real speed in m/s, so the conversion rate is weird.
			
			var plotSizeTooltip = new StringTooltip
			{
				// Electricity.svg
				icon = "Media/Game/Icons/AdvancedElectricity.svg",
				value = $"{m_CustomTranslationSystem.GetLocalGameTranslation("extendedtooltip.speed", "Speed")}: {speedString}"
			};
			tooltipGroup.children.Add(plotSizeTooltip);
		}

		/// <summary>
		/// Returns raw speed
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		private float GetSpeed(Entity entity)
		{
			if (m_EntityManager.TryGetComponent<Game.Objects.Moving>(entity, out var movingComponent))
			{
				var movementVector = new Vector3(movingComponent.m_Velocity.x, movingComponent.m_Velocity.y,
					movingComponent.m_Velocity.z);
				var rawSpeed = movementVector.magnitude;
				return rawSpeed;
			}

			return 0;
		}
	}
}
