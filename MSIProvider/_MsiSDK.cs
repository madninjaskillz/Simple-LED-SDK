// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MSIProvider
{
    // ReSharper disable once InconsistentNaming
    internal static class _MsiSDK
    {
        public static List<string> PossibleX86NativePaths { get; } = new List<string> { "x86/MysticLight_SDK.dll" };
        public static List<string> PossibleX64NativePaths { get; } = new List<string> { "x64/MysticLight_SDK.dll" };
        #region Libary Management

        private static IntPtr _dllHandle = IntPtr.Zero;

        /// <summary>
        /// Gets the loaded architecture (x64/x86).
        /// </summary>
        internal static string LoadedArchitecture { get; private set; }

        /// <summary>
        /// Reloads the SDK.
        /// </summary>
        internal static void Reload()
        {
            UnloadMsiSDK();
            LoadMsiSDK();
        }

        private static void LoadMsiSDK()
        {
            if (_dllHandle != IntPtr.Zero) return;

            // HACK: Load library at runtime to support both, x86 and x64 with one managed dll
            List<string> possiblePathList = Environment.Is64BitProcess ? PossibleX64NativePaths : PossibleX86NativePaths;
            string dllPath = possiblePathList.FirstOrDefault(File.Exists);
            if (dllPath == null) throw new Exception($"Can't find the Msi-SDK at one of the expected locations:\r\n '{string.Join("\r\n", possiblePathList.Select(Path.GetFullPath))}'");

            SetDllDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath)));

            _dllHandle = LoadLibrary(dllPath);

            var p1 = GetProcAddress(_dllHandle, "MLAPI_Initialize");
            var p2 = Marshal.GetDelegateForFunctionPointer(p1, typeof(InitializePointer));

            _initializePointer = (InitializePointer)Marshal.GetDelegateForFunctionPointer< InitializePointer>(GetProcAddress(_dllHandle, "MLAPI_Initialize"));
            _getDeviceInfoPointer = (GetDeviceInfoPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetDeviceInfo"), typeof(GetDeviceInfoPointer));
            _getDeviceInfoPointerInt = (GetDeviceInfoPointerInt)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetDeviceInfo"), typeof(GetDeviceInfoPointerInt));
            _getLedInfoPointer = (GetLedInfoPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedInfo"), typeof(GetLedInfoPointer));
            _getLedColorPointer = (GetLedColorPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedColor"), typeof(GetLedColorPointer));
            _getLedStylePointer = (GetLedStylePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedStyle"), typeof(GetLedStylePointer));
            _getLedMaxBrightPointer = (GetLedMaxBrightPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedMaxBright"), typeof(GetLedMaxBrightPointer));
            _getLedBrightPointer = (GetLedBrightPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedBright"), typeof(GetLedBrightPointer));
            _getLedMaxSpeedPointer = (GetLedMaxSpeedPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedMaxSpeed"), typeof(GetLedMaxSpeedPointer));
            _getLedSpeedPointer = (GetLedSpeedPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetLedSpeed"), typeof(GetLedSpeedPointer));
            _setLedColorPointer = (SetLedColorPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_SetLedColor"), typeof(SetLedColorPointer));
            _setLedStylePointer = (SetLedStylePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_SetLedStyle"), typeof(SetLedStylePointer));
            _setLedBrightPointer = (SetLedBrightPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_SetLedBright"), typeof(SetLedBrightPointer));
            _setLedSpeedPointer = (SetLedSpeedPointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_SetLedSpeed"), typeof(SetLedSpeedPointer));
            _getErrorMessagePointer = (GetErrorMessagePointer)Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllHandle, "MLAPI_GetErrorMessage"), typeof(GetErrorMessagePointer));
        }

        public static void UnloadMsiSDK()
        {
            if (_dllHandle == IntPtr.Zero) return;

            // ReSharper disable once EmptyEmbeddedStatement - DarthAffe 07.10.2017: We might need to reduce the internal reference counter more than once to set the library free
            while (FreeLibrary(_dllHandle)) ;
            _dllHandle = IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr dllHandle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr dllHandle, string name);

        #endregion

        #region SDK-METHODS

        #region Pointers

        private static InitializePointer _initializePointer;
        private static GetDeviceInfoPointer _getDeviceInfoPointer;
        private static GetDeviceInfoPointerInt _getDeviceInfoPointerInt;
        private static GetLedInfoPointer _getLedInfoPointer;
        private static GetLedColorPointer _getLedColorPointer;
        private static GetLedStylePointer _getLedStylePointer;
        private static GetLedMaxBrightPointer _getLedMaxBrightPointer;
        private static GetLedBrightPointer _getLedBrightPointer;
        private static GetLedMaxSpeedPointer _getLedMaxSpeedPointer;
        private static GetLedSpeedPointer _getLedSpeedPointer;
        private static SetLedColorPointer _setLedColorPointer;
        private static SetLedStylePointer _setLedStylePointer;
        private static SetLedBrightPointer _setLedBrightPointer;
        private static SetLedSpeedPointer _setLedSpeedPointer;
        private static GetErrorMessagePointer _getErrorMessagePointer;

        #endregion

        #region Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitializePointer();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetDeviceInfoPointer(
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pDevType,
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetDeviceInfoPointerInt(
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pDevType,
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out int[] pLedCount);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedInfoPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.BStr)] out string pName,
            [Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] pLedStyles);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedColorPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.I4)] out int r,
            [Out, MarshalAs(UnmanagedType.I4)] out int g,
            [Out, MarshalAs(UnmanagedType.I4)] out int b);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedStylePointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.BStr)] out string style);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedMaxBrightPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.I4)] out int maxLevel);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedBrightPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.I4)] out int currentLevel);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedMaxSpeedPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.I4)] out int maxSpeed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetLedSpeedPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [Out, MarshalAs(UnmanagedType.I4)] out int currentSpeed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetLedColorPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [In, MarshalAs(UnmanagedType.I4)] int r,
            [In, MarshalAs(UnmanagedType.I4)] int g,
            [In, MarshalAs(UnmanagedType.I4)] int b);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetLedStylePointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [In, MarshalAs(UnmanagedType.BStr)] string style);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetLedBrightPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [In, MarshalAs(UnmanagedType.I4)] int level);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetLedSpeedPointer(
            [In, MarshalAs(UnmanagedType.BStr)] string type,
            [In, MarshalAs(UnmanagedType.I4)] int index,
            [In, MarshalAs(UnmanagedType.I4)] int speed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetErrorMessagePointer(
            [In, MarshalAs(UnmanagedType.I4)] int errorCode,
            [Out, MarshalAs(UnmanagedType.BStr)] out string pDesc);

        #endregion

        internal static int Initialize() => _initializePointer();
        internal static int GetDeviceInfo(out string[] pDevType, out int[] pLedCount)
        {
            // HACK - SDK GetDeviceInfo returns a string[] for ledCount, so we'll parse that to int.
            int result = _getDeviceInfoPointer(out pDevType, out string[] ledCount);
            if (pDevType != null)
            {
                pLedCount = new int[ledCount.Length];

                for (int i = 0; i < ledCount.Length; i++)
                    pLedCount[i] = int.Parse(ledCount[i]);



                return result;
            }
            else
            {
                result = _getDeviceInfoPointerInt(out pDevType, out int[] ledCountInt);

                if (ledCount != null)
                {
                    pLedCount = new int[ledCount.Length];

                    for (int i = 0; i < ledCount.Length; i++)
                        pLedCount[i] = int.Parse(ledCount[i]);
                }
                else
                {
                    pLedCount= new int[0];
                }


                return result;
            }

            pDevType = new string[0];
            pLedCount = new int[0];
            return 0;
        }

        internal static int GetLedInfo(string type, int index, out string pName, out string[] pLedStyles) => _getLedInfoPointer(type, index, out pName, out pLedStyles);
        internal static int GetLedColor(string type, int index, out int r, out int g, out int b) => _getLedColorPointer(type, index, out r, out g, out b);
        internal static int GetLedStyle(string type, int index, out string style) => _getLedStylePointer(type, index, out style);
        internal static int GetLedMaxBright(string type, int index, out int maxLevel) => _getLedMaxBrightPointer(type, index, out maxLevel);
        internal static int GetLedBright(string type, int index, out int currentLevel) => _getLedBrightPointer(type, index, out currentLevel);
        internal static int GetLedMaxSpeed(string type, int index, out int maxSpeed) => _getLedMaxSpeedPointer(type, index, out maxSpeed);
        internal static int GetLedSpeed(string type, int index, out int currentSpeed) => _getLedSpeedPointer(type, index, out currentSpeed);
        internal static int SetLedColor(string type, int index, int r, int g, int b) => _setLedColorPointer(type, index, r, g, b);
        internal static int SetLedStyle(string type, int index, string style) => _setLedStylePointer(type, index, style);
        internal static int SetLedBright(string type, int index, int level) => _setLedBrightPointer(type, index, level);
        internal static int SetLedSpeed(string type, int index, int speed) => _setLedSpeedPointer(type, index, speed);

        internal static string GetErrorMessage(int errorCode)
        {
            _getErrorMessagePointer(errorCode, out string description);
            return description;
        }

        #endregion
    }
}
