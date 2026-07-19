namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Sends force-feedback commands to the physical wheel.
    /// </summary>
    internal interface IWheelFfbProvider
    {
        /// <summary>Initialise the FFB provider. Returns true on success.</summary>
        bool Initialize();

        /// <summary>Apply the given FFB command to the wheel hardware.</summary>
        void SendFfb(FfbCommand command);

        /// <summary>Stop all active FFB effects and release resources.</summary>
        void Dispose();
    }
}
