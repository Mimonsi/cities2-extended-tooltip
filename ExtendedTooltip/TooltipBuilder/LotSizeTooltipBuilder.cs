using Colossal.Entities;
using ExtendedTooltip.Systems;

using Game.Buildings;
using Game.Prefabs;
using Game.UI.Tooltip;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class LotSizeTooltipBuilder : TooltipBuilderBase
	{

		public LotSizeTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created LotSizeTooltipBuilder.");
		}

		public void Build(Entity entity, TooltipGroup tooltipGroup)
		{
			var lotSize = GetLotSize(entity);
			if (lotSize.x < 0 || lotSize.y < 0)
				return;
			var plotSizeTooltip = new StringTooltip
			{
				icon = "Media/Game/Icons/LotTool.svg",
				value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.LOT_SIZE", "Lot Size") + ": " + lotSize.x + "x" + lotSize.y,
			};
			if (Mod.Settings.DisableTooltipIcons)
				plotSizeTooltip.icon = null;
			tooltipGroup.children.Add(plotSizeTooltip);
		}

		private int2 GetLotSize(Entity entity)
		{
			if (m_EntityManager.TryGetComponent(entity, out PrefabRef prefabRef))
			{
				if (m_EntityManager.TryGetComponent(prefabRef.m_Prefab, out BuildingData buildingData))
				{
					return buildingData.m_LotSize;
				}
			}

			return new int2(-1, -1);
		}
	}
}
