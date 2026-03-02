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

namespace System.Firmware.SystemPartition;

/// <summary>
/// Enumeration of supported processor architectures for firmware applications
/// </summary>
public enum FirmwareApplicationArchitecture : ushort
{
    /// <summary>
    /// Unknown architecture
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 32-bit
    /// </summary>
    Ia32 = 0x14c,
    
    /// <summary>
    /// AMD64
    /// </summary>
    x64 = 0x8664,

    /// <summary>
    /// Intel Itanium 64
    /// </summary>
    Ia64 = 0x200,

    /// <summary>
    /// AArch32 architecture
    /// </summary>
    Arm = 0x1c2,

    /// <summary>
    /// AArch64 architecture
    /// </summary>
    AArch64 = 0xaa64,

    /// <summary>
    /// RISC-V 32-bit architecture
    /// </summary>
    RISC_V32 = 0x5032,

    /// <summary>
    /// RISC-V 64-bit architecture
    /// </summary>
    RISC_V64 = 0x5064,

    /// <summary>
    /// RISC-V 128-bit architecture
    /// </summary>
    RISC_V128 = 0x5128
}
