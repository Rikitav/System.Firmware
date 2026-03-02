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

using System.Firmware.Win32Native;
using System.IO;

namespace System.Firmware.BootService.DevicePathProtocols;

/// <summary>
/// The default instance for protocols, has no abstractions, but only raw data
/// </summary>
public class RawMediaDevicePath(DeviceProtocolType type, byte subType) : DevicePathProtocolBase(type, subType)
{
    /// <summary>
    /// <para>GUID - An EFI GUID in standard format xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx.</para>
    /// <para>Keyword - In some cases, one of a series of keywords must be listed.</para>
    /// <para>Integer - Unless otherwise specified, this indicates an unsigned integer in the range of 0 to 2^32-1. The value is decimal, unless preceded by �0x� or �0X�, in which case it is hexadecimal.</para>
    /// <para>EISAID - A seven character text identifier in the format used by the ACPI specification. The first three characters must be alphabetic, either upper or lower case. The second four characters are hexadecimal digits, either numeric, upper case or lower case. Optionally, it can be the number 0.</para>
    /// <para>String - Series of alphabetic, numeric and punctuation characters not including a right parenthesis �)�, bar �</para>
    /// <para>HexDump - Series of bytes, represented by two hexadecimal characters per byte. Unless otherwise indicated, the size is only limited by the length of the device node.</para>
    /// <para>IPv4 Address - Series of four integer values (each between 0-255), separated by a �.� Optionally, followed by a �:� and an integer value between 0-65555. If the �:� is not present, then the port value is zero.</para>
    /// <para>IPv6 Address - IPv6 Address is expressed in the format [address]:port. The �address� is expressed in the way defined in RFC4291 Section 2.2. The �:port� after the [address] is optional. If present, the �port� is an integer value between 0-65535 to represent the port number, or else, port number is zero.</para>
    /// </summary>
    public byte[] ProtocolData { get; private set; } = [];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="subType"></param>
    /// <param name="protocolData"></param>
    public RawMediaDevicePath(DeviceProtocolType type, byte subType, byte[] protocolData) : this(type, subType)
    {
        ProtocolData = protocolData;
    }

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength()
        => (ushort)ProtocolData.Length;

    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
        => ProtocolData = reader.ReadBytes(length);

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
        => writer.Write(ProtocolData);
}
