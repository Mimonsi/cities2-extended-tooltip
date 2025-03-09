using System.Collections.Generic;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using UnityEngine.InputSystem;

namespace ExtendedTooltip
{
	[FileLocation($"ModsSettings/{nameof(ExtendedTooltip)}/{nameof(ExtendedTooltip)}")]
	[SettingsUITabOrder(TAB_GENERAL, TAB_TOOLTIPS)]
	[SettingsUIGroupOrder(GRP_GENERAL, GRP_COMPANY, GRP_TOOLS, GRP_CITIZEN, GRP_PARKS, GRP_PARKING, GRP_PUBLICTRANSPORT, GRP_ROADS, GRP_EDUCATION, GRP_GROWABLES, GRP_VEHICLES)]
	[SettingsUIShowGroupName(GRP_GENERAL, GRP_COMPANY, GRP_TOOLS, GRP_CITIZEN, GRP_PARKS, GRP_PARKING, GRP_PUBLICTRANSPORT, GRP_ROADS, GRP_EDUCATION, GRP_GROWABLES, GRP_VEHICLES)]
	public class ModSettings : ModSetting
	{
		public const string TAB_GENERAL = nameof(TAB_GENERAL);
		public const string TAB_TOOLTIPS = nameof(TAB_TOOLTIPS);
		public const string GRP_GENERAL = nameof(GRP_GENERAL);
		public const string GRP_COMPANY = nameof(GRP_COMPANY);
		public const string GRP_TOOLS = nameof(GRP_TOOLS);
		public const string GRP_CITIZEN = nameof(GRP_CITIZEN);
		public const string GRP_PARKS = nameof(GRP_PARKS);
		public const string GRP_PARKING = nameof(GRP_PARKING);
		public const string GRP_PUBLICTRANSPORT = nameof(GRP_PUBLICTRANSPORT);
		public const string GRP_ROADS = nameof(GRP_ROADS);
		public const string GRP_EDUCATION = nameof(GRP_EDUCATION);
		public const string GRP_GROWABLES = nameof(GRP_GROWABLES);
		public const string GRP_VEHICLES = nameof(GRP_VEHICLES);

		public ModSettings(IMod mod) : base(mod)
		{

		}

		[SettingsUISection(TAB_GENERAL, "")]
		public bool UseExtendedLayout { get; set; }
		[SettingsUISection(TAB_GENERAL, "")]
		public DisplayMode DisplayMode { get; set; } = DisplayMode.Instant;

		[SettingsUIDropdown(typeof(ModSettings), nameof(GetModifierDropdownItems))]
		// Hide by condition is not updating properly, disabling that for now
		[SettingsUIDisableByCondition(typeof(ModSettings), nameof(HideDisplayModeHotkey))]
		[SettingsUISection(TAB_GENERAL, "")]
		public int DisplayModeHotkey { get; set; } = 3;
		private bool HideDisplayModeHotkey() => DisplayMode != DisplayMode.Hotkey;

		[SettingsUIDisableByCondition(typeof(ModSettings), nameof(HideDisplayDelay))]
		[SettingsUISection(TAB_GENERAL, "")]
		[SettingsUISlider(min = 50, max = 2000, step = 50, scalarMultiplier = 1, unit = Unit.kInteger)]
		public int DisplayModeDelay { get; set; } = 250;
		[SettingsUIDisableByCondition(typeof(ModSettings), nameof(HideDisplayDelay))]
		[SettingsUISection(TAB_GENERAL, "")]
		public bool DisplayModeDelayOnMoveables { get; set; } = false;
		private bool HideDisplayDelay() => DisplayMode != DisplayMode.Delayed;

		[SettingsUISection(TAB_GENERAL, "")]
		public bool DisableTooltipIcons { get; set; }

		// GENERAL
		[SettingsUISection(TAB_TOOLTIPS, GRP_GENERAL)]
		public bool ShowEfficiency { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GENERAL)]
		public bool ShowLotSize { get; set; } = true;

		// COMPANY
		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		public bool ShowCompanyOutput { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		public bool ShowEmployee { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		public bool ShowCompanyRent { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		public bool ShowCompanyBalance { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		public bool ShowCompanyProfitability { get; set; } = true;

		[SettingsUISection(TAB_TOOLTIPS, GRP_COMPANY)]
		[SettingsUIDisableByCondition(typeof(ModSettings), nameof(DisableShowCompanyProfitabilityAbsolute))]
		public bool ShowCompanyProfitabilityAbsolute { get; set; } = false;

		private bool DisableShowCompanyProfitabilityAbsolute() => !ShowCompanyProfitability;

		// TOOLS
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowNetToolSystem { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowNetToolUnits { get; set; } = false;
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		[SettingsUIKeyboardBinding(BindingKeyboard.U, "ToggleShowNetToolUnits", shift:true)]
		public ProxyBinding ToggleShowNetToolUnits { get; set; }
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowNetToolMode { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowNetToolElevation { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowTerrainToolHeight { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_TOOLS)]
		public bool ShowWaterToolHeight { get; set; } = true;

		// CITIZEN
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizen { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenState { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenShift { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenWealth { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenType { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenHappiness { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_CITIZEN)]
		public bool ShowCitizenEducation { get; set; } = true;

		// PARKS & RECREATION
		[SettingsUISection(TAB_TOOLTIPS, GRP_PARKS)]
		public bool ShowPark { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_PARKS)]
		public bool ShowParkMaintenance { get; set; } = true;

		// PARKING
		[SettingsUISection(TAB_TOOLTIPS, GRP_PARKING)]
		public bool ShowParkingFacility { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_PARKING)]
		public bool ShowParkingFees { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_PARKING)]
		public bool ShowParkingCapacity { get; set; } = true;

		// PUBLIC TRANSPORT
		[SettingsUISection(TAB_TOOLTIPS, GRP_PUBLICTRANSPORT)]
		public bool ShowPublicTransport { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_PUBLICTRANSPORT)]
		public bool ShowPublicTransportWaitingPassengers { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_PUBLICTRANSPORT)]
		public bool ShowPublicTransportWaitingTime { get; set; } = true;

		// ROADS
		[SettingsUISection(TAB_TOOLTIPS, GRP_ROADS)]
		public bool ShowRoad { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_ROADS)]
		public bool ShowRoadLength { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_ROADS)]
		public bool ShowRoadUpkeep { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_ROADS)]
		public bool ShowRoadCondition { get; set; } = true;

		// EDUCATION
		[SettingsUISection(TAB_TOOLTIPS, GRP_EDUCATION)]
		public bool ShowEducation { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_EDUCATION)]
		public bool ShowEducationStudentCapacity { get; set; } = true;

		// GROWABLES
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowables { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowLandValue { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesHousehold { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesHouseholdDetails { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesLevel { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesLevelDetails { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesRent { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesHouseholdWealth { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesBalance { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_GROWABLES)]
		public bool ShowGrowablesZoneInfo { get; set; } = true;

		// VEHICLES
		[SettingsUISection(TAB_TOOLTIPS, GRP_VEHICLES)]
		public bool ShowVehicle { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_VEHICLES)]
		public bool ShowVehicleState { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_VEHICLES)]
		public bool ShowVehiclePostvan { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_VEHICLES)]
		public bool ShowVehicleGarbageTruck { get; set; } = true;
		[SettingsUISection(TAB_TOOLTIPS, GRP_VEHICLES)]
		public bool ShowVehiclePassengerDetails { get; set; } = true;

		public DropdownItem<int>[] GetModifierDropdownItems()
		{
			var items = new List<DropdownItem<int>>
			{
				new()
				{
					value = 1,
					displayName = "CTRL"
				},
				new()
				{
					value = 2,
					displayName = "SHIFT"
				},
				new()
				{
					value = 3,
					displayName = "ALT"
				}
			};
			return items.ToArray();
		}

		public override void SetDefaults()
		{

		}
	}
}
