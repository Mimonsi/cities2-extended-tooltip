using Colossal.Entities;
using Colossal.Mathematics;

using ExtendedTooltip.Systems;

using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Economy;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.UI.InGame;
using Game.UI.Tooltip;
using Game.Zones;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class SpawnablesTooltipBuilder : TooltipBuilderBase
	{
		private readonly PrefabSystem m_PrefabSystem;
		private readonly Type m_PloppableBuildingDataType;
		private readonly Type m_HistoricalType;

		public SpawnablesTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem, PrefabSystem prefabSystem)
		: base(entityManager, customTranslationSystem)
		{
			m_PrefabSystem = prefabSystem;
			var findStuffAssembly = m_ExtendedTooltipSystem.loadedAssemblies.FirstOrDefault(a => a.GetName().Name == "FindStuff");
			m_PloppableBuildingDataType = findStuffAssembly?.GetTypes().FirstOrDefault(a => a.Name == "PloppableBuildingData");
			m_HistoricalType = findStuffAssembly?.GetTypes().FirstOrDefault(a => a.Name == "Historical");
			Mod.Log.Info($"Created SpawnablesTooltipBuilder.");
		}

		public void Build(ToolBaseSystem activeTool, bool isMixed, Entity entity, Entity prefab, int buildingLevel, int currentCondition, int levelingCost, SpawnableBuildingData spawnableBuildingData, CitizenHappinessParameterData citizenHappinessParameters, TooltipGroup tooltipGroup, TooltipGroup secondaryTooltipGroup)
		{
			var model = Mod.Settings;

			if (model.ShowGrowablesHousehold == false && model.ShowGrowablesHouseholdDetails == false && model.ShowGrowablesLevel == false && model.ShowGrowablesLevelDetails == false && model.ShowGrowablesRent == false)
			{
				return;
			}

			var isBulldozing = activeTool is BulldozeToolSystem;

			if (model.ShowGrowablesZoneInfo)
			{
				//var zoneData = m_EntityManager.GetComponentData<ZoneData>(spawnableBuildingData.m_ZonePrefab);
				PrefabBase rawZone;
				try
				{
					rawZone = m_PrefabSystem.GetPrefab<PrefabBase>(spawnableBuildingData.m_ZonePrefab);
					if (rawZone == null)
						return;
				}
				catch (Exception e)
				{
					return;
				}
				var rawZoneName = rawZone.name;
				string finalZoneName;

				// Offices and industrial zones have a different naming convention
				/*if (zoneData.m_AreaType == AreaType.Industrial) // Offices are technically industrial zones
				{
					finalZoneName = m_CustomTranslationSystem.GetLocalGameTranslation($"Assets.NAME[{rawZoneName}]", rawZoneName);
				}
				else
				{
					// Split the string into individual values
					var zoneInfos = rawZoneName.Split(' ').ToList();
					for (var i = 0; i < zoneInfos.Count; i++)
					{
						zoneInfos[i] = m_CustomTranslationSystem.GetTranslation($"zone_info[{zoneInfos[i]}]", zoneInfos[i]);
					}

					finalZoneName = string.Join(", ", zoneInfos);
				}*/
				finalZoneName = m_CustomTranslationSystem.GetLocalGameTranslation($"Assets.NAME[{rawZoneName}]", rawZoneName);
				if (m_EntityManager.HasComponent<Signature>(entity))
				{
					finalZoneName += " - Signature";
				}

				StringTooltip zoneTooltip = new()
				{
					icon = "Media/Game/Icons/Zones.svg",
					value = finalZoneName,
				};
				if (Mod.Settings.DisableTooltipIcons)
					zoneTooltip.icon = null;
				(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(zoneTooltip);
			}

			if (model.ShowGrowablesLevel)
			{
				var isHistorical = m_PloppableBuildingDataType != null && IsHistorical(entity);
				var buildingLevelColor = TooltipColor.Info;
				var buildingLevelLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.LEVEL", "Level");
				var buildingLevelValue = !isHistorical ? $"{buildingLevel}/5" : $"{buildingLevel}";

				if (buildingLevel == 5)
				{
					buildingLevelColor = TooltipColor.Success;
				}

				if (currentCondition < 0 && !isHistorical)
				{
					buildingLevelColor = TooltipColor.Error;
				}

				if (model.ShowGrowablesLevelDetails == true && !isHistorical && levelingCost > 0)
				{
					buildingLevelValue += $" [¢{currentCondition} / ¢{levelingCost}]";
				}

				StringTooltip levelTooltip = new()
				{
					icon = "Media/Game/Icons/BuildingLevel.svg",
					value = $"{buildingLevelLabel}: {buildingLevelValue}",
					color = buildingLevelColor
				};
				if (Mod.Settings.DisableTooltipIcons)
					levelTooltip.icon = null;
				(model.UseExtendedLayout && isMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(levelTooltip);
			}

			// LAND VALUE TOOLTIP
			if (model.ShowLandValue && !isBulldozing && m_EntityManager.TryGetComponent(entity, out Building building) && m_EntityManager.HasComponent<BuildingCondition>(entity))
			{
				if (building.m_RoadEdge != Entity.Null)
				{
					var landValue = m_EntityManager.GetComponentData<LandValue>(building.m_RoadEdge);
					double landValueAmount = landValue.m_LandValue;
					var landValueTooltipColor = landValueAmount <= 150 ? TooltipColor.Success : landValueAmount <= 300 ? TooltipColor.Info : landValueAmount <= 450 ? TooltipColor.Warning : TooltipColor.Error;

					var landValueLabel = m_CustomTranslationSystem.GetLocalGameTranslation("Infoviews.INFOVIEW[LandValue]", "Land Value");
					var landValueString = "\u00a2" + landValueAmount.ToString("N0");
					var finalLandValueString = $"{landValueLabel}: {landValueString}";

					StringTooltip landValueTooltip = new()
					{
						value = finalLandValueString,
						icon = "Media/Game/Icons/LandValue.svg",
						color = landValueTooltipColor
					};
					if (Mod.Settings.DisableTooltipIcons)
						landValueTooltip.icon = null;
					(model.UseExtendedLayout && isMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(landValueTooltip);
				}
			}

			// Add residential info if available
			var residentCount = 0;
			var householdCount = 0;
			var maxHouseholds = 0;
			var petsCount = 0;
			List<int> householdRents = new();
			List<int> householdBalances = new();
			NativeList<Entity> householdsResult = new(Allocator.Temp);
			if ((model.ShowGrowablesHousehold == true || model.ShowGrowablesRent == true) && CreateTooltipsForResidentialProperties(ref residentCount, ref householdCount, ref maxHouseholds, ref petsCount, ref householdRents, ref householdBalances, ref householdsResult, entity, prefab))
			{
				BuildHouseholdCitizenInfo(activeTool, householdCount, maxHouseholds, residentCount, petsCount, out var finalInfoString);
				if (model.ShowGrowablesHousehold)
				{
					var householdCapacityPercentage = householdCount == 0 ? 0 : (int)math.round(100 * householdCount / maxHouseholds);
					var householdTooltipColor = (householdCount == 0 || householdCapacityPercentage < 50) ? TooltipColor.Error : householdCapacityPercentage < 80 ? TooltipColor.Warning : TooltipColor.Success;
					StringTooltip householdTooltip = new()
					{
						icon = "Media/Game/Icons/Household.svg",
						value = finalInfoString,
						color = householdTooltipColor
					};
					if (Mod.Settings.DisableTooltipIcons)
						householdTooltip.icon = null;
					(model.UseExtendedLayout && isMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(householdTooltip);
				}

				if (model.ShowGrowablesHouseholdWealth && activeTool is DefaultToolSystem && householdsResult.Length > 0)
				{
					var wealthKey = householdsResult.Length == 1
						? CitizenUIUtils.GetHouseholdWealth(m_EntityManager, householdsResult[0], citizenHappinessParameters)
						: CitizenUIUtils.GetAverageHouseholdWealth(m_EntityManager, householdsResult, citizenHappinessParameters);

					var tooltipColor = wealthKey switch
					{
						HouseholdWealthKey.Wretched => TooltipColor.Error,
						HouseholdWealthKey.Poor => TooltipColor.Warning,
						HouseholdWealthKey.Modest => TooltipColor.Info,
						_ => TooltipColor.Success,
					};

					var wealthLabel = m_CustomTranslationSystem.GetLocalGameTranslation(householdsResult.Length > 1 ? "SelectedInfoPanel.AVERAGE_HOUSEHOLD_WEALTH" : "StatisticsPanel.STAT_TITLE[Wealth]", "Household wealth");
					var wealthValue = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_WEALTH[{wealthKey}]");
					StringTooltip wealthTooltip = new()
					{
						icon = "Media/Game/Icons/CitizenWealth.svg",
						value = $"{wealthLabel}: {wealthValue}",
						color = tooltipColor,
					};
					if (Mod.Settings.DisableTooltipIcons)
						wealthTooltip.icon = null;
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(wealthTooltip);
				}

				if (model.ShowGrowablesBalance && !isBulldozing && householdBalances.Count > 0)
				{
					var balanceLabel = m_CustomTranslationSystem.GetTranslation("balance", "Balance");
					string balanceValue;
					int finalBalance = default;
					int minBalance = default;
					int maxBalance = default;

					if (householdCount > 1)
					{
						minBalance = householdBalances.Min();
						var minBalanceValue = minBalance < 0 ? Math.Abs(minBalance) : minBalance;
						maxBalance = householdBalances.Max();
						var maxBalanceValue = maxBalance < 0 ? Math.Abs(maxBalance) : maxBalance;
						balanceValue = m_CustomTranslationSystem.GetTranslation("common.range_minmax_value_money", "", "SIGN0", minBalance < 0 ? "-" : "", "VALUE0", minBalanceValue.ToString(), "SIGN1", maxBalance < 0 ? "-" : "", "VALUE1", maxBalanceValue.ToString("N0"));
					}
					else
					{
						finalBalance = householdBalances.First();
						var finalRentValue = finalBalance < 0 ? Math.Abs(finalBalance) : finalBalance;
						balanceValue = m_CustomTranslationSystem.GetLocalGameTranslation("Common.VALUE_MONEY", "", "SIGN", finalBalance < 0 ? "-" : "", "VALUE", finalRentValue.ToString("N0"));
					}

					StringTooltip balanceTooltip = new()
					{
						icon = "Media/Game/Icons/Money.svg",
						value = $"{balanceLabel}: {balanceValue}",
						color = (householdCount > 1 && minBalance < 0 && maxBalance < 0) || (householdCount == 1 && finalBalance < 0) ? TooltipColor.Error : TooltipColor.Info,
					};
					if (Mod.Settings.DisableTooltipIcons)
						balanceTooltip.icon = null;
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(balanceTooltip);
				}

				if (model.ShowGrowablesRent == true && !isBulldozing && householdRents.Count > 0)
				{
					var rentLabel = m_CustomTranslationSystem.GetTranslation("rent", "Rent");
					string rentValue;

					// Only show range if there are more than 1 household and the min and max rents differ
					if (householdCount > 1 && householdRents.Min() != householdRents.Max())
					{
						var minRent = householdRents.Min();
						var minRentValue = minRent < 0 ? Math.Abs(minRent) : minRent;
						var maxRent = householdRents.Max();
						var maxRentValue = maxRent < 0 ? Math.Abs(maxRent) : maxRent;
						rentValue = m_CustomTranslationSystem.GetTranslation("common.range_value_money", "", "SIGN0", minRent < 0 ? "-" : "", "VALUE0", minRentValue.ToString("N0"), "SIGN1", maxRent < 0 ? "-" : "", "VALUE1", maxRentValue.ToString("N0"));
					}
					else
					{
						var finalRent = householdRents.First();
						var finalRentValue = finalRent < 0 ? Math.Abs(finalRent) : finalRent;
						rentValue = m_CustomTranslationSystem.GetLocalGameTranslation("Common.VALUE_MONEY_PER_MONTH", "-", "SIGN", finalRent < 0 ? "-" : "", "VALUE", finalRentValue.ToString("N0"));
					}

					StringTooltip rentTooltip = new()
					{
						icon = "Media/Game/Icons/Money.svg",
						value = $"{rentLabel}: {rentValue}",
						color = TooltipColor.Info,
					};
					if (Mod.Settings.DisableTooltipIcons)
						rentTooltip.icon = null;
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(rentTooltip);
				}
			}
		}

		private bool CreateTooltipsForResidentialProperties(
				ref int residentCount,
				ref int householdCount,
				ref int maxHouseholds,
				ref int petsCount,
				ref List<int> householdRents,
				ref List<int> householdBalances,
				ref NativeList<Entity> householdsResult,
				Entity entity,
				Entity prefab
			)
		{
			var flag = false;
			var isAbondened = m_EntityManager.HasComponent<Abandoned>(entity);
			var hasBuildingPropertyData = m_EntityManager.TryGetComponent(prefab, out BuildingPropertyData buildingPropertyData);
			var hasResidentialProperties = buildingPropertyData.m_ResidentialProperties > 0;
			var randomSeed = RandomSeed.Next();

			// Only check for residents if the building is not abandoned and has residential properties
			if (!isAbondened && hasBuildingPropertyData && hasResidentialProperties)
			{
				flag = true;
				maxHouseholds += buildingPropertyData.m_ResidentialProperties;

				if (m_EntityManager.TryGetBuffer(entity, true, out DynamicBuffer<Renter> renterBuffer))
				{
					for (var i = 0; i < renterBuffer.Length; i++)
					{
						var renter = renterBuffer[i].m_Renter;

						// Does only count residential properties rent
						if (!m_EntityManager.HasComponent<Household>(renter))
						{
							continue;
						}

						// Get rent
						int rent;
						if (m_EntityManager.TryGetComponent(renter, out PropertyRenter propertyRenter))
						{
							var random = randomSeed.GetRandom(1 + i);
							rent = MathUtils.RoundToIntRandom(ref random, propertyRenter.m_Rent * 1f);
							householdRents.Add(rent);
						}

						// Balances (Savings)
						if (m_EntityManager.TryGetBuffer(renter, true, out DynamicBuffer<Resources> resources))
						{
							var balance = EconomyUtils.GetResources(Resource.Money, resources);
							householdBalances.Add(balance);
						}

						// Household info
						if (m_EntityManager.TryGetBuffer(renter, true, out DynamicBuffer<HouseholdCitizen> householdCitizenBuffer))
						{
							householdsResult.Add(renter);
							householdCount++;
							for (var j = 0; j < householdCitizenBuffer.Length; j++)
							{
								// Is not in death routine
								if (!CitizenUtils.IsCorpsePickedByHearse(m_EntityManager, householdCitizenBuffer[j].m_Citizen))
								{
									residentCount++;
								}
							}

							// Woof
							if (m_EntityManager.TryGetBuffer(renter, true, out DynamicBuffer<HouseholdAnimal> animalBuffer))
							{
								petsCount += animalBuffer.Length;
							}
						}
					}
				}
			}

			return flag;
		}

		public void BuildHouseholdCitizenInfo(ToolBaseSystem activeTool, int households, int maxHouseholds, int residents, int pets, out string finalInfoString)
		{
			// Households
			var householdsLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.HOUSEHOLDS", "Households");
			var householdsValue = $"{households}/{maxHouseholds}";

			// Residents
			var residentsLabel = m_CustomTranslationSystem.GetLocalGameTranslation("Properties.ADJUST_HAPPINESS_MODIFIER_TARGET[Residents]", "Residents");
			var residentsValue = residents.ToString();

			// Pets
			var petsValue = pets.ToString();
			var petsLabel = pets > 1 ? m_CustomTranslationSystem.GetTranslation("pets", "Pets") : m_CustomTranslationSystem.GetTranslation("pet");

			// If residential building is > low density (only 1 household) show the household label only
			if (maxHouseholds > 1)
			{
				if (Mod.Settings.ShowGrowablesHouseholdDetails && activeTool is DefaultToolSystem)
				{
					if (pets > 0)
					{
						finalInfoString = $"{householdsValue} {householdsLabel} [{residentsValue} {residentsLabel}, {petsValue} {petsLabel}]";
						return;
					}
					else
					{
						finalInfoString = $"{householdsValue} {householdsLabel} [{residentsValue} {residentsLabel}]";
					}
				}
				else
				{
					finalInfoString = $"{householdsValue} {householdsLabel}";
				}
			}
			else // low densitiy housing only has 1 household
			{
				if (Mod.Settings.ShowGrowablesHouseholdDetails && activeTool is DefaultToolSystem && pets > 0)
				{
					finalInfoString = $"{residentsValue} {residentsLabel} [{petsValue} {petsLabel}]";
				}
				else
				{
					finalInfoString = $"{residentsValue} {residentsLabel}";
				}
			}
		}

		private bool IsHistorical(Entity entity)
		{
			if (m_PloppableBuildingDataType == null)
			{
				return false;
			}

			return m_EntityManager.HasComponent(entity, m_HistoricalType);
		}
	}
}
