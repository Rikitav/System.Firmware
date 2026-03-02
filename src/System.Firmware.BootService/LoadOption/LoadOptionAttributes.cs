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

using System;

namespace System.Firmware.BootService.LoadOption;

/// <summary>
/// The attributes for this load option entry
/// </summary>
[Flags]
public enum LoadOptionAttributes : uint
{
    /// <summary>
    /// If a load option is marked as LOAD_OPTION_ACTIVE, the boot manager will attempt to boot automatically using the device path information in the load option
    /// </summary>
    ACTIVE = 0x00000001,

    /// <summary>
    /// If any Driver#### load option is marked as LOAD_OPTION_FORCE_RECONNECT , then all of the UEFI drivers in the system will be disconnected and reconnected after the last Driver#### load option is processed
    /// </summary>
    FORCE_RECONNECT = 0x00000002,

    /// <summary>
    /// If any Boot#### load option is marked as LOAD_OPTION_HIDDEN , then the load option will not appear in the menu (if any) provided by the boot manager for load option selection
    /// </summary>
    HIDDEN = 0x00000008,

    /// <summary>
    /// The LOAD_OPTION_CATEGORY is a sub-field of Attributes that provides details to the boot manager to describe how it should group the Boot#### load options
    /// </summary>
    CATEGORY = 0x00001F00,

    /// <summary>
    /// Boot#### load options with LOAD_OPTION_CATEGORY set to LOAD_OPTION_CATEGORY_BOOT are meant to be part of the normal boot processing
    /// </summary>
    CATEGORY_BOOT = 0x00000000,

    /// <summary>
    /// Boot#### load options with LOAD_OPTION_CATEGORY set to LOAD_OPTION_CATEGORY_APP are executables which are not part of the normal boot processing but can be optionally chosen for execution if boot menu is provided, or via Hot Keys
    /// </summary>
    CATEGORY_APP = 0x00000100
}
