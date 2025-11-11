using Colossal.Entities;

using ExtendedTooltip.TooltipBuilder;

using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Creatures;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Routes;
using Game.Simulation;
using Game.Tools;
using Game.UI.InGame;
using Game.UI.Tooltip;
using Game.Vehicles;
using Game.Zones;

using System;
using System.Linq;
using System.Reflection;
using Game.Objects;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace ExtendedTooltip.Systems
{
	public partial class ExtendedTooltipSystem : TooltipSystemBase
	{
		public Assembly[] loadedAssemblies;
		private Entity? lastEntity;
		private float timer = 0f;
		private float2 tooltipPointerDistance;

		private ToolSystem m_ToolSystem;
		private DefaultToolSystem m_DefaultTool;
		private ToolRaycastSystem m_ToolRaycastSystem;
		private PrefabSystem m_PrefabSystem;
		private CustomTranslationSystem m_CustomTranslationSystem;

		private EntityQuery m_CitizenHappinessParameterDataQuery;

		private TooltipGroup m_PrimaryETGroup;
		private TooltipGroup m_SecondaryETGroup;

		private CitizenTooltipBuilder m_CitizenTooltipBuilder;
		private VehicleTooltipBuilder m_VehicleTooltipBuilder;
		private SpawnablesTooltipBuilder m_SpawnablesTooltipBuilder;
		private LotSizeTooltipBuilder m_LotSizeTooltipBuilder;
		private SpeedTooltipBuilder m_SpeedTooltipBuilder;
		private RoadTooltipBuilder m_RoadTooltipBuilder;
		private EfficiencyTooltipBuilder m_EfficiencyTooltipBuilder;
		private ParkTooltipBuilder m_ParkTooltipBuilder;
		private ParkingFacilityTooltipBuilder m_ParkingFacilityTooltipBuilder;
		private PublicTransportationTooltipBuilder m_PublicTransportationTooltipBuilder;
		private EmployeesTooltipBuilder m_EmployeesTooltipBuilder;
		private EducationTooltipBuilder m_EducationTooltipBuilder;
		private CompanyTooltipBuilder m_CompanyTooltipBuilder;
		private TooltipUISystem m_TooltipUISystem;


		public ExtendedTooltipSystem()
		{
		}


		protected override void OnCreate()
		{
			base.OnCreate();

			loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_DefaultTool = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			m_ToolRaycastSystem = World.GetOrCreateSystemManaged<ToolRaycastSystem>();
			m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			m_CustomTranslationSystem = World.GetOrCreateSystemManaged<CustomTranslationSystem>();
			m_TooltipUISystem = World.GetOrCreateSystemManaged<TooltipUISystem>();
			m_CitizenHappinessParameterDataQuery = GetEntityQuery(new ComponentType[] { ComponentType.ReadOnly<CitizenHappinessParameterData>() });

			m_CitizenTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_VehicleTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_RoadTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_SpawnablesTooltipBuilder = new(EntityManager, m_CustomTranslationSystem, m_PrefabSystem);
			m_LotSizeTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_SpeedTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_EfficiencyTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_ParkTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_ParkingFacilityTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_PublicTransportationTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_EmployeesTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_EducationTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);
			m_CompanyTooltipBuilder = new(EntityManager, m_CustomTranslationSystem);

			m_PrimaryETGroup = new TooltipGroup()
			{
				path = "extendedTooltipPrimary",
				position = default,
				horizontalAlignment = TooltipGroup.Alignment.Start,
				verticalAlignment = TooltipGroup.Alignment.Start
			};
			m_SecondaryETGroup = new TooltipGroup()
			{
				path = "extendedTooltipSecondary",
				position = default,
				horizontalAlignment = TooltipGroup.Alignment.End,
				verticalAlignment = TooltipGroup.Alignment.Start,
			};

			tooltipPointerDistance = new(0f, 16f);
			timer = 0;

			Mod.Log.Info("ExtendedTooltipSystem created.");
		}


		protected override void OnUpdate()
		{
			if (IsValidDefaultToolRaycast(out var raycastResult, out var prefabRef))
			{
				var entity = raycastResult.m_Owner;
				var prefab = prefabRef.m_Prefab;
				AdjustTargets(ref entity, ref prefab);

				// Reset timer if entity changed
				if (lastEntity != null && !lastEntity.Equals(entity))
				{
					timer = 0;
					lastEntity = null;
				}

				m_PrimaryETGroup.children.Clear();
				m_SecondaryETGroup.children.Clear();

				if (!IsInfomodeActivated())
				{
					try
					{
						timer += World.Time.DeltaTime;

						if (ShouldDisplayExtendedTooltip(entity))
						{
							CreateExtendedTooltips(entity, prefab);
							var i = 1;
							foreach (var tooltip in m_PrimaryETGroup.children)
							{
								// TODO: Find reason for this error
								/*if (m_TooltipUISystem.mouseGroup.children.Any(t => t.path == tooltip.path))
								{
									// Tooltip already exists in the mouse group
									var message = "Tooltip already exists in the mouse group, this should not happen. Tooltip List: ";
									foreach (var t in m_TooltipUISystem.mouseGroup.children)
									{
										message += t.path + ", ";
									}
									Mod.Log.Debug(message);
									return;
								}*/
								tooltip.path = "etPrimaryTooltip" + i;
								AddMouseTooltip(tooltip);
								i++;
							}

							UpdateSecondaryTooltipGroup();

							lastEntity = entity;
						}
					}
					catch (Exception e)
					{
						Mod.Log.Info("Creating ExtendedTooltips failed at: " + e);
					}
				}
			}
			else
			{
				timer = 0;
			}
		}

		private bool ShouldDisplayExtendedTooltip(Entity entity)
		{
			var shouldDisplayInstantly = Mod.Settings.DisplayMode == DisplayMode.Instant;
			var shouldDisplayWithHotkey = Mod.Settings.DisplayMode == DisplayMode.Hotkey && IsHotkeyPressed();
			var shouldDisplayDelayed = Mod.Settings.DisplayMode == DisplayMode.Delayed && (timer > (float)(Mod.Settings.DisplayModeDelay / 1000f) || (IsMoveable(entity) && !Mod.Settings.DisplayModeDelayOnMoveables));

			return shouldDisplayInstantly || shouldDisplayWithHotkey || shouldDisplayDelayed;
		}

		private bool IsValidDefaultToolRaycast(out RaycastResult raycastResult, out PrefabRef prefabRef)
		{
			if (m_ToolSystem.activeTool == m_DefaultTool && m_ToolRaycastSystem.GetRaycastResult(out raycastResult)
				&& (EntityManager.HasComponent<Building>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Game.Routes.TransportStop>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Game.Objects.OutsideConnection>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Route>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Creature>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Vehicle>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Aggregate>(raycastResult.m_Owner)
				|| EntityManager.HasComponent<Game.Objects.NetObject>(raycastResult.m_Owner))
				&& EntityManager.TryGetComponent(raycastResult.m_Owner, out prefabRef))
			{
				return true;
			}

			raycastResult = default;
			prefabRef = default;

			return false;
		}

		private void CreateExtendedTooltips(Entity selectedEntity, Entity prefab)
		{
			// LOT SIZE TOOLTIP
			if (Mod.Settings.ShowLotSize && EntityManager.HasComponent<PrefabRef>(selectedEntity))
			{
				m_LotSizeTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
			}
			
			// SPEED TOOLTIP
			if (Mod.Settings.ShowSpeed && EntityManager.HasComponent<Moving>(selectedEntity)) // has returns false for cims, even though it has component?!
			{
				m_SpeedTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
			}
			
			// CITIZEN TOOLTIP
			if (Mod.Settings.ShowCitizen && EntityManager.TryGetComponent<Citizen>(selectedEntity, out var citizen))
			{
				var citizenHappinessParameters = m_CitizenHappinessParameterDataQuery.GetSingleton<CitizenHappinessParameterData>();
				m_CitizenTooltipBuilder.Build(selectedEntity, citizen, citizenHappinessParameters, m_PrimaryETGroup, m_SecondaryETGroup);

				return; // don't have any other info. No need to check for other components
			}

			// VEHICLE TOOLTIP
			if (Mod.Settings.ShowVehicle && EntityManager.HasComponent<Vehicle>(selectedEntity))
			{
				m_VehicleTooltipBuilder.Build(selectedEntity, prefab, m_PrimaryETGroup);
				return; // don't have any other info. No need to check for other components
			}

			// ROAD TOOLTIP
			if (Mod.Settings.ShowRoad && EntityManager.HasComponent<AggregateElement>(selectedEntity))
			{
				m_RoadTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
				return; // don't have any other info. No need to check for other components
			}

			// SPAWNABLES TOOLTIP
			var IsMixed = IsMixedBuilding(prefab);
			if (Mod.Settings.ShowGrowables && HasSpawnableBuildingData(selectedEntity, prefab, out var buildingLevel, out var currentCondition, out var levelingCost, out var spawnableData))
			{
				var citizenHappinessParameters = m_CitizenHappinessParameterDataQuery.GetSingleton<CitizenHappinessParameterData>();
				m_SpawnablesTooltipBuilder.Build(m_DefaultTool, IsMixed, selectedEntity, prefab, buildingLevel, currentCondition, levelingCost, spawnableData, citizenHappinessParameters, m_PrimaryETGroup, m_SecondaryETGroup);
			}

			// EFFICIENCY TOOLTIP
			if (Mod.Settings.ShowEfficiency && HasEfficiency(selectedEntity, prefab) && EntityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<Efficiency> buffer))
			{
				m_EfficiencyTooltipBuilder.Build(buffer, m_PrimaryETGroup);
			}

			// PARK BUILDINGS TOOLTIP
			if (Mod.Settings.ShowPark && EntityManager.HasComponent<Game.Buildings.Park>(selectedEntity))
			{
				m_ParkTooltipBuilder.Build(selectedEntity, prefab, m_PrimaryETGroup);
				return; // don't have any other info. No need to check for other components
			}

			// PARKING FACILITY TOOLTIP
			if (Mod.Settings.ShowParkingFacility)
			{
				if (EntityManager.HasComponent<Game.Buildings.ParkingFacility>(selectedEntity))
				{
					m_ParkingFacilityTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
					return; // don't have any other info. No need to check for other components
				}
				else
				{
					try
					{
						m_ParkingFacilityTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
					}
					catch (Exception x)
					{
						Mod.Log.Error("Failed to build ParkingFacilityTooltip: " + x);
					}
				}
			}

			// PUBLIC TRANSPORTATION TOOLTIP
			if (Mod.Settings.ShowPublicTransport && (EntityManager.HasComponent<WaitingPassengers>(selectedEntity) || EntityManager.HasBuffer<ConnectedRoute>(selectedEntity) || EntityManager.HasComponent<Game.Buildings.PublicTransportStation>(selectedEntity)))
			{
				m_PublicTransportationTooltipBuilder.Build(selectedEntity, m_PrimaryETGroup);
				return; // don't have any other info. No need to check for other components
			}

			// EMPLOYEES TOOLTIP
			if (Mod.Settings.ShowEmployee && HasEmployees(selectedEntity, prefab))
			{
				m_EmployeesTooltipBuilder.Build(selectedEntity, prefab, m_PrimaryETGroup);
			}

			// EDUCATION TOOLTIP
			if (Mod.Settings.ShowEducation && EntityManager.HasComponent<Game.Buildings.School>(selectedEntity))
			{
				m_EducationTooltipBuilder.Build(selectedEntity, prefab, m_PrimaryETGroup);
			}

			// COMPANY (Office, Industrial, Commercial) TOOLTIP
			if (CompanyUIUtils.HasCompany(EntityManager, selectedEntity, prefab, out var company))
			{
				m_CompanyTooltipBuilder.Build(company, m_PrimaryETGroup, m_SecondaryETGroup, IsMixed);
			}
		}

		private bool HasEfficiency(Entity selectedEntity, Entity selectedPrefab)
		{
			return EntityManager.HasComponent<Efficiency>(selectedEntity) &&
				!EntityManager.HasComponent<Abandoned>(selectedEntity) &&
				!EntityManager.HasComponent<Destroyed>(selectedEntity) &&
				(!CompanyUIUtils.HasCompany(EntityManager, selectedEntity, selectedPrefab, out var entity) || entity != Entity.Null);
		}

		private bool IsMixedBuilding(Entity prefab)
		{
			var buildingPropertyData = EntityManager.GetComponentData<BuildingPropertyData>(prefab);
			return buildingPropertyData.CountProperties(AreaType.Residential) > 0 && buildingPropertyData.CountProperties(AreaType.Commercial) > 0;
		}

		private bool HasEmployees(Entity entity, Entity prefab)
		{
			// Building is not yet rented or is a park (parks don't have employees)
			if (!EntityManager.TryGetBuffer(entity, true, out DynamicBuffer<Renter> dynamicBuffer) || EntityManager.HasComponent<Game.Buildings.Park>(entity))
			{
				return EntityManager.HasComponent<Employee>(entity) && EntityManager.HasComponent<WorkProvider>(entity);
			}

			// Is a commercial, industrial building or office
			if (dynamicBuffer.Length == 0 && EntityManager.TryGetComponent(prefab, out SpawnableBuildingData spawnableBuildingData))
			{
				var prefab2 = m_PrefabSystem.GetPrefab<ZonePrefab>(spawnableBuildingData.m_ZonePrefab);
				return prefab2 != null && (prefab2.m_AreaType == AreaType.Commercial || prefab2.m_AreaType == AreaType.Industrial || prefab2.m_Office);
			}

			for (var i = 0; i < dynamicBuffer.Length; i++)
			{
				var renter = dynamicBuffer[i].m_Renter;
				if (EntityManager.HasComponent<CompanyData>(renter))
				{
					return EntityManager.HasComponent<Employee>(renter) && EntityManager.HasComponent<WorkProvider>(renter);
				}
			}

			return false;
		}

		private bool HasSpawnableBuildingData(Entity entity, Entity prefab, out int buildingLevel, out int currentCondition, out int levelingCost, out SpawnableBuildingData spawnableData)
		{
			buildingLevel = default;
			currentCondition = default;
			levelingCost = default;
			spawnableData = default;

			var citySystem = EntityManager.World.GetOrCreateSystemManaged<CitySystem>();
			var city = citySystem.City;

			if (EntityManager.TryGetComponent(prefab, out BuildingPropertyData buildingPropertyData) &&
				EntityManager.TryGetComponent(prefab, out SpawnableBuildingData spawnableBuildingData) &&
				EntityManager.TryGetComponent(entity, out BuildingCondition buildingCondition) &&
				EntityManager.TryGetBuffer(city, true, out DynamicBuffer<CityModifier> cityEffectsBuffer) &&
				EntityManager.TryGetComponent(spawnableBuildingData.m_ZonePrefab, out ZoneData zoneData)
			)
			{
				buildingLevel = spawnableBuildingData.m_Level;
				currentCondition = buildingCondition.m_Condition;
				spawnableData = spawnableBuildingData;
				levelingCost = spawnableBuildingData.m_Level < 5 ? BuildingUtils.GetLevelingCost(zoneData.m_AreaType, buildingPropertyData, spawnableBuildingData.m_Level, cityEffectsBuffer) : 0;

				return true;
			}

			return false;
		}

		private void AdjustTargets(ref Entity instance, ref Entity prefab)
		{
			if (EntityManager.TryGetComponent<Game.Creatures.Resident>(instance, out var component) && EntityManager.TryGetComponent<PrefabRef>(component.m_Citizen, out var component2))
			{
				instance = component.m_Citizen;
				prefab = component2.m_Prefab;
			}

			if (EntityManager.TryGetComponent<Controller>(instance, out var component3) && EntityManager.TryGetComponent<PrefabRef>(component3.m_Controller, out var component4))
			{
				instance = component3.m_Controller;
				prefab = component4.m_Prefab;
			}

			if (EntityManager.TryGetComponent<Game.Creatures.Pet>(instance, out var component5) && EntityManager.TryGetComponent<PrefabRef>(component5.m_HouseholdPet, out var component6))
			{
				instance = component5.m_HouseholdPet;
				prefab = component6.m_Prefab;
			}
		}

		private bool IsInfomodeActivated()
		{
			if (m_ToolSystem.activeInfoview == null)
			{
				return false;
			}

			return m_ToolSystem.activeInfoview.name.Equals("Electricity".Trim()) || m_ToolSystem.activeInfoview.name.Equals("WaterPipes".Trim());
		}

		private void UpdateSecondaryTooltipGroup()
		{
			if (InputManager.instance.mouseOnScreen && m_SecondaryETGroup.children.Count > 0)
			{
				var mousePosition = InputManager.instance.mousePosition;
				m_SecondaryETGroup.position = math.round(new float2(mousePosition.x - 8.0f, Screen.height - mousePosition.y) + tooltipPointerDistance);
				AddGroup(m_SecondaryETGroup);
			}
		}

		private bool IsMoveable(Entity entity)
		{
			return EntityManager.HasComponent<Vehicle>(entity) || EntityManager.HasComponent<Citizen>(entity);
		}

		private bool IsHotkeyPressed()
		{
			return Mod.Settings.DisplayModeHotkey switch
			{
				1 => Input.GetKey(KeyCode.LeftControl),
				2 => Input.GetKey(KeyCode.LeftShift),
				3 => Input.GetKey(KeyCode.LeftAlt),
				_ => false,
			};
		}
	}
}
