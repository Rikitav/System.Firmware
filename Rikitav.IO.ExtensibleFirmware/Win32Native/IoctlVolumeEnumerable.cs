// Rikitav.IO.ExtensibleFirmware
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Rikitav.IO.ExtensibleFirmware.Win32Native;

/// <summary>
/// Enumerable collection of volumes by physical hard drive index
/// </summary>
public class IoctlVolumeEnumerable : IEnumerable<PARTITION_INFORMATION_EX>
{
    private readonly int _driveIndex = 0;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new IoctlVolumeEnumerator(_driveIndex);
    }

    /// <summary>
    /// Create new instance of <see cref="IoctlVolumeEnumerable"/>'s enumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator<PARTITION_INFORMATION_EX> GetEnumerator()
    {
        return new IoctlVolumeEnumerator(_driveIndex);
    }

    /// <summary>
    /// Create new instance of <see cref="IoctlVolumeEnumerable"/>
    /// </summary>
    /// <param name="DriveIndex"></param>
    public IoctlVolumeEnumerable(int DriveIndex = 0)
    {
        _driveIndex = DriveIndex;
    }
}

internal class IoctlVolumeEnumerator : IEnumerator<PARTITION_INFORMATION_EX>
{
    // Unmanaged resources
    private IntPtr _physicalDriveHandle = IntPtr.Zero;
    private IntPtr _driveLayoutStructPtr = IntPtr.Zero;

    // Managed resources
    private int _driveLayoutPartitionCount = -1;
    private int _currentPartitionEntryIndex = -1;
    private long _partitionEntryAddress = -1;

    // IEnumerator realization
    public PARTITION_INFORMATION_EX Current { get; private set; }
    object IEnumerator.Current => Current;

    public bool IsDisposed
    {
        get;
        private set;
    } = true;

    public IoctlVolumeEnumerator(int DriveIndex = 0)
    {
        // Enumerate all partitions by WinApi FindVolume function
        _physicalDriveHandle = NativeMethods.CreateFile(
            "\\\\.\\PhysicalDrive" + DriveIndex,
            NativeMethods.GENERIC_READWRITE,
            NativeMethods.FILE_SHARE_READWRITE,
            IntPtr.Zero,
            NativeMethods.OPEN_EXISTING,
            NativeMethods.FILE_ATTRIBUTE_READONLY,
            IntPtr.Zero);

        // Checking handle value
        if (_physicalDriveHandle == NativeMethods.INVALID_HANDLE_VALUE)
        {
            int lastError = Marshal.GetLastWin32Error();
            if (lastError == NativeMethods.ERROR_FILE_NOT_FOUND)
            {
                // Drive doesnt exists
                throw new DriveNotFoundException("Drive doesnt exist");
            }
            else
            {
                // Something went wrong
                throw new Win32Exception(lastError, "Failed to create drive handle");
            }
        }

        // Initilizing drive layout info
        Reset();
        IsDisposed = false;
    }

    public bool MoveNext()
    {
        // Index in bounds
        if (_currentPartitionEntryIndex >= _driveLayoutPartitionCount)
            return false;

        try
        {
            // Finding the offset of the desired structure and marshalling current indexed structure
            IntPtr partitionEntryPtr = new IntPtr(_partitionEntryAddress + _currentPartitionEntryIndex * Marshal.SizeOf<PARTITION_INFORMATION_EX>());
            PARTITION_INFORMATION_EX partitionEntryInfo = Marshal.PtrToStructure<PARTITION_INFORMATION_EX>(partitionEntryPtr);

            // Success
            Current = partitionEntryInfo;
            _currentPartitionEntryIndex++;
            return true;
        }
        catch
        {
            // something went wrong
            return false;
        }
    }

    public void Reset()
    {
        // Allocating memory for drive layout info structure
        int PtrSize = 512;
        _driveLayoutStructPtr = Marshal.AllocHGlobal(PtrSize);

        try
        {
            // Trying to get structure
            for (int Attempt = 0; Attempt < 5; Attempt++)
            {
                // Calling DeviceIoControl
                if (!NativeMethods.DeviceIoControl(_physicalDriveHandle, NativeMethods.IOCTL_DISK_GET_DRIVE_LAYOUT_EX, IntPtr.Zero, 0, _driveLayoutStructPtr, PtrSize, out _, IntPtr.Zero))
                {
                    // Error check
                    int lastError = Marshal.GetLastWin32Error();
                    
                    // If error is not insufficient buffer? then it fatal
                    if (lastError != NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                        throw new Win32Exception(lastError, "Failed to get drive layout");

                    // Allocating more memory for drive layout info structure
                    _driveLayoutStructPtr = Marshal.ReAllocHGlobal(_driveLayoutStructPtr, new IntPtr(PtrSize += 512));
                    continue;
                }

                // Getting count of partitions from drive layout structure
                long partitionCountAdress = _driveLayoutStructPtr.ToInt64() + Marshal.OffsetOf<DRIVE_LAYOUT_INFORMATION_EX>(nameof(DRIVE_LAYOUT_INFORMATION_EX.PartitionCount)).ToInt64();
                _driveLayoutPartitionCount = Marshal.ReadInt32(new IntPtr(partitionCountAdress));

                // Getting adress of 'PartitionEntry' field from drive layout structure
                _partitionEntryAddress = _driveLayoutStructPtr.ToInt64() + Marshal.OffsetOf<DRIVE_LAYOUT_INFORMATION_EX>(nameof(DRIVE_LAYOUT_INFORMATION_EX.PartitionEntry)).ToInt64();
                break;
            }
        }
        finally
        {
            // Resetting enumerating index
            _currentPartitionEntryIndex = 0;
        }
    }

    public void Dispose()
    {
        // Dont dispose id already
        if (IsDisposed)
            return;

        // Freeing unmanaged resource
        Marshal.FreeHGlobal(_physicalDriveHandle);
        Marshal.FreeHGlobal(_driveLayoutStructPtr);

        // Nullify unmanaged resources
        _physicalDriveHandle = IntPtr.Zero;
        _driveLayoutStructPtr = IntPtr.Zero;

        // Nullify managed resources
        _driveLayoutPartitionCount = -1;
        _currentPartitionEntryIndex = -1;
        _partitionEntryAddress = -1;

        // Do not garbage collect
        GC.SuppressFinalize(this);
        IsDisposed = true;
    }

    private static class NativeMethods
    {
        public const uint GENERIC_READWRITE = 0x01U << 30 | 0x01U << 31;
        public const uint FILE_SHARE_READWRITE = 0x00000001U | 0x00000002U;
        public const uint FILE_ATTRIBUTE_READONLY = 0x00000001U;
        public const uint OPEN_EXISTING = 3U;

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public const uint IOCTL_DISK_GET_DRIVE_LAYOUT_EX = 0x00070050U;
        public const uint ERROR_INSUFFICIENT_BUFFER = 0x7A;
        public const uint ERROR_FILE_NOT_FOUND = 0x2;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            int nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            int nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr CreateFile(
           string lpFileName,
           uint dwDesiredAccess,
           uint dwShareMode,
           IntPtr SecurityAttributes,
           uint dwCreationDisposition,
           uint dwFlagsAndAttributes,
           IntPtr hTemplateFile);
    }
}
