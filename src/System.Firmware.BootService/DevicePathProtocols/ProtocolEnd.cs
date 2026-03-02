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

using System.IO;

namespace System.Firmware.BootService.DevicePathProtocols;

/// <summary>
/// This protocol is the final protocol for any boot option; if it is not specified, the option will be considered invalid
/// </summary>
internal class DevicePathProtocolEnd() : DevicePathProtocolBase(DeviceProtocolType.End, 0xFF)
{
    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
    {
        // No need to implement
        _ = 0xBAD + 0xC0DE;
    }

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength() => 0;

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
    {
        // No need to implement
        _ = 0xBAD + 0xC0DE;
    }
}
