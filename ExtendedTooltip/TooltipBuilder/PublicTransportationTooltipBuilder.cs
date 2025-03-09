using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Objects;
using Game.Routes;
using Game.UI.Tooltip;

using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class PublicTransportationTooltipBuilder : TooltipBuilderBase
	{
		public PublicTransportationTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created PublicTransportationTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, TooltipGroup tooltipGroup)
		{
			var model = Mod.Settings;

			if (model.ShowPublicTransportWaitingPassengers == false && model.ShowPublicTransportWaitingTime == false)
			{
				return;
			}

			var waitingPassengers = 0;
			var averageWaitingTime = 0;
			GetPassengerInfo(selectedEntity, ref averageWaitingTime, ref waitingPassengers);

			if (model.ShowPublicTransportWaitingPassengers)
			{
				StringTooltip waitingPassengersTooltip = new()
				{
					icon = "Media/Game/Icons/Population.svg",
					value = $"{m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.WAITING_PASSENGERS", "Waiting passengers")}: {waitingPassengers}",
				};
				if (Mod.Settings.DisableTooltipIcons)
					waitingPassengersTooltip.icon = null;
				tooltipGroup.children.Add(waitingPassengersTooltip);
			}

			if (model.ShowPublicTransportWaitingTime && waitingPassengers != 0)
			{
				var unit = "s";
				var tooltipColor = TooltipColor.Success;
				if (averageWaitingTime < 0)
				{
					averageWaitingTime = 0;
				}

				if (averageWaitingTime > 120)
				{
					averageWaitingTime = (int)math.round(averageWaitingTime / 60);
					unit = "m";
					tooltipColor = averageWaitingTime < 60 ? TooltipColor.Success : averageWaitingTime < 120 ? TooltipColor.Warning : TooltipColor.Error;
				}

				StringTooltip averageWaitingTimeTooltip = new()
				{
					icon = "Media/Game/Icons/Fastforward.svg",
					value = $"{m_CustomTranslationSystem.GetTranslation("average_waiting_time", "~ waiting time")}: {averageWaitingTime}{unit}",
					color = tooltipColor,
				};
				if (Mod.Settings.DisableTooltipIcons)
					averageWaitingTimeTooltip.icon = null;
				tooltipGroup.children.Add(averageWaitingTimeTooltip);
			}
		}

		private void GetPassengerInfo(Entity selectedEntity, ref int averageWaitingTime, ref int waitingPassengers)
		{
			// Check connected routes
			if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<ConnectedRoute> dynamicBuffer))
			{
				var num = 0;
				for (var i = 0; i < dynamicBuffer.Length; i++)
				{
					if (m_EntityManager.TryGetComponent(dynamicBuffer[i].m_Waypoint, out WaitingPassengers waitingPassengersData))
					{
						waitingPassengers += waitingPassengersData.m_Count;
						num += waitingPassengersData.m_AverageWaitingTime;
					}
				}

				num /= math.max(1, dynamicBuffer.Length);
				num -= num % 5;
				averageWaitingTime += (ushort)num;
			}

			// Check building itself
			if (m_EntityManager.TryGetComponent(selectedEntity, out WaitingPassengers waitingPassengersData2))
			{
				waitingPassengers += waitingPassengersData2.m_Count;
				averageWaitingTime += waitingPassengersData2.m_AverageWaitingTime;
			}

			// Also count sub buildings (e.g extension buildings)
			if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<SubObject> subObjects))
			{
				List<int> averageWaitingTimes = new();
				for (var i = 0; i < subObjects.Length; i++)
				{
					GetPassengerInfo(subObjects[i].m_SubObject, ref averageWaitingTime, ref waitingPassengers);
				}
			}
		}
	}
}
