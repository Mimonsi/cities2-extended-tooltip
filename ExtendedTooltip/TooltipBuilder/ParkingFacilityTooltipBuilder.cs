using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Net;
using Game.Prefabs;
using Game.UI.Tooltip;
using Game.Vehicles;

using System;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class ParkingFacilityTooltipBuilder : TooltipBuilderBase
	{
		public ParkingFacilityTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created ParkingFacilityTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, TooltipGroup tooltipGroup)
		{
			var model = Mod.Settings;

			if (!model.ShowParkingFees && !model.ShowParkingCapacity)
			{
				return;
			}

			var laneCount = 0;
			var parkingCapacity = 0;
			var parkedCars = 0;
			var parkingFee = 0;

			if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<Game.Net.SubLane> dynamicBuffer))
			{
				CheckParkingLanes(dynamicBuffer, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
			}

			if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<Game.Net.SubNet> dynamicBuffer2))
			{
				CheckParkingLanes(dynamicBuffer2, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
			}

			if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<Game.Objects.SubObject> dynamicBuffer3))
			{
				CheckParkingLanes(dynamicBuffer3, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
			}

			// Only if activated
			if (model.ShowParkingFees && parkingFee > 0)
			{
				if (laneCount != 0)
				{
					parkingFee /= laneCount;
				}

				StringTooltip parkingFeesTooltip = new()
				{
					icon = "Media/Game/Icons/ServiceFees.svg",
					value = $"{m_CustomTranslationSystem.GetLocalGameTranslation("Policy.TITLE[Lot Parking Fee]")}: {m_CustomTranslationSystem.GetLocalGameTranslation("Common.VALUE_MONEY", "€", "SIGN", "", "VALUE", parkingFee.ToString())}",
				};
				if (Mod.Settings.DisableTooltipIcons)
					parkingFeesTooltip.icon = null;
				tooltipGroup.children.Add(parkingFeesTooltip);
			}

			// Only if activated
			if (model.ShowParkingCapacity && parkingCapacity > 0)
			{
				if (parkingCapacity < 0)
				{
					parkingCapacity = 0;
				}

				var parkingOccupationPercentage = parkedCars == 0 ? 0 : Convert.ToInt16(math.round(parkedCars * 100 / parkingCapacity));
				StringTooltip parkingOccupationTooltip = new()
				{
					//icon = "Media/Game/Icons/Traffic.svg",
					icon = "Media/Game/Icons/Parking.svg",
					value = $"{m_CustomTranslationSystem.GetTranslation("parkinglots.utilization", "Utilization")}: {parkingOccupationPercentage}% [{parkedCars}/{parkingCapacity}]",
					color = (parkingOccupationPercentage <= 75) ? TooltipColor.Success : (parkingOccupationPercentage <= 90) ? TooltipColor.Warning : TooltipColor.Error,
				};
				if (Mod.Settings.DisableTooltipIcons)
					parkingOccupationTooltip.icon = null;
				tooltipGroup.children.Add(parkingOccupationTooltip);
			}
		}

		private void CheckParkingLanes(DynamicBuffer<Game.Objects.SubObject> subObjects, ref int laneCount, ref int parkingCapacity, ref int parkedCars, ref int parkingFee)
		{
			for (var i = 0; i < subObjects.Length; i++)
			{
				var subObject = subObjects[i].m_SubObject;
				if (m_EntityManager.TryGetBuffer(subObject, true, out DynamicBuffer<Game.Net.SubLane> dynamicBuffer))
				{
					CheckParkingLanes(dynamicBuffer, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
				}

				if (m_EntityManager.TryGetBuffer(subObject, true, out DynamicBuffer<Game.Objects.SubObject> dynamicBuffer2))
				{
					CheckParkingLanes(dynamicBuffer2, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
				}
			}
		}
		private void CheckParkingLanes(DynamicBuffer<Game.Net.SubNet> subNets, ref int laneCount, ref int parkingCapacity, ref int parkedCars, ref int parkingFee)
		{
			for (var i = 0; i < subNets.Length; i++)
			{
				var subNet = subNets[i].m_SubNet;
				if (m_EntityManager.TryGetBuffer(subNet, true, out DynamicBuffer<Game.Net.SubLane> dynamicBuffer))
				{
					CheckParkingLanes(dynamicBuffer, ref laneCount, ref parkingCapacity, ref parkedCars, ref parkingFee);
				}
			}
		}

		private void CheckParkingLanes(DynamicBuffer<Game.Net.SubLane> subLanes, ref int laneCount, ref int parkingCapacity, ref int parkedCars, ref int parkingFee)
		{
			for (var i = 0; i < subLanes.Length; i++)
			{
				var subLane = subLanes[i].m_SubLane;
				if (m_EntityManager.TryGetComponent(subLane, out Game.Net.ParkingLane parkingLane))
				{
					if ((parkingLane.m_Flags & ParkingLaneFlags.VirtualLane) == 0)
					{
						var prefab = m_EntityManager.GetComponentData<PrefabRef>(subLane).m_Prefab;
						var componentData = m_EntityManager.GetComponentData<Curve>(subLane);
						var buffer = m_EntityManager.GetBuffer<LaneObject>(subLane, true);
						var componentData2 = m_EntityManager.GetComponentData<ParkingLaneData>(prefab);
						if (componentData2.m_SlotInterval != 0f)
						{
							var num = (int)math.floor((componentData.m_Length + 0.01f) / componentData2.m_SlotInterval);
							parkingCapacity += num;
						}
						else
						{
							parkingCapacity = -1000000;
						}

						for (var j = 0; j < buffer.Length; j++)
						{
							if (m_EntityManager.HasComponent<ParkedCar>(buffer[j].m_LaneObject))
							{
								parkedCars++;
							}
						}

						parkingFee += parkingLane.m_ParkingFee;
						laneCount++;
					}
				}
				else if (m_EntityManager.TryGetComponent(subLane, out GarageLane garageLane))
				{
					parkingCapacity += garageLane.m_VehicleCapacity;
					parkedCars += garageLane.m_VehicleCount;
					parkingFee += garageLane.m_ParkingFee;
					laneCount++;
				}
			}
		}
	}
}
