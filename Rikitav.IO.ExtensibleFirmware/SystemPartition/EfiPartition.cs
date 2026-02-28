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

using Rikitav.IO.ExtensibleFirmware.Win32Native;
using System;
using System.IO;

namespace Rikitav.IO.ExtensibleFirmware.SystemPartition;

/// <summary>
/// System partition containing boot files of operating systems
/// </summary>
public static class EfiPartition
{
    private static readonly PARTITION_INFORMATION_EX _partitionInfo = FindEfiPartitionInfo();
    private static readonly Guid _partitionInfoTypeId = Guid.Parse("c12a7328-f81f-11d2-ba4b-00a0c93ec93b");
    
    private static string? _rawFullPath = null;
    private static DirectoryInfo? _directoryInfo = null;

    /// <summary>
    /// GUID identifier of the system EFI partition.
    /// This is a unique identifier assigned to the partition when it is created,
    /// and can be used to identify the partition among other partitions on the same disk.
    /// It is not related to the partition type, and is not a constant value.
    /// It can be used to access the partition's files and directories or passed to Win32 API functions that require a volume path.
    /// </summary>
    public static Guid Identificator
    {
        get => _partitionInfo.Gpt.PartitionId;
    }

    /// <summary>
    /// GUID identifier of the system EFI partition type.
    /// This is a constant value defined by the UEFI specification for EFI System Partitions,
    /// and is used to identify partitions that are intended to be used as EFI System Partitions.
    /// </summary>
    public static Guid TypeID
    {
        get => _partitionInfoTypeId;
    }

    /// <summary>
    /// Full path to the system EFI partition, in the form of "\\?\Volume{GUID}\".
    /// This path can be used to access the partition's files and directories or passed to Win32 API functions that require a volume path.
    /// Note that this path is not a drive letter and cannot be used with functions that expect a drive letter.
    /// 
    /// Use <see cref="VolumeDirectory"/> to access the partition's files and directories using .NET's file system APIs.
    /// </summary>
    /// <returns></returns>
    public static string VolumeFullPath
    {
        get => _rawFullPath ??= string.Concat(@"\\?\Volume{", Identificator.ToString(), @"}\");
    }

    /// <summary>
    /// <see cref="DirectoryInfo"/> of system EFI partition.
    /// This can be used to access the partition's files and directories using .NET's file system APIs.
    /// Note that this is not a drive letter and cannot be used with functions that expect a drive letter.
    /// 
    /// Use <see cref="VolumeFullPath"/> to get the partition's full path for use with Win32 API functions that require a volume path.
    /// </summary>
    /// <returns></returns>
    public static DirectoryInfo VolumeDirectory
    {
        get => _directoryInfo ??= new DirectoryInfo(VolumeFullPath);
    }

    /// <summary>
    /// Get volume information for system EFI partition.
    /// Avoid using this propert, unless you need to access the partition's low-level properties that are not provided by <see cref="VolumeDirectory"/> or <see cref="VolumeFullPath"/>, like partition length or starting offset.
    /// </summary>
    /// <returns></returns>
    public static PARTITION_INFORMATION_EX PartitionInfo
    {
        get => _partitionInfo;
    }

    private static PARTITION_INFORMATION_EX FindEfiPartitionInfo()
    {
        if (!FirmwareInterface.Available)
            throw new PlatformNotSupportedException("Executing on non UEFI System");

        foreach (PARTITION_INFORMATION_EX partition in new IoctlVolumeEnumerable(0))
        {
            if (partition.PartitionStyle != PartitionStyle.GuidPartitionTable)
                throw new DriveNotFoundException("Drive signature is not GPT (Guid Partition Table)");

            if (partition.Gpt.PartitionType == TypeID)
                return partition;
        }

        throw new DriveNotFoundException("Efi partition was not found");
    }
}
