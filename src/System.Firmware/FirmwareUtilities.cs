// System.Firmware
// Copyright (C) 2024 Rikitav
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System.Firmware.Win32Native;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Firmware;

/// <summary>
/// Features for interacting with UEFI
/// </summary>
public static class FirmwareUtilities
{
    /// <summary>
    /// Determines whether firmware environment variable functionality is available on the current system.
    /// </summary>
    /// <remarks>
    /// Firmware environment variables are typically used for advanced system configuration and are
    /// only supported on certain platforms, such as Windows running on UEFI firmware. This method can be used to check
    /// for support before attempting operations that require firmware environment variables.
    /// </remarks>
    /// <returns>true if firmware environment variable support is available; otherwise, false.</returns>
    public static bool CheckFirmwareAvailablity()
    {
        _ = NativeMethods.GetFirmwareEnvironmentVariableExW("", "{00000000-0000-0000-0000-000000000000}", IntPtr.Zero, 0, out _);
        return Marshal.GetLastWin32Error() != NativeMethods.ERROR_INVALID_FUNCTION;
    }

    /// <summary>
    /// Retrieves the value of a global environment variable as a string.
    /// </summary>
    /// <remarks>
    /// The returned value is read from the global environment. If the specified variable is not found, the method returns null.
    /// This method allocates and frees unmanaged memory internally, callers do not need to manage memory for the returned string.
    /// </remarks>
    /// <param name="VarName">The name of the environment variable to retrieve. Cannot be null or empty.</param>
    /// <returns>A string containing the value of the specified environment variable, or null if the variable does not exist.</returns>
    public static string ReadStringVariable(string VarName)
    {
        IntPtr pointer = GetGlobalEnvironmentVariable(VarName, out _, 1024);
        string varVal = Marshal.PtrToStringUni(pointer);
        Marshal.FreeHGlobal(pointer);
        return varVal;
    }

    /// <summary>
    /// Sets a global environment variable with the specified name to the provided string value.
    /// </summary>
    /// <remarks>
    /// The variable is set globally and may affect other processes or components that read environment variables.
    /// The value is stored as a Unicode string. Ensure that the variable name and value conform to any platform-specific restrictions.
    /// </remarks>
    /// <param name="VarName">The name of the environment variable to set. Cannot be null or empty.</param>
    /// <param name="Value">The string value to assign to the environment variable. Cannot be null.</param>
    public static void WriteStringVariable(string VarName, string Value)
    {
        IntPtr pointer = Marshal.StringToHGlobalUni(Value);
        SetGlobalEnvironmentVariable(VarName, pointer, Value.Length * 2);
        Marshal.FreeHGlobal(pointer);
    }

    /// <summary>
    /// Retrieves the value of a global environment variable and returns it as a value type.
    /// </summary>
    /// <remarks>
    /// This method reads the variable from the global environment and marshals it to the specified value type.
    /// The caller must ensure that the variable exists and is compatible with the requested type.
    /// If the variable does not exist or cannot be marshaled to type T, the result may be undefined.
    /// </remarks>
    /// <typeparam name="T">The value type to which the environment variable will be marshaled. Must be a struct.</typeparam>
    /// <param name="VarName">The name of the global environment variable to read. Cannot be null or empty.</param>
    /// <returns>The value of the specified environment variable, marshaled to type T.</returns>
    public static T ReadVariable<T>(string VarName) where T : struct
    {
        // Getting variable data
        int ptrSize = Marshal.SizeOf<T>();
        IntPtr pointer = GetGlobalEnvironmentVariable(VarName, out _, ptrSize);

        T varVal = Marshal.PtrToStructure<T>(pointer);
        Marshal.FreeHGlobal(pointer);
        return varVal;
    }

    /// <summary>
    /// Writes a value of a specified struct type to a global environment variable identified by name.
    /// </summary>
    /// <remarks>
    /// This method stores the value as a binary representation using unmanaged memory.
    /// The caller should ensure that the variable name is unique and valid within the global environment.
    /// Only value types (structs) are supported.</remarks>
    /// <typeparam name="T">The struct type of the value to write to the environment variable.</typeparam>
    /// <param name="VarName">The name of the global environment variable to set. Cannot be null or empty.</param>
    /// <param name="Value">The value to assign to the environment variable.</param>
    public static void WriteVariable<T>(string VarName, T Value) where T : struct
    {
        // Getting variable data
        int ptrSize = Marshal.SizeOf<T>();
        IntPtr pointer = Marshal.AllocHGlobal(ptrSize);

        // Writing data
        Marshal.StructureToPtr(Value, pointer, false);
        SetGlobalEnvironmentVariable(VarName, pointer, ptrSize);

        // Freeing
        Marshal.FreeHGlobal(pointer);
    }

    /// <summary>
    /// Retrieves the value of a global environment variable as an array of value type elements.
    /// </summary>
    /// <remarks>
    /// The method reads the raw memory of the environment variable and interprets it as an array of the specified value type.
    /// Use this method only when the variable is known to contain binary data compatible with the type T.
    /// The caller is responsible for ensuring the variable's format matches the expected type.
    /// </remarks>
    /// <typeparam name="T">The value type of the elements to read from the environment variable.</typeparam>
    /// <param name="VarName">The name of the global environment variable to retrieve. Cannot be null or empty.</param>
    /// <returns>An array of elements of type T containing the values stored in the specified environment variable. The array
    /// will be empty if the variable contains no data.</returns>
    public static T[] ReadArrayVariable<T>(string VarName) where T : struct
    {
        // Getting variable data
        int ptrSize = Marshal.SizeOf<T>();
        IntPtr Data = GetGlobalEnvironmentVariable(VarName, out int DataSize, 256);

        // Parsing values
        int length = DataSize / ptrSize;
        T[] varVal = Data.PinnedMemoryToArray<T>(length);

        // Freeing
        Marshal.FreeHGlobal(Data);
        return varVal;
    }

    /// <summary>
    /// Writes a global environment variable as an array of value type elements.
    /// </summary>
    /// <remarks>
    /// This method pins the array in memory and writes its contents to the specified global environment variable.
    /// The variable is set as a contiguous block of memory representing the array.
    /// Use caution when passing large arrays, as this operation involves unmanaged memory handling.
    /// </remarks>
    /// <typeparam name="T">The value type of the elements in the array to be written.</typeparam>
    /// <param name="VarName">The name of the global environment variable to set. Cannot be null or empty.</param>
    /// <param name="Value">The array of value type elements to write. Cannot be null and must contain at least one element.</param>
    public static void WriteArrayVariable<T>(string VarName, T[] Value) where T : struct
    {
        // Formating new value
        int ptrSize = Marshal.SizeOf<T>();
        IntPtr handle = Marshal.UnsafeAddrOfPinnedArrayElement(Value, 0);

        // Setting variable
        SetGlobalEnvironmentVariable(VarName, handle, Value.Length * ptrSize);
    }

    /// <summary>
    /// Sets a global environment variable with the specified name and value pointer.
    /// </summary>
    /// <remarks>
    /// This method sets a global environment variable using a native identifier. Use this method
    /// when you need to update environment variables that are shared across the firmware or system context. The caller
    /// is responsible for ensuring that the pointer and size are valid and that the memory referenced remains
    /// accessible for the duration required by the environment.
    /// </remarks>
    /// <param name="VarName">The name of the environment variable to set. Cannot be null or empty.</param>
    /// <param name="Value">A pointer to the value to assign to the environment variable.</param>
    /// <param name="PtrSize">The size, in bytes, of the value pointed. Must be greater than zero.</param>
    public static void SetGlobalEnvironmentVariable(string VarName, IntPtr Value, int PtrSize)
        => SetEnvironmentVariable(VarName, NativeMethods._FirmwareGlobalEnvironmentIdentificator, Value, PtrSize);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="DataLength"></param>
    /// <param name="VarSize"></param>
    /// <returns></returns>
    public static IntPtr GetGlobalEnvironmentVariable(string VarName, out int DataLength, int VarSize = 4)
        => GetEnvironmentVariable(VarName, NativeMethods._FirmwareGlobalEnvironmentIdentificator, out DataLength, VarSize);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="attributes"></param>
    /// <param name="Value"></param>
    /// <param name="PtrSize"></param>
    public static void SetGlobalEnvironmentVariableEx(string VarName, VariableAttributes attributes, IntPtr Value, int PtrSize)
        => SetEnvironmentVariableEx(VarName, NativeMethods._FirmwareGlobalEnvironmentIdentificator, attributes, Value, PtrSize);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="attributes"></param>
    /// <param name="DataLength"></param>
    /// <param name="VarSize"></param>
    /// <returns></returns>
    public static IntPtr GetGlobalEnvironmentVariableEx(string VarName, out VariableAttributes attributes, out int DataLength, int VarSize = 4)
        => GetEnvironmentVariableEx(VarName, NativeMethods._FirmwareGlobalEnvironmentIdentificator, out attributes, out DataLength, VarSize);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="EnvironmentIdentificator"></param>
    /// <param name="Value"></param>
    /// <param name="PtrSize"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="FirmwareEnvironmentException"></exception>
    public static void SetEnvironmentVariable(string VarName, Guid EnvironmentIdentificator, IntPtr Value, int PtrSize)
    {
        if (!FirmwareInterface.Available)
            throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

        try
        {
            // Execution and error check
            using (new SystemEnvironmentPriviledge())
            {
                if (!NativeMethods.SetFirmwareEnvironmentVariableW(VarName, "{" + EnvironmentIdentificator.ToString() + "}", Value, PtrSize))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }
        catch (Exception ex)
        {
            // Something wrong happened
            throw new FirmwareEnvironmentException("Failed to write environment variable", ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="EnvironmentIdentificator"></param>
    /// <param name="DataLength"></param>
    /// <param name="VarSize"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="FirmwareEnvironmentException"></exception>
    public static IntPtr GetEnvironmentVariable(string VarName, Guid EnvironmentIdentificator, out int DataLength, int VarSize = 4) // 4 - int size
    {
        if (!FirmwareInterface.Available)
            throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

        // Data
        IntPtr pointer = IntPtr.Zero;
        DataLength = 0;

        try
        {
            // Allocate variable pointer
            pointer = Marshal.AllocHGlobal(VarSize);

            // Reading variable
            using (new SystemEnvironmentPriviledge())
            {
                DataLength = (int)NativeMethods.GetFirmwareEnvironmentVariableW(VarName, "{" + EnvironmentIdentificator.ToString() + "}", pointer, VarSize);
            }

            // Error check
            if (DataLength == 0)
            {
                Marshal.FreeHGlobal(pointer);
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }

            return pointer;
        }
        catch (Exception ex)
        {
            // Something wrong happened
            Marshal.FreeHGlobal(pointer);
            throw new FirmwareEnvironmentException("Failed to read environment variable", ex);
        }
    }

    /// <summary>
    /// Sets the value of an environment variable with the specified name and attributes in the specified environment.
    /// The variable is stored in non-volatile memory and will persist across system reboots.
    /// The variable is associated with the specified environment, which is identified by a GUID.
    /// The attributes parameter specifies the attributes of the variable, such as whether it is read-only or can be accessed from user mode.
    /// The value parameter is a pointer to a buffer that contains the data to be stored in the variable, and ptrSize specifies the size of the buffer in bytes.
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="EnvironmentIdentificator"></param>
    /// <param name="attributes"></param>
    /// <param name="Value"></param>
    /// <param name="PtrSize"></param>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="FirmwareEnvironmentException"></exception>
    public static void SetEnvironmentVariableEx(string VarName, Guid EnvironmentIdentificator, VariableAttributes attributes, IntPtr Value, int PtrSize)
    {
        if (!FirmwareInterface.Available)
            throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

        try
        {
            // Execution and error check
            using (new SystemEnvironmentPriviledge())
            {
                if (!NativeMethods.SetFirmwareEnvironmentVariableExW(VarName, "{" + EnvironmentIdentificator.ToString() + "}", Value, PtrSize, attributes))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }
        catch (Exception ex)
        {
            // Something wrong happened
            throw new FirmwareEnvironmentException("Failed to write environment variable", ex);
        }
    }

    /// <summary>
    /// Gets the value of an environment variable with the specified name and attributes from the specified environment.
    /// </summary>
    /// <param name="VarName"></param>
    /// <param name="EnvironmentIdentificator"></param>
    /// <param name="attributes"></param>
    /// <param name="DataLength"></param>
    /// <param name="VarSize"></param>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    /// <exception cref="FirmwareEnvironmentException"></exception>
    public static IntPtr GetEnvironmentVariableEx(string VarName, Guid EnvironmentIdentificator, out VariableAttributes attributes, out int DataLength, int VarSize = 4) // 4 - int size
    {
        // Data
        IntPtr pointer = IntPtr.Zero;
        DataLength = 0;

        if (!FirmwareInterface.Available)
            throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

        try
        {
            // Allocate variable pointer
            pointer = Marshal.AllocHGlobal(VarSize);

            // Reading variable
            using (new SystemEnvironmentPriviledge())
                DataLength = (int)NativeMethods.GetFirmwareEnvironmentVariableExW(VarName, "{" + EnvironmentIdentificator.ToString() + "}", pointer, VarSize, out attributes);

            // Error check
            if (DataLength == 0)
            {
                Marshal.FreeHGlobal(pointer);
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode);
            }

            return pointer;
        }
        catch (Exception ex)
        {
            // Something wrong happened
            Marshal.FreeHGlobal(pointer);
            throw new FirmwareEnvironmentException("Failed to read environment variable", ex);
        }
    }

    /// <summary>
    /// Manages the temporary elevation of the current process to enable the system environment privilege.
    /// </summary>
    /// <remarks>
    /// This class acquires the system environment privilege for the lifetime of the instance and reverts the privilege when disposed.
    /// It is intended for use with a using statement to ensure proper privilege management and resource cleanup.
    /// This class is not thread-safe.
    /// </remarks>
    private class SystemEnvironmentPriviledge : IDisposable
    {
        private readonly IntPtr hToken = IntPtr.Zero;
        private NativeMethods.TokenPrivelege tp = new NativeMethods.TokenPrivelege()
        {
            Count = 1,
            Luid = 0,
            Attr = NativeMethods.SE_PRIVILEGE_ENABLED
        };

        public SystemEnvironmentPriviledge()
        {
            // Getting process token
            if (!NativeMethods.OpenProcessToken(NativeMethods.GetCurrentProcess(), NativeMethods.TOKEN_ADJUST_PRIVILEGES | NativeMethods.TOKEN_QUERY, ref hToken))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open process token");

            // Getting priviledge info
            if (!NativeMethods.LookupPrivilegeValue(IntPtr.Zero, NativeMethods.SE_SYSTEM_ENVIRONMENT_NAME, ref tp.Luid))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to lookup process privelage value");

            // Promoting process
            if (!NativeMethods.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to adjust process token");
        }

        public void Dispose()
        {
            // Changing privilege state
            tp.Attr = 0;

            // Degrade process
            if (!NativeMethods.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to adjust process token");

            // Freeing process handle
            NativeMethods.CloseHandle(hToken);
        }
    }

    private static class NativeMethods
    {
        public static readonly Guid _FirmwareGlobalEnvironmentIdentificator = new Guid("8BE4DF61-93CA-11D2-AA0D-00E098032B8C");
        public const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const int TOKEN_QUERY = 0x00000008;
        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int ERROR_INVALID_FUNCTION = 1;

        public static bool Promoted = false;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(IntPtr host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokenPrivelege newst, int len, IntPtr prev, IntPtr prevlen);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetFirmwareEnvironmentVariableExW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpGuid,
            IntPtr pBuffer,
            int nSize,
            out VariableAttributes Attributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetFirmwareEnvironmentVariableExW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpGuid,
            IntPtr pValue,
            int nSize,
            VariableAttributes Attributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetFirmwareEnvironmentVariableW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpGuid,
            IntPtr pBuffer,
            int nSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetFirmwareEnvironmentVariableW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpGuid,
            IntPtr pValue,
            int nSize);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TokenPrivelege
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
    }
}

/// <summary>
/// Represents an error that occurred while working with UEFI
/// </summary>
public class FirmwareEnvironmentException : Exception
{
    /// <inheritdoc/>
    public FirmwareEnvironmentException(string Message)
        : base(Message) { }

    /// <inheritdoc/>
    public FirmwareEnvironmentException(string Message, Exception inner)
        : base(Message, inner) { }
}
