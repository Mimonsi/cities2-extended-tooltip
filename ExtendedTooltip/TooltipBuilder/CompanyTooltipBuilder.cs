using Colossal.Entities;
using Colossal.Mathematics;

using ExtendedTooltip.Systems;

using Game.Buildings;
using Game.Common;
using Game.Companies;
using Game.Economy;
using Game.Prefabs;
using Game.UI.InGame;
using Game.UI.Tooltip;

using System;

using Unity.Entities;

namespace ExtendedTooltip.TooltipBuilder
{
	public class CompanyTooltipBuilder : TooltipBuilderBase
	{

		public CompanyTooltipBuilder(EntityManager entityManager, CustomTranslationSystem customTranslationSystem)
		: base(entityManager, customTranslationSystem)
		{
			Mod.Log.Info($"Created CompanyTooltipBuilder.");
		}

		public void Build(Entity companyEntity, TooltipGroup tooltipGroup, TooltipGroup secondaryTooltipGroup, bool IsMixed = false, bool IsBulldozing = false)
		{
			var model = Mod.Settings;

			// Company output tooltip
			var companyEntityPrefab = m_EntityManager.GetComponentData<PrefabRef>(companyEntity).m_Prefab;

			// Profitability
			if (model.ShowCompanyProfitability && !IsBulldozing && m_EntityManager.TryGetComponent(companyEntity, out Profitability companyProfitability))
			{
				var companyProfitabilityKey = CompanyUIUtils.GetProfitabilityKey(companyProfitability.m_Profitability);
				var profitabilityLabel = m_CustomTranslationSystem.GetLocalGameTranslation("Infoviews.INFOMODE[Profitability]", "Profitability");
				var profitabilityValue = m_CustomTranslationSystem.GetLocalGameTranslation($"SelectedInfoPanel.COMPANY_PROFITABILITY_TITLE[{companyProfitabilityKey}]", companyProfitabilityKey.ToString());
				var tooltipColor = companyProfitabilityKey switch
				{
					CompanyProfitabilityKey.Bankrupt => TooltipColor.Error,
					CompanyProfitabilityKey.LosingMoney => TooltipColor.Warning,
					CompanyProfitabilityKey.Profitable => TooltipColor.Success,
					_ => TooltipColor.Info,
				};
				StringTooltip profitabilityTooltip = new()
				{
					icon = "Media/Game/Icons/CompanyProfit.svg",
					value = $"{profitabilityLabel}: {profitabilityValue} ({(int)companyProfitability.m_Profitability})",
					color = tooltipColor
				};
				if (model.DisableTooltipIcons)
					profitabilityTooltip.icon = null;
				if (!model.ShowCompanyProfitabilityAbsolute)
				{
					// Use percentage value
					var percentage = ((double)companyProfitability.m_Profitability / 255.0) * 100;
					profitabilityTooltip.value = $"{profitabilityLabel}: {profitabilityValue} ({(int)percentage}%)";
				}
				(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(profitabilityTooltip);
			}

			// Company resource section
			if (m_EntityManager.TryGetBuffer(companyEntity, true, out DynamicBuffer<Resources> resources))
			{

				if (model.ShowCompanyOutput && m_EntityManager.TryGetComponent(companyEntityPrefab, out IndustrialProcessData industrialProcessData))
				{
					var input1 = industrialProcessData.m_Input1.m_Resource;
					var input2 = industrialProcessData.m_Input2.m_Resource;
					var output = industrialProcessData.m_Output.m_Resource;

					if (input1 > 0 && input1 != Resource.NoResource && input1 != output)
					{
						(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(CreateResourceTooltip(companyEntity, companyEntityPrefab, resources, input1));
					}

					if (input2 > 0 && input2 != Resource.NoResource && input2 != output)
					{
						(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(CreateResourceTooltip(companyEntity, companyEntityPrefab, resources, input2));
					}

						(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(CreateResourceTooltip(companyEntity, companyEntityPrefab, resources, output, true));
				}

				// Company money balance
				if (model.ShowCompanyBalance && !IsBulldozing)
				{
					var companyBalance = 0;
					for (var i = 0; i < resources.Length; i++)
					{
						if (resources[i].m_Resource is Resource.Money)
						{
							companyBalance = resources[i].m_Amount;
							break;
						}
					}
					var companyBalanceLabel = m_CustomTranslationSystem.GetTranslation("balance", "Balance");
					var companyBalanceValueString = "\u00a2" + companyBalance.ToString("N0");
					var finalCompanyBalanceString = $"{companyBalanceLabel}: {companyBalanceValueString}";

					StringTooltip companyBalanceTooltip = new()
					{
						value = finalCompanyBalanceString,
						icon = "Media/Game/Icons/Money.svg",
						color = companyBalance < 0 ? TooltipColor.Error : TooltipColor.Info,
					};
					if (model.DisableTooltipIcons)
						companyBalanceTooltip.icon = null;
					(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(companyBalanceTooltip);
				}
			}

			// Company Rent
			if (model.ShowCompanyRent && !IsBulldozing && m_EntityManager.TryGetComponent(companyEntity, out PropertyRenter propertyRenter))
			{
				var rentLabel = m_CustomTranslationSystem.GetTranslation("rent", "Rent");
				string rentValue;

				var randomSeed = RandomSeed.Next();
				var random = randomSeed.GetRandom(1);

				var rent = MathUtils.RoundToIntRandom(ref random, propertyRenter.m_Rent * 1f);
				var finalRentValue = rent < 0 ? Math.Abs(rent) : rent;
				rentValue = m_CustomTranslationSystem.GetLocalGameTranslation("Common.VALUE_MONEY_PER_MONTH", "-", "SIGN", rent < 0 ? "-" : "", "VALUE", finalRentValue.ToString("N0"));

				StringTooltip rentTooltip = new()
				{
					icon = "Media/Game/Icons/Money.svg",
					value = $"{rentLabel}: {rentValue}",
					color = TooltipColor.Info,
				};
				if (model.DisableTooltipIcons)
					rentTooltip.icon = null;
				(model.UseExtendedLayout && !IsMixed ? secondaryTooltipGroup : tooltipGroup).children.Add(rentTooltip);
			}
		}

		private StringTooltip CreateResourceTooltip(Entity companyEntity, Entity companyEntityPrefab, DynamicBuffer<Resources> companyResources, Resource resource, bool isOutput = false, bool isBulldozing = false)
		{
			// OUTPUT Storage
			StringTooltip companyResourceTooltip = new();
			string resourceLabel;
			var isStorage = false;

			var showResourceUnit = resource switch
			{
				Resource.Media or Resource.Entertainment or Resource.Financial or Resource.Software or Resource.Telecom or Resource.Recreation => false,
				_ => true,
			};

			if (isOutput)
			{
				if (m_EntityManager.HasComponent<ServiceAvailable>(companyEntity))
				{
					resourceLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.COMPANY_SELLS", "Sells");
				}
				else if (m_EntityManager.HasComponent<Game.Companies.ExtractorCompany>(companyEntity) || m_EntityManager.HasComponent<Game.Companies.ProcessingCompany>(companyEntity))
				{
					resourceLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.COMPANY_PRODUCES", "Produces");
				}
				else
				{
					var componentData = m_EntityManager.GetComponentData<StorageCompanyData>(companyEntityPrefab);
					isStorage = true;
					resource = componentData.m_StoredResources;
					resourceLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.COMPANY_STORES", "Stores");
				}
			}
			else
			{
				resourceLabel = m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.COMPANY_REQUIRES", "Needs");
			}

			var resourceValue = m_CustomTranslationSystem.GetLocalGameTranslation($"Resources.TITLE[{resource}]", resource.ToString());
			GetCompanyOutputResource(companyResources, resource, out var resourceAmount);
			if (!Mod.Settings.DisableTooltipIcons)
				companyResourceTooltip.icon = "Media/Game/Resources/" + resource.ToString() + ".svg";
			if (resourceAmount > 0 && !isBulldozing)
			{
				// TODO: Fix actual translations issue #1
				if (showResourceUnit)
				{
					var unit = "t";
					if (resource == Resource.Oil)
					{
						unit = "bbl.";
						var densityCrudeOil = 0.8;
						var litersPerBarrel = 158.9873;
						var litersCrudeOil = resourceAmount * densityCrudeOil;
						var barrelsCrudeOil = litersCrudeOil / litersPerBarrel;

						resourceValue = m_CustomTranslationSystem.GetTranslation(!isStorage ? "setting.company.production_rate_prefixed" : "setting.company.production_rate", "Unknown", "RESOURCE", resourceValue, "AMOUNT", barrelsCrudeOil.ToString("F2"), "UNIT", unit);
					}
					else if (resource == Resource.Petrochemicals)
					{
						var densityGasoline = 0.75;
						var gasolineValue = resourceAmount > 1000 ? resourceAmount * densityGasoline / 1000 : resourceAmount * densityGasoline;
						unit = resourceAmount > 1000 ? "kl" : "l";
						resourceValue = m_CustomTranslationSystem.GetTranslation(!isStorage ? "setting.company.production_rate_prefixed" : "setting.company.production_rate", "Unknown", "RESOURCE", resourceValue, "AMOUNT", gasolineValue.ToString("F2"), "UNIT", unit);
					}
					else
					{
						var resourceValueInDouble = (double)resourceAmount / 1000;
						resourceValue = m_CustomTranslationSystem.GetTranslation(!isStorage ? "setting.company.production_rate_prefixed" : "setting.company.production_rate", "Unknown", "RESOURCE", resourceValue, "AMOUNT", resourceValueInDouble.ToString("F2"), "UNIT", unit);
					}
				}
				else
				{
					resourceValue = m_CustomTranslationSystem.GetTranslation(!isStorage ? "setting.company.production_rate_prefixed" : "setting.company.production_rate", "Unknown", "RESOURCE", resourceValue, "AMOUNT", resourceAmount.ToString(), "UNIT", "");
				}
			}

			companyResourceTooltip.value = $"{resourceLabel}: {resourceValue}";

			return companyResourceTooltip;
		}

		/// <summary>
		/// Get the amount of resource the company is producing (output)
		/// </summary>
		/// <param name="companyEntity"></param>
		/// <param name="outputResource"></param>
		/// <param name="companyResourceAmount"></param>
		private void GetCompanyOutputResource(DynamicBuffer<Resources> resources, Resource outputResource, out int companyResourceAmount)
		{
			companyResourceAmount = 0;
			foreach (var resource in resources)
			{
				if (resource.m_Resource == outputResource)
				{
					companyResourceAmount = resource.m_Amount;
					break;
				}
			}
		}
	}
}
