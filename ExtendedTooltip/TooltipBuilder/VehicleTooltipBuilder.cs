using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Citizens;
using Game.Creatures;
using Game.Objects;
using Game.Prefabs;
using Game.UI.InGame;
using Game.UI.Tooltip;
using Game.Vehicles;

using System.Globalization;

using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace ExtendedTooltip.TooltipBuilder
{
	internal class VehicleTooltipBuilder : TooltipBuilderBase
	{
		public VehicleTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info("Created VehicleTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, Entity prefab, TooltipGroup tooltipGroup)
		{
			var isPersonalCar = m_EntityManager.HasComponent<Game.Vehicles.PersonalCar>(selectedEntity);
			var isPoliceCar = m_EntityManager.HasComponent<Game.Vehicles.PoliceCar>(selectedEntity);
			var isGarbageTruck = m_EntityManager.HasComponent<Game.Vehicles.GarbageTruck>(selectedEntity);
			var isPostVan = m_EntityManager.HasComponent<Game.Vehicles.PostVan>(selectedEntity);
			var isPublicTransport = m_EntityManager.HasComponent<Game.Vehicles.PublicTransport>(selectedEntity);
			var isTaxi = m_EntityManager.HasComponent<Game.Vehicles.Taxi>(selectedEntity);
			var isParked = m_EntityManager.HasComponent<ParkedCar>(selectedEntity);

			var settings = Mod.Settings;

			if (settings.ShowVehicleState && !isParked)
			{
				if (isPersonalCar)
				{
					// Personal Car
					if (m_EntityManager.TryGetComponent(selectedEntity, out Game.Vehicles.PersonalCar personalCar) && !m_EntityManager.HasComponent<ParkedCar>(selectedEntity)
					    && personalCar.m_Keeper != InfoList.Item.kNullEntity)
					{
						var stateKey = CitizenUIUtils.GetStateKey(m_EntityManager, personalCar.m_Keeper);
						StringTooltip stateTooltip = new()
						{
							icon = "Media/Game/Icons/AdvisorInfoView.svg",
							value = $"{m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_STATE[{stateKey}]", "Unknown")}",
							color = TooltipColor.Info,
						};
						tooltipGroup.children.Add(stateTooltip);
					}
				}
				else
				{
					VehicleStateLocaleKey vehicleStateLocaleKey;
					vehicleStateLocaleKey = VehicleUIUtils.GetStateKey(selectedEntity, m_EntityManager);
					var finalStateValueString = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.VEHICLE_STATES[{vehicleStateLocaleKey}]", "Unknown");
					StringTooltip vehicleStateTooltip = new()
					{
						icon = "Media/Game/Icons/AdvisorInfoView.svg",
						value = finalStateValueString,
					};
					tooltipGroup.children.Add(vehicleStateTooltip);
				}
			}

			// GARBAGE TRUCKS
			if (settings.ShowVehicleGarbageTruck && isGarbageTruck && !isParked)
			{
				var garbageTruck = m_EntityManager.GetComponentData<Game.Vehicles.GarbageTruck>(selectedEntity);
				var garbageTruckData = m_EntityManager.GetComponentData<GarbageTruckData>(prefab);

				var collectedGarbage = garbageTruck.m_Garbage <= 0 ? 0 : garbageTruck.m_Garbage / 1000f;
				var garbageCapacity = garbageTruckData.m_GarbageCapacity / 1000f;
				var finalResourceLabelString = m_CustomTranslationSystem.GetLocalGameTranslation("Resources.TITLE[Garbage]", "Garbage");

				StringTooltip garbageTruckTooltip = new()
				{
					icon = "Media/Game/Icons/DeliveryVan.svg",
					value = $"{finalResourceLabelString}: {collectedGarbage.ToString("F", CultureInfo.InvariantCulture)} / {garbageCapacity.ToString("F", CultureInfo.InvariantCulture)} t",
				};
				tooltipGroup.children.Add(garbageTruckTooltip);
			}

			// POST VANS
			if (settings.ShowVehiclePostvan && isPostVan && !isParked)
			{
				var postVan = m_EntityManager.GetComponentData<Game.Vehicles.PostVan>(selectedEntity);
				var postVanData = m_EntityManager.GetComponentData<PostVanData>(prefab);

				var toDeliverValue = postVan.m_DeliveringMail <= 0 ? 0 : postVan.m_DeliveringMail / 1000f;
				var toDeliverMailString = toDeliverValue.ToString("F", CultureInfo.InvariantCulture);
				var collectedValue = postVan.m_CollectedMail <= 0 ? 0 : postVan.m_CollectedMail / 1000f;
				var collectedMailString = collectedValue.ToString("F", CultureInfo.InvariantCulture);
				var combinedMailString = (toDeliverValue + collectedValue).ToString("F", CultureInfo.InvariantCulture);
				var capacityString = (postVanData.m_MailCapacity / 1000f).ToString("F", CultureInfo.InvariantCulture);

				var finalDeliveredLabelString = m_CustomTranslationSystem.GetTranslation("mail.to_deliver", "To deliver");
				StringTooltip toDeliverTooltip = new()
				{
					icon = "Media/Game/Icons/DeliveryVan.svg",
					value = $"{finalDeliveredLabelString}: {toDeliverMailString} t",
				};
				tooltipGroup.children.Add(toDeliverTooltip);

				var finalCollectedLabelString = m_CustomTranslationSystem.GetTranslation("mail.collected", "Collected");
				StringTooltip collectedTooltip = new()
				{
					icon = "Media/Game/Icons/DeliveryVan.svg",
					value = $"{finalCollectedLabelString}: {collectedMailString} t",
				};
				tooltipGroup.children.Add(collectedTooltip);

				var finalCapacityLabelString = m_CustomTranslationSystem.GetTranslation("mail.capacity", "Capacity");
				StringTooltip postCapacityTooltip = new()
				{
					icon = "Media/Game/Icons/DeliveryVan.svg",
					value = $"{finalCapacityLabelString}: {combinedMailString} / {capacityString} t",
				};
				tooltipGroup.children.Add(postCapacityTooltip);
			}

			//  PASSENGERS INFO OF POLICE CARS, PERSONAL CARS, PUBLIC TRANSPORT AND TAXIS
			if (settings.ShowVehiclePassengerDetails && !isParked && (isPoliceCar || isPersonalCar || isPublicTransport || isTaxi))
			{
				// if (settings.ShowVehicleDriver
				//     && m_EntityManager.TryGetComponent(selectedEntity, out Game.Vehicles.PersonalCar personalCar)
				//     && !m_EntityManager.HasComponent<ParkedCar>(selectedEntity)
				//     && personalCar.m_Keeper != InfoList.Item.kNullEntity)
				// {
				// 	NameTooltip vehicleDriverTooltip = new()
				// 	{
				// 		nameBinder = m_NameSystem,
				// 		entity = personalCar.m_Keeper,
				// 		icon = "Media/Game/Icons/Citizen.svg",
				// 	};
				// 	tooltipGroup.children.Add(vehicleDriverTooltip);
				// }

				var passengers = 0;
				var maxPassengers = 0;
				var vehiclePassengerLocaleKey = VehiclePassengerLocaleKey.Passenger;

				// Vehicle compositions (e.g. vehicles with trailers)
				if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<LayoutElement> dynamicBuffer))
				{
					// For each part of the vehicle composition: Normal vehicles: 1, vehicles with trailers: 2
					for (var i = 0; i < dynamicBuffer.Length; i++)
					{
						var vehicle = dynamicBuffer[i].m_Vehicle;
						if (m_EntityManager.TryGetBuffer(vehicle, true, out DynamicBuffer<Passenger> dynamicBuffer2))
						{
							for (var j = 0; j < dynamicBuffer2.Length; j++)
							{
								if (m_EntityManager.HasComponent<Human>(dynamicBuffer2[j].m_Passenger))
								{
									var num = passengers;
									passengers = num + 1;
								}
							}
						}

						if (m_EntityManager.TryGetComponent(vehicle, out PrefabRef vehiclePrefabRef))
						{
							var vehiclePrefab = vehiclePrefabRef.m_Prefab;
							AddPassengerCapacity(vehiclePrefab, ref maxPassengers, ref passengers, ref vehiclePassengerLocaleKey);
						}
					}
				}
				else
				{
					// Single vehicles
					if (m_EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<Passenger> dynamicBuffer3))
					{
						for (var k = 0; k < dynamicBuffer3.Length; k++)
						{
							if (m_EntityManager.HasComponent<Human>(dynamicBuffer3[k].m_Passenger))
							{
								var num = passengers;
								passengers = num + 1;
							}
						}
					}

					AddPassengerCapacity(prefab, ref maxPassengers, ref passengers, ref vehiclePassengerLocaleKey);
				}

				// Do not create tooltip with invalid values
				if (maxPassengers == 0 || passengers < 0)
				{
					return;
				}

				var tooltipTitle = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.PASSENGERS_TITLE", "Passengers");
				if (vehiclePassengerLocaleKey == VehiclePassengerLocaleKey.Prisoner)
				{
					tooltipTitle = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.POLICE_PRISONERS", "Prisoners");
				}

				if (isPersonalCar)
				{
					tooltipTitle = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.CAR_OCCUPANTS", "Occupants");
				}

				StringTooltip vehicleCapacityTooltip = new()
				{
					icon = "Media/Game/Icons/Population.svg",
					value = $"{tooltipTitle}: {passengers}/{maxPassengers}",
				};
				tooltipGroup.children.Add(vehicleCapacityTooltip);
			}
		}

		private void AddPassengerCapacity(Entity prefab, ref int maxPassengers, ref int passengers, ref VehiclePassengerLocaleKey vehiclePassengerLocaleKey)
		{
			if (m_EntityManager.TryGetComponent(prefab, out PersonalCarData personalCarData))
			{
				maxPassengers += personalCarData.m_PassengerCapacity;
				return;
			}

			if (m_EntityManager.TryGetComponent(prefab, out TaxiData taxiData))
			{
				maxPassengers += taxiData.m_PassengerCapacity;
				return;
			}

			if (m_EntityManager.TryGetComponent(prefab, out PoliceCarData policeCarData))
			{
				vehiclePassengerLocaleKey = VehiclePassengerLocaleKey.Prisoner;
				maxPassengers += policeCarData.m_CriminalCapacity;
				return;
			}

			if (m_EntityManager.TryGetComponent(prefab, out PublicTransportVehicleData publicTransportVehicleData))
			{
				maxPassengers += publicTransportVehicleData.m_PassengerCapacity;
			}
		}
	}
}
