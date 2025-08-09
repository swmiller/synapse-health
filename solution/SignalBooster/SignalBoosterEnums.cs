namespace Synapse.SignalBoosterExample
{
    /// <summary>
    /// Contains enums used throughout the SignalBooster program.
    /// </summary>
    public static class SignalBoosterEnums
    {
        /// <summary>
        /// Supported device types for DME extraction.
        /// </summary>
        public enum DeviceType
        {
            Unknown,
            CPAP,
            OxygenTank,
            Wheelchair
        }

        /// <summary>
        /// Mask types for CPAP devices.
        /// </summary>
        public enum CpapMaskType
        {
            None,
            FullFace,
            Nasal,
            NasalPillow
        }

        /// <summary>
        /// Add-on options for devices.
        /// </summary>
        public enum AddOnOption
        {
            None,
            Humidifier
        }

        /// <summary>
        /// Usage context for oxygen tanks.
        /// </summary>
        public enum OxygenTankUsageContext
        {
            None,
            Sleep,
            Exertion,
            SleepAndExertion
        }
    }
}
