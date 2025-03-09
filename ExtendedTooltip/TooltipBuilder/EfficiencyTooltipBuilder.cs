using ExtendedTooltip.Systems;

using Game.Buildings;
using Game.UI.Tooltip;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class EfficiencyTooltipBuilder : TooltipBuilderBase
	{
		public EfficiencyTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created EfficiencyTooltipBuilder.");
		}

		public void Build(DynamicBuffer<Efficiency> efficiencyBuffer, TooltipGroup tooltipGroup)
		{
			var efficiency = (int)math.round(100f * GetEfficiency(efficiencyBuffer));
			StringTooltip efficiencyTooltip = new()
			{
				icon = "Media/Game/Icons/CompanyProfit.svg",
				value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.EFFICIENCY", "Efficiency") + ": " + efficiency + "%",
				color = efficiency <= 99 ? TooltipColor.Warning : TooltipColor.Success,
			};
			if (Mod.Settings.DisableTooltipIcons)
				efficiencyTooltip.icon = null;
			tooltipGroup.children.Add(efficiencyTooltip);
		}

		public static float GetEfficiency(DynamicBuffer<Efficiency> buffer)
		{
			var num = 1f;
			foreach (var efficiency in buffer)
			{
				num *= math.max(0f, efficiency.m_Efficiency);
			}

			if (num <= 0f)
			{
				return 0f;
			}

			return math.max(0.01f, math.round(100f * num) / 100f);
		}
	}
}
