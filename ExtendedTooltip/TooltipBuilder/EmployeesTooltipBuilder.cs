using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Buildings;
using Game.Companies;
using Game.Prefabs;
using Game.UI.InGame;
using Game.UI.Tooltip;

using Unity.Entities;
using Unity.Mathematics;

namespace ExtendedTooltip.TooltipBuilder
{
	public class EmployeesTooltipBuilder : TooltipBuilderBase
	{
		public EmployeesTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created EmployeesTooltipBuilder.");
		}

		public void Build(Entity selectedEntity, Entity prefab, TooltipGroup tooltipGroup)
		{
			var employeeCount = 0;
			var maxEmployees = 0;
			var renterEntity = GetRenter(selectedEntity);
			var renterPrefab = m_EntityManager.GetComponentData<PrefabRef>(renterEntity).m_Prefab;
			if (m_EntityManager.TryGetBuffer(renterEntity, true, out DynamicBuffer<Employee> dynamicBuffer) && m_EntityManager.TryGetComponent(renterEntity, out WorkProvider workProvider))
			{
				employeeCount += dynamicBuffer.Length;

				var commercialBuildingLevel = 1;
				if (m_EntityManager.TryGetComponent(prefab, out SpawnableBuildingData spawnableBuildingData))
				{
					commercialBuildingLevel = spawnableBuildingData.m_Level;
				}
				else if (m_EntityManager.TryGetComponent(selectedEntity, out PropertyRenter propertyRenter)
					&& m_EntityManager.TryGetComponent(propertyRenter.m_Property, out PrefabRef prefabRef)
					&& m_EntityManager.TryGetComponent(prefabRef.m_Prefab, out SpawnableBuildingData spawnableBuildingData2))
				{
					commercialBuildingLevel = spawnableBuildingData2.m_Level;
				}

				var complexity = m_EntityManager.GetComponentData<WorkplaceData>(renterPrefab).m_Complexity;
				var workplacesData = EmploymentData.GetWorkplacesData(workProvider.m_MaxWorkers, commercialBuildingLevel, complexity);
				maxEmployees += workplacesData.total;
				var employeeCountPercentage = employeeCount == 0 ? 0 : (int)math.round(100 * employeeCount / maxEmployees);

				StringTooltip employeeTooltip = new()
				{
					icon = "Media/Game/Icons/Commuter.svg",
					value = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.EMPLOYEES", "Employees") + ": " + employeeCount + "/" + maxEmployees,
					color = (employeeCountPercentage == 0) ? TooltipColor.Info : (employeeCountPercentage <= 90) ? TooltipColor.Warning : TooltipColor.Success,
				};
				if (Mod.Settings.DisableTooltipIcons)
					employeeTooltip.icon = null;
				tooltipGroup.children.Add(employeeTooltip);
			}
		}

		private Entity GetRenter(Entity entity)
		{
			if (!m_EntityManager.HasComponent<Game.Buildings.Park>(entity) && m_EntityManager.TryGetBuffer(entity, true, out DynamicBuffer<Renter> dynamicBuffer))
			{
				for (var i = 0; i < dynamicBuffer.Length; i++)
				{
					var renter = dynamicBuffer[i].m_Renter;
					if (m_EntityManager.HasComponent<CompanyData>(renter))
					{
						return renter;
					}
				}
			}

			return entity;
		}
	}
}
