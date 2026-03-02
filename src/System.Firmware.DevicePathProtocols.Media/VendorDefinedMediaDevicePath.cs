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

using System.Firmware.BootService.DevicePathProtocols;
using System.Firmware.Win32Native;
using System;
using System.IO;

namespace System.Firmware.MediaDevicePathProtocols;

/// <summary>
/// Vendor-Defined Media Device Path
/// https://uefi.org/specs/UEFI/2.10/10_Protocols_Device_Path_Protocol.html#vendor-defined-media-device-path
/// </summary>
[DefineDevicePathProtocol(DeviceProtocolType.Media, 3)]
public class VendorDefinedMediaDevicePath() : DevicePathProtocolBase(DeviceProtocolType.Media, 3)
{
    /// <summary>
    /// Vendor-assigned GUID that defines the data that follows
    /// </summary>
    public Guid VendorGuid { get; set; }

    /// <summary>
    /// Vendor-defined variable size data.
    /// </summary>
    public byte[] VendorDefinedData { get; set; } = [];

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength()
    {
        // VendorGuid (16 bytes) + VendorDefinedData
        return (ushort)(16 + VendorDefinedData.Length);
    }

    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
    {
        VendorGuid = reader.ReadGuid();
        VendorDefinedData = reader.ReadBytes(length - 16);
    }

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(VendorGuid.ToByteArray());
        writer.Write(VendorDefinedData);
    }

    /// <inheritdoc/>
    public override string ToString() => VendorGuid.ToString();
}
