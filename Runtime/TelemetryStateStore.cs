using System.Threading;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Thread-safe store for the latest enhanced telemetry snapshot.
    /// Written from the Unity main thread (FixedUpdate) and readable from any thread.
    /// </summary>
    internal class TelemetryStateStore
    {
        private readonly object _lock = new object();
        private NewStarTelemetryData _snapshot;

        /// <summary>Gets the most recently published telemetry snapshot. Thread-safe.</summary>
        public NewStarTelemetryData Latest
        {
            get { lock (_lock) { return _snapshot; } }
        }

        /// <summary>Publishes a new telemetry snapshot. Thread-safe.</summary>
        public void Publish(NewStarTelemetryData data)
        {
            lock (_lock) { _snapshot = data; }
        }
    }
}
