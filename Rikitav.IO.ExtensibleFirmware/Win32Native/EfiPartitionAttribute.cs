using System;

namespace Rikitav.IO.ExtensibleFirmware.Win32Native;

/// <summary>
/// The Extensible Firmware Interface (EFI) attributes of the partition.
/// </summary>
[Flags]
public enum EfiPartitionAttribute : ulong
{
    /// <summary>
    /// If this attribute is set, the partition is required by a computer to function properly. 
    /// </summary>
    GPT_ATTRIBUTE_PLATFORM_REQUIRED = 0x0000000000000001,

    /// <summary>
    /// If this attribute is set, the partition does not receive a drive letter by default when the disk is moved to another computer or when the disk is seen for the first time by a computer. 
    /// </summary>
    GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER = 0x8000000000000000,

    /// <summary>
    /// If this attribute is set, the partition is not detected by the Mount Manager. 
    /// </summary>
    GPT_BASIC_DATA_ATTRIBUTE_HIDDEN = 0x4000000000000000,

    /// <summary>
    /// If this attribute is set, the partition is a shadow copy of another partition.
    /// </summary>
    GPT_BASIC_DATA_ATTRIBUTE_SHADOW_COPY = 0x2000000000000000,

    /// <summary>
    /// If this attribute is set, the partition is read-only. 
    /// </summary>
    GPT_BASIC_DATA_ATTRIBUTE_READ_ONLY = 0x1000000000000000
}
