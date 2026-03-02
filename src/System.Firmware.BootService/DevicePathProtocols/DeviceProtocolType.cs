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

namespace System.Firmware.BootService.DevicePathProtocols;

/// <summary>
/// Type of device path protocol
/// </summary>
public enum DeviceProtocolType : byte
{
    /// <summary>
    /// Hardware Device Path
    /// </summary>
    Hardware = 0x01,

    /// <summary>
    /// ACPI Device Path
    /// </summary>
    ACPI = 0x02,

    /// <summary>
    /// Messaging Device Path
    /// </summary>
    Message = 0x03,

    /// <summary>
    /// Media Device Path
    /// </summary>
    Media = 0x04,

    /// <summary>
    /// BIOS Boot Specification Device Path
    /// </summary>
    BIOS = 0x05,

    /// <summary>
    /// End of Hardware Device Path
    /// </summary>
    End = 0x7F
}
