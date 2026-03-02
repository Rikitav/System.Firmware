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

namespace System.Firmware.MediaDevicePathProtocols;

/// <summary>
/// https://uefi.org/specs/UEFI/2.10/10_Protocols_Device_Path_Protocol.html#media-device-path
/// </summary>
public enum MediaDeviceProtocolType : byte
{
    /// <summary>
    /// The Hard Drive Media Device Path is used to represent a partition on a hard drive. Each partition has at least Hard Drive Device Path node, each describing an entry in a partition table. EFI supports MBR and GPT partitioning formats. Partitions are numbered according to their entry in their respective partition table, starting with 1. Partitions are addressed in EFI starting at LBA zero. A partition number of zero can be used to represent the raw hard drive or a raw extended partition
    /// </summary>
    HardDrive = 1,

    /// <summary>
    /// The CD-ROM Media Device Path is used to define a system partition that exists on a CD-ROM. The CD-ROM is assumed to contain an ISO-9660 file system and follow the CD-ROM “El Torito” format. The Boot Entry number from the Boot Catalog is how the “El Torito” specification defines the existence of bootable entities on a CD-ROM. In EFI the bootable entity is an EFI System Partition that is pointed to by the Boot Entry.
    /// </summary>
    CdRom = 2,

    /// <summary>
    /// Vendor-Defined Media Device Path
    /// </summary>
    Vendor = 3,

    /// <summary>
    /// A NULL-terminated Path string including directory and file names. The length of this string n can be determined by subtracting 4 from the Length entry. A device path may contain one or more of these nodes. Each node can optionally add a “" separator to the beginning and/or the end of the Path Name string. The complete path to a file can be found by logically concatenating all the Path Name strings in the File Path Media Device Path nodes. This is typically used to describe the directory path in one node, and the filename in another node.
    /// </summary>
    FilePath = 4,

    /// <summary>
    /// The Media Protocol Device Path is used to denote the protocol that is being used in a device path at the location of the path specified. Many protocols are inherent to the style of device path.
    /// </summary>
    Media = 5,

    /// <summary>
    /// This type is used by systems implementing the UEFI PI Specification to describe a firmware file. The exact format and usage are defined in that specification.
    /// </summary>
    PiwgFirmwareFile = 6,

    /// <summary>
    /// This type is used by systems implementing the UEFI PI Specification to describe a firmware volume. The exact format and usage are defined in that specification.
    /// </summary>
    PiwgFirmwareVolume = 7,

    /// <summary>
    /// This device path node specifies a range of offsets relative to the first byte available on the device. The starting offset is the first byte of the range and the ending offset is the last byte of the range (not the last byte + 1).
    /// </summary>
    RelativeOffsetRange = 8,

    /// <summary>
    /// 
    /// </summary>
    RamDisk = 9
}
