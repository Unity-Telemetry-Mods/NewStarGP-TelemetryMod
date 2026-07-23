using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Module initializer that loads native DLLs BEFORE any other code in this assembly runs.
    /// This uses a static constructor with no dependencies to ensure earliest possible execution.
    /// </summary>
    internal static class NativeDllLoader
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        private static bool _loaded = false;
        public static bool IsLoaded => _loaded;
        public static string LoadError { get; private set; }

        // Static constructor - runs when this class is FIRST referenced
        // Place a call to EnsureLoaded() at the very top of NewStarTelemetryPlugin.Awake()
        static NativeDllLoader()
        {
            LoadNativeDlls();
        }

        /// <summary>
        /// Explicitly loads the native DLLs. Call this before ANY code that uses mozaAPI.
        /// </summary>
        public static void EnsureLoaded()
        {
            // Static constructor already ran when this method was called
            // This method just provides an explicit entry point
        }

        private static void LoadNativeDlls()
        {
            try
            {
                // Detect whether the game process is 32-bit or 64-bit
                var is64Bit = Environment.Is64BitProcess;
                var archFolder = is64Bit ? "x64" : "x86";
                var archDesc = is64Bit ? "64-bit (x64)" : "32-bit (x86)";

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var pluginDir = Path.GetDirectoryName(assemblyLocation);
                var nativeDir = Path.Combine(pluginDir, archFolder);

                if (!Directory.Exists(nativeDir))
                {
                    LoadError = $"Process is {archDesc} but {archFolder} directory not found at: {nativeDir}";
                    return;
                }

                // Set the DLL search path
                if (!SetDllDirectory(nativeDir))
                {
                    var error = Marshal.GetLastWin32Error();
                    LoadError = $"Process is {archDesc}. SetDllDirectory({archFolder}) failed with error code: {error}";
                    return;
                }

                // Explicitly preload native DLLs in dependency order
                var mozaSdkPath = Path.Combine(nativeDir, "MOZA_SDK.dll");
                if (!File.Exists(mozaSdkPath))
                {
                    LoadError = $"Process is {archDesc}. MOZA_SDK.dll not found at: {mozaSdkPath}";
                    return;
                }

                var handleSdk = LoadLibrary(mozaSdkPath);
                if (handleSdk == IntPtr.Zero)
                {
                    var error = Marshal.GetLastWin32Error();
                    LoadError = $"Process is {archDesc}. LoadLibrary({archFolder}/MOZA_SDK.dll) failed with error code: {error}";
                    return;
                }

                var mozaApiCPath = Path.Combine(nativeDir, "MOZA_API_C.dll");
                if (!File.Exists(mozaApiCPath))
                {
                    LoadError = $"Process is {archDesc}. MOZA_API_C.dll not found at: {mozaApiCPath}";
                    return;
                }

                var handleApiC = LoadLibrary(mozaApiCPath);
                if (handleApiC == IntPtr.Zero)
                {
                    var error = Marshal.GetLastWin32Error();
                    LoadError = $"Process is {archDesc}. LoadLibrary({archFolder}/MOZA_API_C.dll) failed with error code: {error}";
                    return;
                }

                _loaded = true;
                LoadError = $"SUCCESS: Process is {archDesc}. Loaded native DLLs from {archFolder} folder.";
            }
            catch (Exception ex)
            {
                LoadError = $"Exception during native DLL loading: {ex.Message}";
            }
        }
    }
}
