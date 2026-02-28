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
using System.Runtime.InteropServices;

#pragma warning disable CS1591
namespace Rikitav.IO.ExtensibleFirmware.Win32Native;

/// <summary>
/// Specifies the partition style used on a disk, such as Master Boot Record (MBR), GUID Partition Table (GPT), or raw
/// (uninitialized).
/// </summary>
/// <remarks>
/// Use this enumeration to determine or specify how a disk is partitioned. The partition style affects
/// compatibility, maximum disk size, and the number of supported partitions. MBR is commonly used for legacy BIOS
/// systems, while GPT is required for disks larger than 2 TB and is standard for UEFI-based systems. The Raw value
/// indicates that the disk has not been initialized with a partition style.
/// </remarks>
public enum PartitionStyle : uint
{
    MasterBootRecord,
    GuidPartitionTable,
    Raw
}

/// <summary>
/// Contains extended information about a drive's partitions.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct DRIVE_LAYOUT_INFORMATION_EX
{
    [FieldOffset(0)] public PartitionStyle PartitionStyle;
    [FieldOffset(4)] public uint PartitionCount;
    [FieldOffset(8)] public IntPtr Mbr;
    [FieldOffset(8)] public IntPtr Gpt;
    [FieldOffset(48)] public PARTITION_INFORMATION_EX PartitionEntry;
}

/// <summary>
/// Contains GUID partition table (GPT) partition information.
/// </summary>
[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
public struct PARTITION_INFORMATION_GPT
{
    [FieldOffset(0)] public Guid PartitionType;
    [FieldOffset(16)] public Guid PartitionId;
    [FieldOffset(32)] public ulong Attributes;
    [FieldOffset(40), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)] public string Name;
}

/// <summary>
/// Contains information about a disk partition.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct PARTITION_INFORMATION_EX
{
    [FieldOffset(0)] public PartitionStyle PartitionStyle;
    [FieldOffset(8)] public long StartingOffset;
    [FieldOffset(16)] public long PartitionLength;
    [FieldOffset(24)] public uint PartitionNumber;
    [FieldOffset(28), MarshalAs(UnmanagedType.I1)] public bool RewritePartition;
    [FieldOffset(32)] public IntPtr Mbr;
    [FieldOffset(32)] public PARTITION_INFORMATION_GPT Gpt;
}
