namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Writes virtual gamepad state to the OS (e.g. via ViGEm).
    /// </summary>
    internal interface IVirtualGamepadOutput
    {
        /// <summary>Initialise and connect the virtual gamepad. Returns true on success.</summary>
        bool Initialize();

        /// <summary>Submit a new gamepad state to the virtual device.</summary>
        void Submit(VirtualPadState state);

        /// <summary>Disconnect and release the virtual gamepad.</summary>
        void Dispose();
    }
}
