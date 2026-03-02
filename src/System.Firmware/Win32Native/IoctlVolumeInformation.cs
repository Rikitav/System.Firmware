using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Firmware.Win32Native;

public class IoctlVolumeInformation
{
    public static PARTITION_INFORMATION_EX GetPartition(VolumePath volumePath)
    {
        if (volumePath == Guid.Empty)
            return default;

        IntPtr partHandle = NativeMethods.CreateFile(volumePath, 0, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        // Checking for INVALID_HANDLE_VALUE
        if (partHandle == new IntPtr(-1))
        {
            int lastError = Marshal.GetLastWin32Error();
            throw new Win32Exception(lastError, "Failed to open partition descriptor");
        }

        // Allocating structure memory
        int StructSize = Marshal.SizeOf<PARTITION_INFORMATION_EX>();
        IntPtr StructPtr = Marshal.AllocHGlobal(StructSize);
        Marshal.StructureToPtr(new PARTITION_INFORMATION_EX(), StructPtr, true);

        //Executing DeviceIoControl()
        if (!NativeMethods.DeviceIoControl(partHandle, NativeMethods.DiskGetPartitionInfoEx, IntPtr.Zero, 0, StructPtr, (uint)StructSize, out _, IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to obtain PARTITION_INFORMATION_EX structure");

        //creating new struct from allocated byte buffer
        PARTITION_INFORMATION_EX partInfoEx = Marshal.PtrToStructure<PARTITION_INFORMATION_EX>(StructPtr);
        Marshal.FreeHGlobal(StructPtr);

        return partInfoEx;
    }

    private static class NativeMethods
    {
        public const uint DiskGetPartitionInfoEx = 0x00000007 << 16 | 0x0012 << 2 | 0 | 0 << 14;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint IoControlCode,
            [In] IntPtr InBuffer,
            uint nInBufferSize,
            [Out] IntPtr OutBuffer,
            uint nOutBufferSize,
            out uint pBytesReturned,
            [In] IntPtr Overlapped);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPWStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            [In] IntPtr templateFile);
    }
}
