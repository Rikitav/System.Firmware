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
using System.IO;

namespace System.Firmware.MediaDevicePathProtocols;

/// <summary>
/// File Path Media Device Path
/// https://uefi.org/specs/UEFI/2.10/10_Protocols_Device_Path_Protocol.html#file-path-media-device-path
/// </summary>
[DefineDevicePathProtocol(DeviceProtocolType.Media, 4)]
public sealed class FilePathMediaDevicePath() : DevicePathProtocolBase(DeviceProtocolType.Media, 4)
{
    /// <summary>
    /// A NULL-terminated Path string including directory and file name.
    /// </summary>
    public string PathName { get; set; } = string.Empty;

    /// <summary>
    /// Create new <see cref="FilePathMediaDevicePath"/> protocol instance from file path
    /// </summary>
    public FilePathMediaDevicePath(string pathName) : this()
        => PathName = pathName;

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength()
        => PathName.GetCstyleWideStringLength();

    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
        => PathName = reader.ReadCstyleWideString();

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
        => writer.WriteCstyleWideString(PathName);

    /// <inheritdoc/>
    public override string ToString()
        => PathName;
}
