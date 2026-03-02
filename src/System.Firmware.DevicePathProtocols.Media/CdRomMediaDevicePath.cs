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
using System.IO;

namespace System.Firmware.MediaDevicePathProtocols;

/// <summary>
/// The CD-ROM Media Device Path is used to define a system partition that exists on a CD-ROM. The CD-ROM is assumed to contain an ISO-9660 file system and follow the CD-ROM “El Torito” format
/// https://uefi.org/specs/UEFI/2.10/10_Protocols_Device_Path_Protocol.html#cd-rom-media-device-path
/// </summary>
[DefineDevicePathProtocol(DeviceProtocolType.Media, 2)]
public sealed class CdRomMediaDevicePath() : DevicePathProtocolBase(DeviceProtocolType.Media, 2)
{
    /// <summary>
    /// Boot Entry number from the Boot Catalog. The Initial/Default entry is defined as zero.
    /// </summary>
    public uint BootEntry { get; set; }

    /// <summary>
    /// Starting RBA of the partition on the medium. CD-ROMs use Relative logical Block Addressing.
    /// </summary>
    public ulong PartitionStart { get; set; }

    /// <summary>
    /// Size of the partition in units of Blocks, also called Sectors.
    /// </summary>
    public ulong PartitionSize { get; set; }

    /// <summary>
    /// Create new <see cref="CdRomMediaDevicePath"/> protocol instance from boot entry and partition offset
    /// </summary>
    public CdRomMediaDevicePath(uint bootEntry, ulong partitionStart, ulong partitionSize) : this()
    {
        BootEntry = bootEntry;
        PartitionStart = partitionStart;
        PartitionSize = partitionSize;
    }

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength()
    {
        // BootEntry (4 bytes) + PartitionStart (8 bytes) + PartitionSize (8 bytes)
        return sizeof(uint) + sizeof(ulong) + sizeof(ulong);
    }

    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
    {
        BootEntry = reader.ReadUInt32();
        PartitionStart = reader.ReadUInt64() * 512;
        PartitionSize = reader.ReadUInt64() * 512;
    }

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(BootEntry);
        writer.Write(PartitionStart / 512);
        writer.Write(PartitionSize / 512);
    }

    /// <inheritdoc/>
    public override string ToString() => BootEntry.ToString();
}
