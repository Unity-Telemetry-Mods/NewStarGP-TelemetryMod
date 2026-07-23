using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using com.drowhunter.TelemetryLib;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

using TelemetryLib;

using UnityEngine;

namespace com.drowhunter.NewStarGPTelemetryMod
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class NewStarTelemetryPlugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        TelemetryExtractor telemetryExtractor;

        //GameManager gameManager;


        internal NewStarTelemetryData data;

        UdpTelemetry<NewStarTelemetryData> _dataOut;

        RacingContextManager _racingContextManager;

        private ConfigEntry<int> Port;

        WheelConfig _wheelConfig;
        WheelIntegrationRuntime _wheelRuntime;

        CarControl _carControl
        {
            get
            {
                if (!_racingContextManager)
                {
                    _racingContextManager = RacingContextManager.inst;
                }

                return _racingContextManager?.SinglePlayer?.Control;
            }
        }

        static bool paused = false;


        //[HarmonyPatch(typeof(PauseMenu), nameof(PauseMenu.Update))]
        //class Patch
        //{
        //    static void Postfix(PauseMenu __instance)
        //    {
        //        paused = __instance.paused;



        //    }
        //}


        private void Awake()
        {
            // CRITICAL: Load native DLLs BEFORE any other code runs
            // This must be the FIRST line to ensure DLLs are loaded before any reference to mozaAPI
            NativeDllLoader.EnsureLoaded();

            //Harmony.CreateAndPatchAll(typeof(NewStarTelemetryPlugin));
            //var harmony = new Harmony("com.drowhunter.NewStarTelemetryPlugin");
            //harmony.PatchAll();


            // Plugin startup logic
            Logger = base.Logger;

            // Report process architecture and native DLL loading status
            var processArch = Environment.Is64BitProcess ? "64-bit (x64)" : "32-bit (x86)";
            Logger.LogInfo($"[MozaNative] *** PROCESS ARCHITECTURE: {processArch} ***");

            // NativeDllLoader.LoadError contains the detailed status message
            if (NativeDllLoader.IsLoaded)
            {
                Logger.LogInfo($"[MozaNative] {NativeDllLoader.LoadError}");
            }
            else
            {
                Logger.LogError($"[MozaNative] FAILED: {NativeDllLoader.LoadError}");
            }

            // Verify both architecture folders exist (for debugging)
            var pluginDir = Path.GetDirectoryName(Info.Location);
            var x86Dir = Path.Combine(pluginDir, "x86");
            var x64Dir = Path.Combine(pluginDir, "x64");
            Logger.LogInfo($"[MozaNative] x86 folder exists: {Directory.Exists(x86Dir)}");
            Logger.LogInfo($"[MozaNative] x64 folder exists: {Directory.Exists(x64Dir)}");


            Port = Config.Bind("Telemetry", "UDP Port", 12345, "Port to send telemetry data on.");

            _dataOut = new UdpTelemetry<NewStarTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, Port.Value)
            }, new MarshalByteConverter<NewStarTelemetryData>());



            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            _wheelConfig = new WheelConfig(Config);
            Logger.LogInfo($"[Startup] WheelConfig created. Enabled={_wheelConfig.Enabled.Value}");

            if (_wheelConfig.Enabled.Value)
            {
                try
                {
                    Logger.LogInfo("[Startup] Creating WheelIntegrationRuntime...");
                    _wheelRuntime = new WheelIntegrationRuntime(Logger);

                    Logger.LogInfo("[Startup] Calling Initialize...");
                    _wheelRuntime.Initialize(_wheelConfig);

                    Logger.LogInfo("[Startup] Initialize completed successfully.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Startup] FATAL: Wheel integration crashed during init: {ex}");
                    _wheelRuntime = null;
                }
            }

            Logger.LogInfo("[Startup] Awake() completed.");
        }

        

        private void Start()
        {
            telemetryExtractor = new TelemetryExtractor();
        }

        

        
        private Delta[] deltas = Enumerable.Range(0, 4).Select(_ => new Delta()).ToArray();

        enum wheel { rr, fr, rl, fl };
        void GetWheels(Vehicle vehicle)
        {
            var localwheels = vehicle.wheels.Select(w => w.transform.position.y).ToArray();

            var avg = localwheels.Average();
            for (var i = 0; i < localwheels.Length; i++)
            {
                deltas[i].Update(localwheels[i] - avg);                
            }
        }

        
        
        

        private void FixedUpdate()
        {
            bool doTelemetry = true;

            if (!_carControl)
            {
                return;
            }
            var isRacing = TWK.RaceState == "Racing";
            var isRaceOver = TWK.ChallengeEventOver == 1;

            var vehicle = _carControl.vehicle;
            var allowDriving = vehicle.allowDriving;
            

            doTelemetry = (allowDriving && !isRaceOver);

            if(!doTelemetry)
            {
                return;
            }

            var data = new NewStarTelemetryData();


            var cRigidbody = vehicle.rigid;
            telemetryExtractor.Update(cRigidbody);

            if (!cRigidbody)
            {
                Logger.LogInfo("Rigidbody is null");
                return;
            }

            var basic = telemetryExtractor.ExtractTelemetry();

            GetWheels(vehicle);
            
            data = new NewStarTelemetryData
            {
                Pitch = basic.EulerAngles.x,
                Yaw = basic.EulerAngles.y,
                Roll = basic.EulerAngles.z,
                AngularVelocityX = basic.LocalAngularVelocity.x,
                AngularVelocityY = basic.LocalAngularVelocity.y,
                AngularVelocityZ = basic.LocalAngularVelocity.z,
                cForce = basic.CentripetalForce,
                VelocityX = basic.LocalVelocity.x,
                VelocityY = basic.LocalVelocity.y,
                VelocityZ = basic.LocalVelocity.z,
                AccelX = basic.Accel.x,
                AccelY = basic.Accel.y,
                AccelZ = basic.Accel.z,
                
                Speed = vehicle.speed,
                RPM = vehicle.engine.maxRPM != 0 ? vehicle.engine.RPM / vehicle.engine.maxRPM : 0,
                CurrentGear = _carControl.vehicle.gearbox.targetGear,

                TireFL = deltas[(int)wheel.fl] / Time.fixedDeltaTime,
                TireFR = deltas[(int)wheel.fr] / Time.fixedDeltaTime,
                TireBL = deltas[(int)wheel.rl] / Time.fixedDeltaTime,
                TireBR = deltas[(int)wheel.rr] / Time.fixedDeltaTime,
               

                WheelsOnTrack = vehicle.wheelsOnTrack,
                IsBoosting = vehicle.slipstream.boosting,
                AllowDriving = allowDriving,
                IsRacing = isRacing,
                IsEventOver = isRaceOver,

            };
            
            _dataOut.Send(data);

            _wheelRuntime?.Update(in data);

        }

        private void OnDestroy()
        {
            _wheelRuntime?.Dispose();
            _wheelRuntime = null;
        }
    }

    public class Delta : MonoBehaviour
    {
        private float _lastValue;

        public float Value { get; private set; }

        public float Update(float currentValue)
        {
            Value = currentValue - _lastValue;
            _lastValue = currentValue;
            return Value;
        }

        public static implicit operator float(Delta d) => d.Value;
    }
}