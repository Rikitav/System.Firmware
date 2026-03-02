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
/// Partition format
/// </summary>
public enum PartitionFormat : byte
{
    /// <summary>
    /// Mastre boot record
    /// </summary>
    LegacyMBR = 0x01,

    /// <summary>
    /// Guid partition table
    /// </summary>
    GuidPartitionTable = 0x02
}

/// <summary>
/// Signature type
/// </summary>
public enum SignatureType : byte
{
    /// <summary>
    /// Raw
    /// </summary>
    NoDiskSignature = 0x00,

    /// <summary>
    /// Mastre boot record
    /// </summary>
    MbrSignature = 0x01,

    /// <summary>
    /// Guid partition table
    /// </summary>
    GuidSignature = 0x02
}

/// <summary>
/// The Hard Drive Media Device Path is used to represent a partition on a hard drive.
/// <see href="https://uefi.org/specs/UEFI/2.9_A/10_Protocols_Device_Path_Protocol.html#hard-drive-media-device-path"/>
/// </summary>
[DefineDevicePathProtocol(DeviceProtocolType.Media, 1)]
public class HardDriveMediaDevicePath() : DevicePathProtocolBase(DeviceProtocolType.Media, 1)
{
    /// <summary>
    /// Describes the entry in a partition table, starting with entry 1. Partition number zero represents the entire device. Valid partition numbers for a MBR partition are [1, 4]. Valid partition numbers for a GPT partition are [1, NumberOfPar titionEntries].
    /// </summary>
    public uint PartitionNumber { get; set; }

    /// <summary>
    /// Starting LBA of the partition on the hard drive
    /// </summary>
    public ulong PartitionStart { get; set; }

    /// <summary>
    /// Size of the partition in units of Logical Blocks
    /// </summary>
    public ulong PartitionSize { get; set; }

    /// <summary>
    /// Signature unique to this partition, this field contains a 16 byte signature.
    /// </summary>
    public Guid GptPartitionSignature { get; set; }

    /// <summary>
    /// Partition Format: (Unused values reserved) 0x01 - PC-AT compatible legacy MBR (Legacy MBR) . Partition Start and Partition Size come from Parti tionStartingLBA and PartitionSizeInLBA for the partition.0x02—GUID Partition Table
    /// </summary>
    public PartitionFormat PartitionFormat { get; set; }

    /// <summary>
    /// Type of Disk Signature: (Unused values reserved) 0x00 - No Disk Signature. 0x01 - 32-bit signature from address 0x1b8 of the type 0x01 MBR. 0x02 - GUID signature.
    /// </summary>
    public SignatureType SignatureType { get; set; }

    /// <summary>
    /// Create new <see cref="HardDriveMediaDevicePath"/> protocol instance from <see cref="PARTITION_INFORMATION_EX"/> structure
    /// </summary>
    /// <param name="partition"></param>
    /// <exception cref="InvalidDataException"></exception>
    public HardDriveMediaDevicePath(PARTITION_INFORMATION_EX partition) : this()
    {
        if (partition.PartitionStyle != PartitionStyle.GuidPartitionTable)
            throw new InvalidDataException("Partition information describe MBR pr RAW based partition");

        Deserialize(partition);
    }

    /// <summary>
    /// Create a protocol from unique partition <see cref="Guid"/>
    /// </summary>
    /// <param name="partitionIdentificator"></param>
    /// <exception cref="ArgumentException"></exception>
    public HardDriveMediaDevicePath(Guid partitionIdentificator) : this()
    {
        if (partitionIdentificator == Guid.Empty)
            throw new ArgumentException("Partition identificator has empty value");

        PARTITION_INFORMATION_EX partition = IoctlVolumeInformation.GetPartition(partitionIdentificator);
        Deserialize(partition);
    }

    /// <inheritdoc/>
    public override ushort GetSerializationDataLength()
    {
        // PartitionNumber (4 bytes) + PartitionStart (8 bytes) + PartitionSize (8 bytes) + GptPartitionSignature (GUID) + PartitionFormat (1 byte) + SignatureType (1 byte)
        return sizeof(uint) + sizeof(ulong) + sizeof(ulong) + 16 + sizeof(byte) + sizeof(byte);
    }

    /// <inheritdoc/>
    public override void Deserialize(BinaryReader reader, ushort length)
    {
        PartitionNumber = reader.ReadUInt32();
        PartitionStart = reader.ReadUInt64() * 512;
        PartitionSize = reader.ReadUInt64() * 512;
        GptPartitionSignature = reader.ReadGuid();
        PartitionFormat = (PartitionFormat)reader.ReadByte();
        SignatureType = (SignatureType)reader.ReadByte();
    }

    /// <inheritdoc/>
    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(PartitionNumber);
        writer.Write(PartitionStart / 512);
        writer.Write(PartitionSize / 512);
        writer.Write(GptPartitionSignature.ToByteArray());
        writer.Write((byte)PartitionFormat);
        writer.Write((byte)SignatureType);
    }

    /// <inheritdoc/>
    public override string ToString()
        => GptPartitionSignature.ToString();

    private void Deserialize(PARTITION_INFORMATION_EX partInfo)
    {
        PartitionNumber = partInfo.PartitionNumber;
        PartitionStart = (ulong)partInfo.StartingOffset;
        PartitionSize = (ulong)partInfo.PartitionLength;
        GptPartitionSignature = partInfo.Gpt.PartitionId;
        PartitionFormat = PartitionFormat.GuidPartitionTable;
        SignatureType = SignatureType.GuidSignature;
    }
}
