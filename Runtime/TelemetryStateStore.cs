using System.Threading;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Thread-safe store for the latest enhanced telemetry snapshot.
    /// Written from the Unity main thread (FixedUpdate) and readable from any thread.
    /// </summary>
    internal class TelemetryStateStore
    {
        // Boxed snapshot stored via Interlocked.Exchange for lock-free reads.
        private volatile NewStarTelemetryData _snapshot;

        /// <summary>Gets the most recently published telemetry snapshot.</summary>
        public NewStarTelemetryData Latest => _snapshot;

        /// <summary>Publishes a new telemetry snapshot. Thread-safe.</summary>
        public void Publish(NewStarTelemetryData data)
        {
            // Assignment of a struct to a volatile field is atomic on 32-bit aligned fields
            // when the struct fits in a single CPU word; for larger structs we rely on the
            // fact that only one thread (FixedUpdate) writes here.
            Thread.MemoryBarrier();
            _snapshot = data;
            Thread.MemoryBarrier();
        }
    }
}
