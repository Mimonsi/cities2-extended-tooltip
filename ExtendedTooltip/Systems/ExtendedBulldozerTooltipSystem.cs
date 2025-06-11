using Colossal.Entities;

using ExtendedTooltip;
using ExtendedTooltip.Systems;
using ExtendedTooltip.TooltipBuilder;

using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI.InGame;

using Unity.Entities;

namespace Game.UI.Tooltip
{
	public partial class ExtendedBulldozerTooltipSystem : TooltipSystemBase
	{
		private TooltipGroup m_TooltipGroup;
		private PrefabSystem m_PrefabSystem;
		private SpawnablesTooltipBuilder m_SpawnablesTooltipBuilder;
		private CompanyTooltipBuilder m_CompanyTooltipBuilder;
		private CustomTranslationSystem m_CustomTranslationSystem;
		private EntityQuery m_CitizenHappinessParameterDataQuery;
		public bool m_LocalSettingsLoaded = false;


		protected override void OnCreate()
		{
			base.OnCreate();
			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			m_BulldozeTool = World.GetOrCreateSystemManaged<BulldozeToolSystem>();
			m_CustomTranslationSystem = World.GetOrCreateSystemManaged<CustomTranslationSystem>();
			m_CitizenHappinessParameterDataQuery = GetEntityQuery(new ComponentType[] { ComponentType.ReadOnly<CitizenHappinessParameterData>() });
			m_SpawnablesTooltipBuilder = new(EntityManager, m_CustomTranslationSystem, m_PrefabSystem);
			m_CompanyTooltipBuilder = new CompanyTooltipBuilder(EntityManager, m_CustomTranslationSystem);
			m_TempQuery = GetEntityQuery(new ComponentType[]
			{
					ComponentType.ReadOnly<Temp>(),
					ComponentType.Exclude<Deleted>()
			});

			RequireForUpdate(m_TempQuery);

			m_TooltipGroup = new TooltipGroup()
			{
				path = "extendedBulldozeTooltipPrimary",
				horizontalAlignment = TooltipGroup.Alignment.Start,
				verticalAlignment = TooltipGroup.Alignment.Start
			};
		}


		protected override void OnUpdate()
		{
			if (m_ToolSystem.activeTool == m_BulldozeTool)
			{
				m_TooltipGroup.children.Clear();

				// Custom bulldoze tooltips
				var lastRaycastPoint = (ControlPoint)typeof(BulldozeToolSystem).GetField("m_LastRaycastPoint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(m_BulldozeTool);
				var selectedEntity = lastRaycastPoint.m_OriginalEntity;
				if (IsValidRaycast(selectedEntity, out var prefab))
				{
					if (Mod.Settings.ShowGrowables && HasSpawnableBuildingData(selectedEntity, prefab, out var buildingLevel, out var currentCondition, out var levelingCost, out var spawnableData))
					{
						var citizenHappinessParameters = m_CitizenHappinessParameterDataQuery.GetSingleton<CitizenHappinessParameterData>();
						m_SpawnablesTooltipBuilder.Build(m_BulldozeTool, false, selectedEntity, prefab, buildingLevel, currentCondition, levelingCost, spawnableData, citizenHappinessParameters, m_TooltipGroup, m_TooltipGroup);
					}

					// COMPANY (Office, Industrial, Commercial) TOOLTIP
					if (CompanyUIUtils.HasCompany(EntityManager, selectedEntity, prefab, out var company))
					{
						m_CompanyTooltipBuilder.Build(company, m_TooltipGroup, m_TooltipGroup, false, true);
					}
				}

				var i = 0;
				foreach (var tooltip in m_TooltipGroup.children)
				{
					tooltip.path = "etBulldozerTooltip" + i;
					AddMouseTooltip(tooltip);
					i++;
				}
			}
		}

		private bool IsValidRaycast(Entity selectedEntity, out PrefabRef prefabRef)
		{
			if (EntityManager.TryGetComponent(selectedEntity, out prefabRef) && (EntityManager.HasComponent<Building>(selectedEntity)
				|| EntityManager.HasComponent<Routes.TransportStop>(selectedEntity)
				|| EntityManager.HasComponent<Objects.OutsideConnection>(selectedEntity)
				|| EntityManager.HasComponent<Aggregate>(selectedEntity)
				|| EntityManager.HasComponent<Objects.NetObject>(selectedEntity)))
			{
				return true;
			}

			prefabRef = default;

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


		public ExtendedBulldozerTooltipSystem()
		{
		}

		private ToolSystem m_ToolSystem;

		private BulldozeToolSystem m_BulldozeTool;

		private EntityQuery m_TempQuery;
	}
}
