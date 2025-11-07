using Colossal.UI.Binding;

using Game.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExtendedTooltip.Systems
{
	public partial class ExtendedTooltipUISystem : UISystemBase
	{
		private readonly string kGroup = "extendedtooltip";
		private CustomTranslationSystem m_CustomTranslationSystem;
		public Dictionary<string, string> m_SettingLocalization;

		protected override void OnCreate()
		{
			base.OnCreate();
			m_CustomTranslationSystem = World.GetOrCreateSystemManaged<CustomTranslationSystem>();

			CreateLanguages();
			AddBinding(new TriggerBinding<string>(kGroup, "launchUrl", OpenURL));

			Mod.Log.Info("ExtendedTooltipUISystem created.");
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Mod.Log.Info("ExtendedTooltipUISystem destroyed.");
		}

		/// <summary>
		/// Open a URL in the web browser
		/// </summary>
		/// <param name="url"></param>
		/// Copyright by optimus-code
		private void OpenURL(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				return;
			}

			try
			{
				// Launch the URL in the default browser
				Process.Start(url);
			}
			catch (Exception ex)
			{
				// Handle exceptions, if any
				Console.WriteLine("An error occurred: " + ex.Message);
			}
		}

		private void CreateLanguages()
		{
			m_SettingLocalization = new()
				{
	                // UI
	                { "uiTabTooltips", m_CustomTranslationSystem.GetTranslation("ui.tab.tooltips", "Tooltips")},
					{ "uiTabTooltipsDescription", m_CustomTranslationSystem.GetTranslation("ui.tab.tooltips.description", "Manage and customize the tooltips.")},
					{ "uiOnOff", m_CustomTranslationSystem.GetTranslation("ui.on_off", "On/Off")},
					{ "uiTabSettings", m_CustomTranslationSystem.GetTranslation("ui.tab.settings", "Settings")},
					{ "uiTabAbout", m_CustomTranslationSystem.GetTranslation("ui.tab.about", "About")},

	                // ABOUT TAB
	                { "about", m_CustomTranslationSystem.GetTranslation("about", "About")},
					{ "aboutVersion", m_CustomTranslationSystem.GetTranslation("about.version", "Version")},
					{ "aboutAuthor", m_CustomTranslationSystem.GetTranslation("about.author", "Author")},
					{ "aboutGithub", m_CustomTranslationSystem.GetTranslation("about.github", "Github")},
					{ "aboutGithubDescription", m_CustomTranslationSystem.GetTranslation("about.github.description", "Open the Github page of the mod.")},
					{ "aboutTranslation", m_CustomTranslationSystem.GetTranslation("about.translation", "Translation")},
					{ "aboutTranslationDescription", m_CustomTranslationSystem.GetTranslation("about.translation.description", "Help to translate the mod.")},
					{ "aboutTranslationDescription2", m_CustomTranslationSystem.GetTranslation("about.translation.description2", "The mod is currently available in the following languages:")},

	                // GENERAL
	                { "enableMod", m_CustomTranslationSystem.GetTranslation("setting.enableMod", "Enable Mod") },
					{ "enableModDescription", m_CustomTranslationSystem.GetTranslation("setting.enableMod.description", "Enable/Disable the mod globally.") },
					{ "modDisabledMessage", m_CustomTranslationSystem.GetTranslation("setting.modDisabledMessage", "Mod is globally disabled!")},
					{ "displayMode", m_CustomTranslationSystem.GetTranslation("setting.displayMode", "Display mode") },
					{ "displayModeDescription", m_CustomTranslationSystem.GetTranslation("setting.displayMode.description", "Decide between different display modes for the tooltip.") },
					{ "displayModeInstant", m_CustomTranslationSystem.GetTranslation("setting.displayMode.instant", "Instant (default)") },
					{ "displayModeDelayed", m_CustomTranslationSystem.GetTranslation("setting.displayMode.delayed", "Delayed") },
					{ "displayModeHotkey", m_CustomTranslationSystem.GetTranslation("setting.displayMode.hotkey", "Hotkey") },
					{ "displayModeHotkeyDescription", m_CustomTranslationSystem.GetTranslation("setting.displayMode.hotkey.description", "The hotkey to trigger the tooltip to show.") },
					{ "displayModeDelay", m_CustomTranslationSystem.GetTranslation("setting.displayMode.delay", "Delay") },
					{ "displayModeDelayDescription", m_CustomTranslationSystem.GetTranslation("setting.displayMode.delay.description", "The amount of delay to show up the tooltip.") },
					{ "displayModeDelayOnMoveables", m_CustomTranslationSystem.GetTranslation("setting.displayMode.delay.on_moveables", "On Moveables") },
					{ "displayModeDelayOnMoveablesDescription", m_CustomTranslationSystem.GetTranslation("setting.displayMode.delay.on_moveables.description", "Activates the delay on moveable entities like citizens or vehicles.") },
					{ "extendedLayout", m_CustomTranslationSystem.GetTranslation("setting.extendedLayout", "Extended Layout") },
					{ "extendedLayoutDescription", m_CustomTranslationSystem.GetTranslation("setting.extendedLayout.description", "Use a second layout group to show tooltips.") },

	                // GENERAL TOOLTIPS
	                { "general", m_CustomTranslationSystem.GetTranslation("setting.general", "General") },
					{ "generalDescription", m_CustomTranslationSystem.GetTranslation("setting.general.description", "Manage tooltips which show up on multiple entities of the game.") },
					{ "generalEfficiency", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.EFFICIENCY", "Efficiency")},
					{ "generalEfficiencyDescription", m_CustomTranslationSystem.GetTranslation("setting.general.efficiency.description", "Shows the efficiency of buliding in %.")},
					{ "generalLandValue", m_CustomTranslationSystem.GetLocalGameTranslation("Infoviews.INFOVIEW[LandValue]", "Land Value")},
					{ "generalLandValueDescription", m_CustomTranslationSystem.GetTranslation("setting.general.land_value.description", "Shows the land value of a building.")},

	                // COMPANY
	                { "company", m_CustomTranslationSystem.GetTranslation("setting.company", "Company") },
					{ "companyDescription", m_CustomTranslationSystem.GetTranslation("setting.company.description", "Manage tooltips which show up on multiple entities of the game.") },
					{ "companyEmployees", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.EMPLOYEES", "Employees")},
					{ "companyEmployeesDescription", m_CustomTranslationSystem.GetTranslation("setting.company.employees.description", "Shows the amount of employees of a building.")},
					{ "companyOutput", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.PRODUCTION", "Company Output") },
					{ "companyOutputDescription", m_CustomTranslationSystem.GetTranslation("setting.company.output.description", "Shows information about the production of companies.")},
					{ "companyRent", m_CustomTranslationSystem.GetTranslation("rent", "Rent")},
					{ "companyRentDescription", m_CustomTranslationSystem.GetTranslation("setting.company.rent.description", "Shows the current rent of the selected building.")},
					{ "companyBalance", m_CustomTranslationSystem.GetTranslation("balance", "Balance")},
					{ "companyBalanceDescription", m_CustomTranslationSystem.GetTranslation("setting.company.balance.description", "Shows the current balance of the selected building.")},
					{ "companyProfitability", m_CustomTranslationSystem.GetLocalGameTranslation("Infoviews.INFOMODE[Profitability]", "Profitability")},
					{ "companyProfitabilityDescription", m_CustomTranslationSystem.GetTranslation("setting.company.profitability.description", "Shows the current profitability of the selected building.")},

	                // TOOL SYSTEMS
	                { "toolSystem", m_CustomTranslationSystem.GetTranslation("setting.toolSystem", "Tool Systems") },
					{ "toolSystemDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.description", "Manage tooltips for different tool systems.") },
					{ "toolSystemAnarchyMode", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.anarchyMode", "Anarchy") },
					{ "toolSystemAnarchyModeDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.anarchyMode.description", "Shows anarchy status.") },
					{ "toolSystemNetTool", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool", "Net Tool") },
					{ "toolSystemNetToolDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool.description", "Manage tooltips shown while using the net tool.") },
					{ "toolSystemNetToolMode", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool.mode", "Net Tool Mode") },
					{ "toolSystemNetToolModeDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool.mode.description", "Shows which mode (straight, curve, ect.) is in use.") },
					{ "toolSystemNetToolElevation", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool.elevation", "Elevation") },
					{ "toolSystemNetToolElevationDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.netTool.elevation.description", "Adds elevation to net tool mode tooltip.") },
					{ "toolSystemTerrainToolHeight", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.terrainTool.height", "Terrain Tool Height") },
					{ "toolSystemTerrainToolHeightDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.terrainTool.height.description", "Shows the current height of the terrain tool.") },
					{ "toolSystemWaterToolHeight", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.waterTool.height", "Water Tool Height.") },
					{ "toolSystemWaterToolHeightDescription", m_CustomTranslationSystem.GetTranslation("setting.toolSystem.waterTool.height.description", "Shows the current height of the water tool.") },

	                // CITIZEN
	                { "citizen", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.CITIZEN_TYPE[Citizen]", "Citizens") },
					{ "citizenDescription", m_CustomTranslationSystem.GetTranslation("setting.citizen.description", "Manage tooltips while hover a citizen.")},
					{ "citizenState", m_CustomTranslationSystem.GetTranslation("setting.citizen.state", "Citizen state") },
					{ "citizenStateDescription", m_CustomTranslationSystem.GetTranslation("setting.citizen.state.description", "Shows the current state of the selected citizen.") },
					{ "citizenHappiness", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.CITIZEN_HAPPINESS", "Citizen happiness") },
					{ "citizenHappinessDecription", m_CustomTranslationSystem.GetTranslation("setting.citizen.happiness.description", "Shows how happy the selected citizen currently is.") },
					{ "citizenEducation", m_CustomTranslationSystem.GetLocalGameTranslation("Infoviews.INFOVIEW[Education]", "Educational Facilities") },
					{ "citizenEducationDescription", m_CustomTranslationSystem.GetTranslation("setting.citizen.education.description", "Shows the education level of the selected citizen.") },
					{ "citizenWealth", m_CustomTranslationSystem.GetLocalGameTranslation("StatisticsPanel.STAT_TITLE[Wealth]", "Citizen wealth") },
					{ "citizenWealthDescription", m_CustomTranslationSystem.GetTranslation("setting.citizen.wealth.description", "Shows the citizens current wealth.") },
					{ "citizenType", m_CustomTranslationSystem.GetLocalGameTranslation("Tutorials.TITLE[TaxationTutorialType]", "Citizen type") },
					{ "citizenTypeDescription", m_CustomTranslationSystem.GetTranslation("setting.citizen.type.description", "Shows which type the selected citizen is.") },

	                // PARK
	                { "park", m_CustomTranslationSystem.GetLocalGameTranslation("Services.NAME[Parks & Recreation]", "Parks & Recreation")},
					{ "parkDescription", m_CustomTranslationSystem.GetTranslation("setting.park.description", "Manage tooltips that show information about parks & recreation sites.")},
					{ "parkMaintenance", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.PARK_MAINTENANCE", "Maintenance")},
					{ "parkMaintenanceDescription", m_CustomTranslationSystem.GetTranslation("setting.park.maintenance.description", "Shows the current maintenance level.")},
                
	                // PARKING
	                { "parkingFacility", m_CustomTranslationSystem.GetLocalGameTranslation("SubServices.NAME[RoadsParking]", "Parking Facilities")},
					{ "parkingFacilityDescription", m_CustomTranslationSystem.GetTranslation("setting.parking.description", "Manage tooltips that show information about parking facilities.")},
					{ "parkingFees", m_CustomTranslationSystem.GetTranslation("setting.parking.fees", "Fees")},
					{ "parkingFeesDescription", m_CustomTranslationSystem.GetTranslation("setting.parking.fees.description", "Shows the current fees for parking.")},
					{ "parkingCapacity", m_CustomTranslationSystem.GetTranslation("setting.parking.capacity", "Capacity")},
					{ "parkingCapacityDescription", m_CustomTranslationSystem.GetTranslation("setting.parking.capacity.description", "Shows the occupied and free parking spaces.")},
                
	                // PUBLIC TRANSPORT
	                { "publicTransport", m_CustomTranslationSystem.GetLocalGameTranslation("TransportInfoPanel.PUBLIC_TRANSPORT_TITLE", "Public Transport")},
					{ "publicTransportDescription", m_CustomTranslationSystem.GetTranslation("setting.public_transportation.description", "Manage tooltips that show information about public transportation.") },
					{ "publicTransportWaitingPassengers", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.WAITING_PASSENGERS", "Waiting Passengers")},
					{ "publicTransportWaitingPassengersDescription", m_CustomTranslationSystem.GetTranslation("setting.public_transportation.waiting_passengers.description", "Shows the current waiting passengers.")},
					{ "publicTransportWaitingTime", m_CustomTranslationSystem.GetTranslation("setting.public_transportation.waiting_time", "Waiting Time")},
					{ "publicTransportWaitingTimeDescription", m_CustomTranslationSystem.GetTranslation("setting.public_transportation.waiting_time.description", "Shows the average waiting time for buildings with stations.")},

	                // ROAD
	                { "road", m_CustomTranslationSystem.GetLocalGameTranslation("Services.NAME[Roads]", "Roads")},
					{ "roadDescription", m_CustomTranslationSystem.GetTranslation("setting.road.description", "Manage tooltips while hover over roads.")},
					{ "roadLength", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.ROAD_LENGTH", "Length")},
					{ "roadLengthDescription", m_CustomTranslationSystem.GetTranslation("setting.road.length.description", "Shows the length of the selected road.")},
					{ "roadUpkeep", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.ROAD_UPKEEP", "Upkeep")},
					{ "roadUpkeepDescription", m_CustomTranslationSystem.GetTranslation("setting.road.upkeep.description", "Shows the upkeep costs of the selected road.")},
					{ "roadCondition", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.ROAD_CONDITION", "Condition")},
					{ "roadConditionDescription", m_CustomTranslationSystem.GetTranslation("setting.road.condition.description", "Shows the condition of the selected road.")},

	                // EDUCATION
	                { "education", m_CustomTranslationSystem.GetLocalGameTranslation("SubServices.NAME[Education]", "Educational Facilities")},
					{ "educationDescription", m_CustomTranslationSystem.GetTranslation("setting.education.description", "Manage tooltips related to educational buildings.")},
					{ "educationStudentCapacity", m_CustomTranslationSystem.GetTranslation("setting.education.student_capacity", "Students Capacity")},
					{ "educationStudentCapacityDescription", m_CustomTranslationSystem.GetTranslation("setting.education.student_capacity.description", "Shows the currently occupied and available places at schools and universities.")},

	                // SPAWNABLE
	                { "growables", m_CustomTranslationSystem.GetLocalGameTranslation("Services.NAME[Zones]", "Growables")},
					{ "growablesDescription",m_CustomTranslationSystem.GetTranslation("setting.growables.description", "Manage tooltips for zone buildings like residentials, commercials, offices and insdustrials.")},
					{ "growablesHousehold", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.HOUSEHOLDS", "Households")},
					{ "growablesHouseholdDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.household.description", "Shows different numbers to the households of the selected building.")},
					{ "growablesHouseholdDetails", m_CustomTranslationSystem.GetTranslation("setting.growables.household_details", "Household Details")},
					{ "growablesHouseholdDetailsDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.household_details.description", "Shows deven more detailed informations to households.")},
					{ "growablesHouseholdWealth", m_CustomTranslationSystem.GetLocalGameTranslation("StatisticsPanel.STAT_TITLE[Wealth]", "Household Wealth")},
					{ "growablesHouseholdWealthDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.wealth.description", "Shows the current average wealth of the households of a building.")},
					{ "growablesLevel", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.LEVEL", "Level")},
					{ "growablesLevelDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.level.description", "Level")},
					{ "growablesLevelDetails", m_CustomTranslationSystem.GetTranslation("setting.growables.level_details", "Shows the level of the selected building.")},
					{ "growablesLevelDetailsDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.level_details.description", "Shows more info related to level of a building.")},
					{ "growablesZoneInfo", m_CustomTranslationSystem.GetTranslation("setting.growables.zone_info", "Zone Info")},
					{ "growablesZoneInfoDescription",m_CustomTranslationSystem.GetTranslation("setting.growables.zone_info.description", "Shows detailed zone information like name, type, level and others.")},
					{ "growablesRent", m_CustomTranslationSystem.GetTranslation("rent", "Rent")},
					{ "growablesRentDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.rent.description", "Shows the current rent of the selected building.")},
					{ "growablesBalance", m_CustomTranslationSystem.GetTranslation("balance", "Balance")},
					{ "growablesBalanceDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.balance.description", "Shows the current balance of the selected building.")},
					{ "growablesPloppableRICOInfo", m_CustomTranslationSystem.GetTranslation("setting.growables.ploppable_rico_info", "Ploppable RICO")},
					{ "growablesPloppableRICOInfoDescription", m_CustomTranslationSystem.GetTranslation("setting.growables.ploppable_rico_info.description", "Shows ploppable RICO related info.")},


	                // VEHICLE
	                { "vehicle", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.VEHICLES[HouseholdVehicle]", "Vehicles")},
					{ "vehicleDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.description", "Manage tooltips while hover over vehicles like Cars, Trucks, Taxis and Co.")},
					{ "vehicleState", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.VEHICLE_STATE", "Status")},
					{ "vehicleStateDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.state.description", "Shows the current state of the selected vehicle.")},
					{ "vehicleDriver", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.DRIVER", "Driver")},
					{ "vehicleDriverDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.driver.description", "Shows driver information of the selected vehicle.")},
					{ "vehicleGarbageTruck", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.GARBAGE_VEHICLE_TITLE[GarbageTruck]", "Garbage Truck")},
					{ "vehicleGarbageTruckDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.garbage_truck.description", "Shows garbage capacity of the selected garbage trucks.")},
					{ "vehiclePostvan", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.POST_VEHICLE_TITLE", "Postvan")},
					{ "vehiclePostvanDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.postvan.description", "Shows mail capacity of the selected post vans.")},
					{ "vehiclePassengerDetail", m_CustomTranslationSystem.GetLocalGameTranslation("SelectedInfoPanel.PASSENGERS_TITLE", "Passengers")},
					{ "vehiclePassengerDetailDescription", m_CustomTranslationSystem.GetTranslation("setting.vehicle.passenger_detail.description", "Shows detailed information about the passengers of the selected vehicle.")},
				};
		}
	}
}
