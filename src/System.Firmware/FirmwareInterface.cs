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

using System.Firmware.SystemPartition;
using System;
using System.IO;

namespace System.Firmware;

/// <summary>
/// 
/// </summary>
public static class FirmwareInterface
{
    /// <summary>
    /// Checks whether the UEFI platform is available on this system
    /// </summary>
    /// <returns>If available, return <see langword="true"/>, else <see langword="false"/></returns>
    public static bool Available
    {
        get => FirmwareUtilities.CheckFirmwareAvailablity();
    }

    /// <summary>
    /// Searches among the partition for the one that is marked as EfiSystemPartition
    /// </summary>
    /// <returns><see cref="DirectoryInfo"/> of EfiSystemPartition</returns>
    public static DirectoryInfo SystemPartition
    {
        get
        {
            if (!Available)
                throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

            return EfiPartition.VolumePath;
        }
    }

    /// <summary>
    /// Boot into the UEFI user interface after rebooting the computer. Does NOT reboot the computer, but sets the boot condition
    /// </summary>
    public static void BootToUserInterface()
    {
        if (!Available)
            throw new PlatformNotSupportedException("This system does not support UEFI, or is loaded in LEGACY mode");

        if (!FirmwareGlobalEnvironment.OsIndicationsSupported.HasFlag(EfiOsIindications.BOOT_TO_FW_UI))
            throw new PlatformNotSupportedException("Current UEFI platform does not support force reboot in Firmware UI");

        FirmwareGlobalEnvironment.OsIndications |= EfiOsIindications.BOOT_TO_FW_UI;
    }
}
