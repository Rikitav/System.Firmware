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
using System.Firmware.BootService.LoadOption;
using System.Firmware.BootService.UefiNative;
using System.Firmware.Win32Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Firmware.BootService.Win32Native;

public static partial class MarshalExtensions
{
    private static readonly int DevicePathProtocolHeaderLength = sizeof(byte) + sizeof(byte) + sizeof(ushort); // Type + SubType + DataLength
    private static readonly int LoadOptionBaseHeaderLength = sizeof(LoadOptionAttributes) + sizeof(ushort); // Attributes + FilePathListLength
    
    public static EFI_LOAD_OPTION ReadRawLoadOption(this BinaryReader reader)
    {
        // Starting manual marshalling strcture to managed
        EFI_LOAD_OPTION loadOption = new EFI_LOAD_OPTION();

        // Reading general data
        loadOption.Attributes = (LoadOptionAttributes)reader.ReadUInt32();  // Reading Attributes field
        loadOption.FilePathListLength = reader.ReadUInt16();                // Reading length of filepath list
        loadOption.Description = reader.ReadCstyleWideString();             // Reading Description (Load option name)
        loadOption.FilePathList = reader.ReadUntilRawEndProtocol();         // Reading Device path list

        // Manually seek to optional data position because EFI_DEVICE_PATH_PROTOCOL sequence not always property aligned
        int SeekLength = LoadOptionBaseHeaderLength + loadOption.Description.GetCstyleWideStringLength() + loadOption.FilePathListLength;
        reader.BaseStream.Seek(SeekLength, SeekOrigin.Begin);

        // Reading OptionalData field
        loadOption.OptionalData = reader.ReadRemainingBytes();
        return loadOption;
    }

    public static EFI_DEVICE_PATH_PROTOCOL ReadRawDevicePathProtocol(this BinaryReader reader)
    {
        // Starting manual marshalling strcture to managed
        EFI_DEVICE_PATH_PROTOCOL protocol = new EFI_DEVICE_PATH_PROTOCOL();

        // Reading general data
        protocol.Type = (DeviceProtocolType)reader.ReadByte();                                  // Reading device type
        protocol.SubType = reader.ReadByte();                                                   // Reading device subType
        protocol.Length = reader.ReadUInt16();                                                  // Reading structure length
        protocol.Data = reader.ReadBytes(protocol.Length - DevicePathProtocolHeaderLength);     // Reading protocol data

        // Done
        return protocol;
    }

    public static FirmwareBootOption ReadLoadOption(this BinaryReader reader)
    {
        return (FirmwareBootOption)reader.ReadLoadOption(typeof(FirmwareBootOption));
    }

    public static LoadOptionBase ReadLoadOption(this BinaryReader reader, Type loadOptionType)
    {
        // Starting manual marshalling strcture to managed
        LoadOptionBase loadOption = (LoadOptionBase)Activator.CreateInstance(loadOptionType);

        // Reading general data
        loadOption.Attributes = (LoadOptionAttributes)reader.ReadUInt32();  // Reading Attributes field
        ushort devicePathListLength = reader.ReadUInt16();                  // Reading length of filepath list
        loadOption.Description = reader.ReadCstyleWideString();             // Reading Description (Load option name)
        loadOption.Protocols = reader.ReadUntilEndProtocol();               // Reading Device path list

        // Manually seek to optional data position because EFI_DEVICE_PATH_PROTOCOL sequence not always property aligned
        int SeekLength = LoadOptionBaseHeaderLength + loadOption.Description.GetCstyleWideStringLength() + devicePathListLength;
        reader.BaseStream.Seek(SeekLength, SeekOrigin.Begin);

        // Reading OptionalData field
        loadOption.OptionalData = reader.ReadRemainingBytes();
        
        return loadOption;
    }

    public static DevicePathProtocolBase ReadDevicePathProtocol(this BinaryReader reader)
    {
        // Starting manual marshalling strcture to managed
        DeviceProtocolType type = (DeviceProtocolType)reader.ReadByte();    // Reading device type
        byte subType = reader.ReadByte();                                   // Reading device subType
        ushort length = reader.ReadUInt16();                                // Reading structure length

        // Getting protocol wrapper type and creating protocol wrapper and deserializing protocol data
        Type protocolWrapperType = DevicePathProtocolWrapperSelector.GetRegisteredType(type, subType);
        if (protocolWrapperType == typeof(RawMediaDevicePath))
        {
            RawMediaDevicePath rawMediaDevice = new RawMediaDevicePath(type, subType);
            rawMediaDevice.Deserialize(reader, (ushort)(length - DevicePathProtocolHeaderLength));
            return rawMediaDevice;
        }
        else
        {
            DevicePathProtocolBase protocol = (DevicePathProtocolBase)Activator.CreateInstance(protocolWrapperType);

            // Deserializing protocol data
            protocol.Deserialize(reader, length);

            // Validating protocol data length
            if (protocol.GetSerializationDataLength() + DevicePathProtocolHeaderLength != length)
                throw new InvalidDataException("");

            return protocol;
        }
    }

    public static DevicePathProtocolBase ReadDevicePathProtocol(this BinaryReader reader, Type devicePathType)
    {
        // Starting manual marshalling strcture to managed
        DeviceProtocolType type = (DeviceProtocolType)reader.ReadByte();    // Reading device type
        byte subType = reader.ReadByte();                                   // Reading device subType
        ushort length = reader.ReadUInt16();                                // Reading structure length

        // Creating protocol wrapper and deserializing protocol data
        DevicePathProtocolBase protocol = (DevicePathProtocolBase)Activator.CreateInstance(devicePathType);

        // Validating protocol wrapper type
        if (protocol.Type != type || protocol.SubType != subType)
            throw new DeviceProtocolCastingException($"Failed to cast DevicePathProtocol of type {type} and subType {subType} to managed object of type {devicePathType}. The wrapper has different Type or SubType.");

        // Deserializing protocol data
        protocol.Deserialize(reader, length);

        // Validating protocol data length
        if (protocol.GetSerializationDataLength() + DevicePathProtocolHeaderLength != length)
            throw new InvalidDataException("");

        return protocol;
    }

    public static T ReadLoadOption<T>(this BinaryReader reader) where T : LoadOptionBase, new()
    {
        return (T)reader.ReadLoadOption(typeof(T));
    }

    public static T ReadDevicePathProtocol<T>(this BinaryReader reader) where T : DevicePathProtocolBase, new()
    {
        return (T)reader.ReadDevicePathProtocol(typeof(T));
    }

    public static BinaryWriter WriteLoadOption(this BinaryWriter writer, EFI_LOAD_OPTION loadOption)
    {
        // Writing general data
        writer.Write((uint)loadOption.Attributes); // Writing attributes field
        writer.Write(loadOption.FilePathListLength); // Writing length of filepath list
        writer.WriteCstyleWideString(loadOption.Description); // Writing description (Load option name)

        // Writing filepath list
        foreach (EFI_DEVICE_PATH_PROTOCOL edpp in loadOption.FilePathList)
            writer.WriteDevicePathProtocol(edpp);

        // Writing optional data
        writer.Write(loadOption.OptionalData);

        // Flushing
        writer.Flush();

        // Done
        return writer;
    }

    public static BinaryWriter WriteDevicePathProtocol(this BinaryWriter writer, EFI_DEVICE_PATH_PROTOCOL protocol)
    {
        // Writing general data
        writer.Write((byte)protocol.Type);  // Writing device type
        writer.Write(protocol.SubType);     // Writing device subType
        writer.Write(protocol.Length);      // Writing structure length
        writer.Write(protocol.Data);        // Writing protocol data

        // Done
        return writer;
    }

    public static BinaryWriter WriteLoadOption<T>(this BinaryWriter writer, T loadOption) where T : LoadOptionBase
    {
        // Writing general data
        writer.Write((uint)loadOption.Attributes);                                                                              // Writing attributes field
        writer.Write((ushort)loadOption.Protocols.Sum(p => p.GetSerializationDataLength() + DevicePathProtocolHeaderLength));   // Writing length of filepath list
        writer.Write(loadOption.Description.GetCstyleWideStringLength());                                                       // Writing option description

        // Writing filepath list
        foreach (DevicePathProtocolBase edpp in loadOption.Protocols)
            writer.WriteDevicePathProtocol(edpp);

        if (loadOption.Protocols.Last() is not DevicePathProtocolEnd)
            writer.WriteDevicePathProtocol(new DevicePathProtocolEnd());

        // Writing optional data
        writer.Write(loadOption.OptionalData);

        // Done
        return writer;
    }

    public static BinaryWriter WriteDevicePathProtocol<T>(this BinaryWriter writer, T protocol) where T : DevicePathProtocolBase
    {
        // Writing general data
        writer.Write((byte)protocol.Type);
        writer.Write(protocol.SubType);
        writer.Write(protocol.GetSerializationDataLength() + DevicePathProtocolHeaderLength);

        // Serializing protocol data
        protocol.Serialize(writer);

        // Done
        return writer;
    }

    public static int GetStrcutureLength(this LoadOptionBase loadOption)
    {
        int structLength = LoadOptionBaseHeaderLength + loadOption.Description.GetCstyleWideStringLength();
        foreach (DevicePathProtocolBase devicePathProtocol in loadOption.Protocols)
            structLength += devicePathProtocol.GetStrctureLength();

        structLength += loadOption.OptionalData.Length;
        return structLength;
    }

    public static int GetStrctureLength(this DevicePathProtocolBase devicePathProtocol)
    {
        return DevicePathProtocolHeaderLength + devicePathProtocol.GetSerializationDataLength();
    }

    private static EFI_DEVICE_PATH_PROTOCOL[] ReadUntilRawEndProtocol(this BinaryReader reader)
    {
        List<EFI_DEVICE_PATH_PROTOCOL> filePathListBuilder = new List<EFI_DEVICE_PATH_PROTOCOL>();
        while (!filePathListBuilder.LastOrDefault().IsEndProtocol())
            filePathListBuilder.Add(reader.ReadRawDevicePathProtocol());

        return filePathListBuilder.ToArray();
    }

    private static DevicePathProtocolBase[] ReadUntilEndProtocol(this BinaryReader reader)
    {
        List<DevicePathProtocolBase> filePathListBuilder = new List<DevicePathProtocolBase>();
        while (true)
        {
            DevicePathProtocolBase protocol = reader.ReadDevicePathProtocol();
            if (protocol.IsEndProtocol())
                break;

            filePathListBuilder.Add(protocol);
        }

        return filePathListBuilder.ToArray();
    }

    private static bool IsEndProtocol(this EFI_DEVICE_PATH_PROTOCOL protocol)
    {
        return protocol.Type == DeviceProtocolType.End && protocol.SubType == 0xFF;
    }

    private static bool IsEndProtocol(this DevicePathProtocolBase? protocol)
    {
        if (protocol == null)
            return false;

        return protocol.Type == DeviceProtocolType.End && protocol.SubType == 0xFF;
    }
}
