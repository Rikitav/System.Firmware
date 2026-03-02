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

namespace System.Firmware;

/// <summary>
/// Attributes bitmask to set for the variable
/// </summary>
[Flags]
public enum VariableAttributes : int
{
    /// <summary>
    /// The variable is accessible from a non volatile environment
    /// </summary>
    NON_VOLATILE = 0x00000001,

    /// <summary>
    /// The variable is available while the Boot service is running
    /// </summary>
    BOOTSERVICE_ACCESS = 0x00000002,

    /// <summary>
    /// The variable is available at runtime
    /// </summary>
    RUNTIME_ACCESS = 0x00000004,

    /// <summary>
    /// NoDescription
    /// </summary>
    HARDWARE_ERROR_RECORD = 0x00000008,

    /// <summary>
    /// The variable is available only to authorized sources
    /// </summary>
    AUTHENTICATED_WRITE_ACCESS = 0x00000010,

    /// <summary>
    /// NoDescription
    /// </summary>
    TIME_BASED_AUTHENTICATED_WRITE_ACCESS = 0x00000020,

    /// <summary>
    /// NoDescription
    /// </summary>
    APPEND_WRITE = 0x00000040,

    /// <summary>
    /// NoDescription
    /// </summary>
    ENHANCED_AUTHENTICATED_ACCESS = 0x00000080
}
