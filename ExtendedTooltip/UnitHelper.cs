using Game.SceneFlow;
using Game.Settings;

namespace ExtendedTooltip
{
    public static class UnitHelper
    {

        public static string FormatSpeedLimit(float value)
        {
            if (Mod.Settings.SpeedUnit == SpeedUnitSetting.Kph || (Mod.Settings.SpeedUnit  == SpeedUnitSetting.Default && IsMetric()))
                return $"{MsToKph(value)} km/h";
            return $"{MsToMph(value)} mph";
        }
        
        public static int MsToKph(float value)
        {
            return (int)(value * 3.6);
        }
        
        public static int MsToMph(float value)
        {
            return (int)(value * 2.23694);
        }
    
        public static double FeetToMeters(double value)
        {
            return value * 0.3048;
        }

        public static double MetersToFeet(double value)
        {
            return value / 0.3048;
        }
    
        private static bool IsMetric()
        {
            return GameManager.instance?.settings?.userInterface?.unitSystem is null or InterfaceSettings.UnitSystem.Metric;
        }
    }
}