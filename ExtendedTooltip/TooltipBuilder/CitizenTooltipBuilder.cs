using Colossal.Entities;

using ExtendedTooltip.Systems;

using Game.Citizens;
using Game.Prefabs;
using Game.UI.InGame;
using Game.UI.Tooltip;

using System;

using Unity.Entities;

namespace ExtendedTooltip.TooltipBuilder
{
	internal class CitizenTooltipBuilder : TooltipBuilderBase
	{
		public CitizenTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created CitizenTooltipBuilder.");
		}

		public void Build(Entity entity, Citizen citizen, CitizenHappinessParameterData citizenHappinessParameters, TooltipGroup tooltipGroup, TooltipGroup secondaryTooltipGroup)
		{
			var model = Mod.Settings;

			// State
			if (model.ShowCitizenState)
			{
				var stateKey = CitizenUIUtils.GetStateKey(m_EntityManager, entity);
				StringTooltip stateTooltip = new()
				{
					icon = "Media/Game/Icons/AdvisorInfoView.svg",
					value = $"{m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_STATE[{stateKey}]", "Unknown")}",
					color = TooltipColor.Info,
				};
				tooltipGroup.children.Add(stateTooltip);
			}

			// Needs revisting, not working
			if ((model.ShowCitizenWealth || model.ShowCitizenType) & m_EntityManager.TryGetComponent(entity, out HouseholdMember householdMember))
			{
				var household = householdMember.m_Household;

				if (model.ShowCitizenType)
				{
					var citizenKey = CitizenKey.Citizen;
					if (m_EntityManager.HasComponent<CommuterHousehold>(householdMember.m_Household))
					{
						citizenKey = CitizenKey.Commuter;
					}
					else if (m_EntityManager.HasComponent<TouristHousehold>(householdMember.m_Household))
					{
						citizenKey = CitizenKey.Tourist;
					}

					var citizenTypeValue = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_TYPE[{citizenKey}]", "Unknown");
					StringTooltip citizenTypeTooltip = new()
					{
						icon = $"Media/Game/Icons/{citizenKey}.svg",
						value = $"{citizenTypeValue}",
						color = TooltipColor.Info,
					};
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(citizenTypeTooltip);
				}

				if (model.ShowCitizenWealth)
				{
					var wealthKey = CitizenUIUtils.GetHouseholdWealth(m_EntityManager, household, citizenHappinessParameters);
					var tooltipColor = wealthKey switch
					{
						HouseholdWealthKey.Wretched => TooltipColor.Error,
						HouseholdWealthKey.Poor => TooltipColor.Warning,
						HouseholdWealthKey.Modest => TooltipColor.Info,
						_ => TooltipColor.Success,
					};
					var wealthLabel = m_CustomTranslationSystem.GetLocalGameTranslation("StatisticsPanel.STAT_TITLE[Wealth]", "Wealth");
					var wealthValue = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_WEALTH[{wealthKey}]");
					StringTooltip wealthTooltip = new()
					{
						icon = "Media/Game/Icons/CitizenWealth.svg",
						value = $"{wealthLabel}: {wealthValue}",
						color = tooltipColor,
					};
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(wealthTooltip);
				}
			}

			/*if (m_EntityManager.TryGetComponent(entity, out TaxPayer taxPayer))
			{
				int wage = (taxPayer.m_UntaxedIncome - ((int)math.round(0.01f * taxPayer.m_AverageTaxRate * taxPayer.m_UntaxedIncome)));
				wage = wage < 0 ? 0 : wage;
				StringTooltip wageTooltip = new()
				{
					icon = "Media/Game/Icons/Money.svg",
					value = $"Wage: {wage}",
				};
				tooltipGroup.children.Add(wageTooltip);
			}*/

			// Happiness
			if (model.ShowCitizenHappiness)
			{
				var happinessValue = citizen.Happiness;
				var happinessKey = (CitizenHappinessKey)CitizenUtils.GetHappinessKey(happinessValue);
				var happinessLabelString = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.CITIZEN_HAPPINESS", "Happiness");
				var happinessValueString = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_HAPPINESS_TITLE[{Enum.GetName(typeof(CitizenHappinessKey), happinessKey)}]", happinessKey.ToString());
				StringTooltip happinessTooltip = new()
				{
					icon = "Media/Game/Icons/" + happinessKey.ToString() + ".svg",
					value = $"{happinessLabelString}: {happinessValueString}",
					color = happinessValue < 50 ? TooltipColor.Error : happinessValue < 75 ? TooltipColor.Warning : TooltipColor.Success
				};
				tooltipGroup.children.Add(happinessTooltip);
			}

			// Education
			if (model.ShowCitizenEducation)
			{
				var educationKey = CitizenUIUtils.GetEducation(citizen);
				var educationLabelString = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.CITIZEN_EDUCATION_TITLE", "Education");
				var educationValueString = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_EDUCATION[{Enum.GetName(typeof(CitizenEducationKey), educationKey)}]", educationKey.ToString());
				StringTooltip educationTooltip = new()
				{
					icon = "Media/Game/Icons/Education.svg",
					value = $"{educationLabelString}: {educationValueString}",
					color = TooltipColor.Info,
				};
				tooltipGroup.children.Add(educationTooltip);
			}

			// Shift
			if (model.ShowCitizenShift)
			{
				if (m_EntityManager.TryGetComponent(entity, out Worker worker))
				{
					string shiftLocalization = m_CustomTranslationSystem.GetLocalGameTranslation($"extendedtooltip.workshift[{worker.m_Shift}]", "Unknown");
					StringTooltip shiftTooltip = new()
					{
						icon = "Media/Game/Icons/Workers.svg",
						value = $"{m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.CITIZEN_SHIFT", "Shift")}: {shiftLocalization}",
						color = TooltipColor.Info,
					};
					(model.UseExtendedLayout ? secondaryTooltipGroup : tooltipGroup).children.Add(shiftTooltip);
				}
			}
		}
	}
}
