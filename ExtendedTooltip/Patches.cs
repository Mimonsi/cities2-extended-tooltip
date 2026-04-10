using Game.Rendering;
using Game.UI.Localization;
using Game.UI.Tooltip;
using Game.UI.Widgets;

using HarmonyLib;

using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;


namespace ExtendedTooltip
{
	[HarmonyPatch(typeof(GuideLineTooltipSystem), "OnUpdate")]
	internal class GuideLineTooltipSystem_OnUpdatePatch
	{
		private static bool Prefix(GuideLineTooltipSystem __instance)
		{

			var m_TooltipUISystem = Traverse.Create(__instance).Field("m_TooltipUISystem").GetValue<TooltipUISystem>();
			var m_Groups = Traverse.Create(__instance).Field("m_Groups").GetValue<List<TooltipGroup>>();
			var m_GuideLinesSystem = Traverse.Create(__instance).Field("m_GuideLinesSystem").GetValue<GuideLinesSystem>();
			var useUnits = Mod.Settings.ShowNetToolSystem && Mod.Settings.ShowNetToolUnits;

			var tooltips = m_GuideLinesSystem.GetTooltips(out var dependencies);
			dependencies.Complete();

			for (var index = 0; index < tooltips.Length; ++index)
			{
				var tooltipInfo = tooltips[index];

				if (m_Groups.Count <= index)
				{
					m_Groups.Add(new TooltipGroup
					{
						path = (PathSegment)string.Format("guideLineTooltip{0}", index),
						horizontalAlignment = TooltipGroup.Alignment.Center,
						verticalAlignment = TooltipGroup.Alignment.Center,
						children = { new FloatTooltip() }
					});
				}

				var group = m_Groups[index];

				var tooltipPos = WorldToTooltipPos((Vector3)tooltipInfo.m_Position);

				if (!group.position.Equals(tooltipPos))
				{
					group.position = tooltipPos;
					group.SetChildrenChanged();
				}

				var child = group.children[0] as FloatTooltip;
				var type = tooltipInfo.m_Type;
				switch (type)
				{
					case GuideLinesSystem.TooltipType.Angle:
						child.icon = "Media/Glyphs/Angle.svg";
						child.value = tooltipInfo.m_Value;
						child.unit = "angle";
						break;

					case GuideLinesSystem.TooltipType.Length:
						child.icon = "Media/Glyphs/Length.svg";
						child.value = useUnits ? tooltipInfo.m_Value / 8f : tooltipInfo.m_Value;
						child.unit = useUnits ? "floatTwoFractions" : "length";
						child.label = useUnits ? LocalizedString.Value("U") : default;
						break;
				}

				AddGroup(m_TooltipUISystem, group);
			}

			return false;
		}

		private static void AddGroup(TooltipUISystem tooltipUISystem, TooltipGroup group)
		{
			if (group.path != PathSegment.Empty && tooltipUISystem.groups.Any(g => g.path == group.path))
			{
				Debug.LogError(string.Format("Trying to add tooltip group with duplicate path '{0}'", group.path));
			}
			else
			{
				tooltipUISystem.groups.Add(group);
			}
		}

		private static float2 WorldToTooltipPos(Vector3 worldPos)
		{
			var xy = ((float3)Camera.main.WorldToScreenPoint(worldPos)).xy;
			xy.y = Screen.height - xy.y;
			return xy;
		}
	}

	[HarmonyPatch(typeof(NetCourseTooltipSystem), "OnUpdate")]
	internal class NetCourseTooltipSystem_OnUpdatePatch
	{
		private static void Postfix(NetCourseTooltipSystem __instance)
		{
			if (!Mod.Settings.ShowNetToolSystem)
			{
				return;
			}

			var m_Length = Traverse.Create(__instance).Field("m_Length").GetValue<FloatTooltip>();

			if (m_Length != null)
			{
				if (Mod.Settings.ShowNetToolUnits)
				{
					m_Length.value /= 8f;
					m_Length.unit = "floatTwoFractions";
					m_Length.label = LocalizedString.Value("U");
				}
				else
				{
					m_Length.unit = "length";
					m_Length.label = default;
				}

			}
		}
	}
}
