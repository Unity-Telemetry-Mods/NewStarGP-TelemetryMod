namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Provides physical wheel input state each frame.
    /// </summary>
    internal interface IWheelInputProvider
    {
        /// <summary>Initialise the input provider. Returns true on success.</summary>
        bool Initialize();

        /// <summary>Poll and return the latest wheel input state.</summary>
        WheelInputState Poll();

        /// <summary>Release all resources held by this provider.</summary>
        void Dispose();
    }
}
