using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Net;
using Game.Prefabs;
using Game.UI.Tooltip;

using System;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class RoadTooltipBuilder : TooltipBuilderBase
	{
		public RoadTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created RoadTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, TooltipGroup tooltipGroup)
		{

			var model = Mod.Settings;

			var length = 0f;
			var worstCondition = 0f;
			var bestCondition = 100f;
			var condition = 0f;
			var upkeep = 0f;

			/*float4 trafficFlowDuration0 = default;
			float4 trafficFlowDuration1 = default;
			float4 trafficFlowDistance0 = default;
			float4 trafficFlowDistance1 = default;

			float[] volume = new float[5];
			float[] flow = new float[5];

			// Don't ask :D
			TimeSystem timeSystem = EntityManager.World.GetOrCreateSystemManaged<TimeSystem>();
			float num = timeSystem.normalizedTime * 4f;
			float4 timeFactors = new(math.max(num - 3f, 1f - num), 1f - math.abs(num - new float3(1f, 2f, 3f)));
			timeFactors = math.saturate(timeFactors);*/

			var buffer = m_EntityManager.GetBuffer<AggregateElement>(selectedEntity, true);
			for (var i = 0; i < buffer.Length; i++)
			{

				var edge = buffer[i].m_Edge;
				if (model.ShowRoadLength && m_EntityManager.TryGetComponent(edge, out Road _) && m_EntityManager.TryGetComponent(edge, out Curve curve))
				{
					length += curve.m_Length;

					/* if (EntityManager.TryGetBuffer(edge, true, out DynamicBuffer<Game.Net.SubLane> subLaneBuffer)) {
						trafficFlowDuration0 = default;
						trafficFlowDuration1 = default;
						trafficFlowDistance0 = default;
						trafficFlowDistance1 = default;

						for (int j = 0; j < subLaneBuffer.Length; j++)
						{
							Entity subLane = subLaneBuffer[j].m_SubLane;
							if (EntityManager.TryGetComponent<LaneFlow>(subLane, out var laneFlow))
							{
								// float4 trafficFlowSpeed2 = NetUtils.GetTrafficFlowSpeed(laneFlow.m_Duration, laneFlow.m_Distance);
								// float duration = math.dot(road.m_TrafficFlowDuration0 + road.m_TrafficFlowDuration1, timeFactors) * 2.6666667f;
								// float distance = math.dot(road.m_TrafficFlowDistance0 + road.m_TrafficFlowDistance1, timeFactors) * 2.6666667f;
								// float trafficFlowSpeed2 = NetUtils.GetTrafficFlowSpeed(duration, distance);

								float2 float3;
								if (EntityManager.TryGetComponent(subLane, out EdgeLane edgeLane))
								{
									float3 = math.select(0f, 1f, new bool2(math.any(edgeLane.m_EdgeDelta == 0f), math.any(edgeLane.m_EdgeDelta == 1f)));
								}
								else
								{
									float3 = math.select(1f, 0.33333334f, false);
								}
								trafficFlowDuration0 += laneFlow.m_Duration * float3.x;
								trafficFlowDuration1 += laneFlow.m_Duration * float3.y;
								trafficFlowDistance0 += laneFlow.m_Distance * float3.x;
								trafficFlowDistance1 += laneFlow.m_Distance * float3.y;
							}
						}
					}

					float4 @float = (trafficFlowDistance0 + trafficFlowDistance1) * 16f;
					float4 float2 = NetUtils.GetTrafficFlowSpeed(road) * 100f;

					volume[0] += @float.x * 4f / 24f;
					volume[1] += @float.y * 4f / 24f;
					volume[2] += @float.z * 4f / 24f;
					volume[3] += @float.w * 4f / 24f;
					flow[0] += float2.x;
					flow[1] += float2.y;
					flow[2] += float2.z;
					flow[3] += float2.w;*/
				}

				if (m_EntityManager.TryGetComponent(edge, out NetCondition netCondition))
				{
					var wear = netCondition.m_Wear;
					if (wear.x > worstCondition)
					{
						worstCondition = wear.x;
					}

					if (wear.y > worstCondition)
					{
						worstCondition = wear.y;
					}

					if (wear.x < bestCondition)
					{
						bestCondition = wear.x;
					}

					if (wear.y < bestCondition)
					{
						bestCondition = wear.y;
					}

					condition += math.csum(wear) * 0.5f;
				}

				if (model.ShowRoadUpkeep && m_EntityManager.TryGetComponent(edge, out PrefabRef prefabRef) && m_EntityManager.TryGetComponent(prefabRef.m_Prefab, out PlaceableNetData placeableNetData))
				{
					upkeep += placeableNetData.m_DefaultUpkeepCost;
				}
			}

			// No need to calc if both are disabled
			if (model.ShowRoadUpkeep == false && model.ShowRoadCondition == false && model.ShowRoadLength == false)
			{
				return;
			}

			// Traffic volume and flow need some work
			/*volume[0] /= buffer.Length;
	            volume[1] /= buffer.Length;
	            volume[2] /= buffer.Length;
	            volume[3] /= buffer.Length;
	            volume[4] = volume[0];
	            flow[0] /= buffer.Length;
	            flow[1] /= buffer.Length;
	            flow[2] /= buffer.Length;
	            flow[3] /= buffer.Length;
	            flow[4] = flow[0];

	            UnityEngine.Debug.Log(string.Join(", ", volume));
	            UnityEngine.Debug.Log(string.Join(", ", flow));*/

			if (model.ShowRoadLength)
			{
				// 1 = km , 0 = m
				var lengthFormat = length >= 1000 ? 1 : 0;
				var finalLength = (length >= 1000 ? math.round(length / 1000) : math.round(length)).ToString();
				StringTooltip lengthTooltip = new()
				{
					icon = "Media/Game/Icons/OutsideConnections.svg",
					value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.LINE_VISUALIZER_LENGTH", "Length") + ": " +
					m_CustomTranslationSystem.GetLocalGameTranslation(lengthFormat == 1 ? "Common.VALUE_KILOMETER" : "Common.VALUE_METER", lengthFormat == 1 ? " km" : " m", "SIGN", "", "VALUE", finalLength)
				};
				if (Mod.Settings.DisableTooltipIcons)
					lengthTooltip.icon = null;
				tooltipGroup.children.Add(lengthTooltip);
			}

			if (model.ShowRoadUpkeep)
			{
				int finalUpkeep = Convert.ToInt16(upkeep);
				StringTooltip upkeepTooltip = new()
				{
					icon = "Media/Game/Icons/ServiceUpkeep.svg",
					value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.UPKEEP", "Upkeep") + ": " +
					m_CustomTranslationSystem.GetLocalGameTranslation(
						"Common.VALUE_MONEY_PER_MONTH", finalUpkeep.ToString() + " /month",
					"SIGN", "",
						"VALUE", finalUpkeep.ToString()
					),
				};
				if (Mod.Settings.DisableTooltipIcons)
					upkeepTooltip.icon = null;
				tooltipGroup.children.Add(upkeepTooltip);
			}

			if (model.ShowRoadCondition)
			{
				bestCondition = 100f - (bestCondition / 10f * 100f);
				worstCondition = 100f - (worstCondition / 10f * 100f);
				condition = condition / 10f * 100f;
				condition = 100f - (condition / buffer.Length);

				int finalAvgCondition = Convert.ToInt16(math.round(condition));
				int finalWorstCondition = Convert.ToInt16(math.round(worstCondition));
				int finalBestCondition = Convert.ToInt16(math.round(bestCondition));
				StringTooltip conditionTooltip = new()
				{
					icon = "Media/Game/Icons/RoadsServices.svg",
					value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.ROAD_CONDITION", "Condition") + ": ~" + finalAvgCondition + $"% ({finalWorstCondition}% - {finalBestCondition}%)",
					color = finalAvgCondition < 66 ? TooltipColor.Error : finalAvgCondition < 85 ? TooltipColor.Warning : finalAvgCondition < 95 ? TooltipColor.Info : TooltipColor.Success,
				};
				if (Mod.Settings.DisableTooltipIcons)
					conditionTooltip.icon = null;
				tooltipGroup.children.Add(conditionTooltip);
			}
		}
	}
}
