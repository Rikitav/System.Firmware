// Rikitav.IO.ExtensibleFirmware
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

using Rikitav.IO.ExtensibleFirmware.BootService.DevicePathProtocols;
using Rikitav.IO.ExtensibleFirmware.BootService.LoadOption;
using Rikitav.IO.ExtensibleFirmware.BootService.UefiNative;
using Rikitav.IO.ExtensibleFirmware.BootService.Win32Native;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Rikitav.IO.ExtensibleFirmware.BootService;

/// <summary>
/// Provides methods to read, write, update, and delete boot entries from your computer's UEFI NVRAM
/// </summary>
public static class FirmwareBootService
{
    /// <summary>
    /// Gets the index of the boot record this computer was booted from. Use this index in methods of this <see cref="FirmwareBootService"/>
    /// </summary>
    public static BootOptionIndex CurrentLoadOptionIndex
    {
        get => FirmwareGlobalEnvironment.BootCurrent;
    }

    /// <summary>
    /// Sets the index of the boot entry that will be loaded next time ONCE. Use this index in methods of this class
    /// </summary>
    public static BootOptionIndex NextLoadOptionIndex
    {
        set => FirmwareGlobalEnvironment.BootNext = value;
    }

    /// <summary>
    /// Gets or sets the boot order. The array contains the indexes of all boot entries in the order in which they will be loaded. Use this index in methods of this class
    /// </summary>
    public static BootOptionIndex[] LoadOrder
    {
        get => FirmwareGlobalEnvironment.BootOrder.Cast<BootOptionIndex>().ToArray();
        set => FirmwareGlobalEnvironment.BootOrder = value.Cast<ushort>().ToArray();
    }

    /// <summary>
    /// Resets the Boot#### variable at the specified index and removes it from the boot order
    /// </summary>
    /// <param name="bootOptionIndex"></param>
    public static void DeleteLoadOption(BootOptionIndex bootOptionIndex)
    {
        // Writing null variable to firmware
        FirmwareUtilities.SetGlobalEnvironmentVariable(bootOptionIndex, IntPtr.Zero, 0);

        // Removing index from boot order
        //LoadOrder = LoadOrder.Where(x => x != bootOptionIndex).ToArray();
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<FirmwareBootOption> EnumerateBootOptions()
    {
        return LoadOrder.Select(i => ReadLoadOption(i));
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<T> EnumrateBootOptions<T>() where T : LoadOptionBase, new()
    {
        return LoadOrder.Select(i => ReadLoadOption<T>(i));
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<EFI_LOAD_OPTION> EnumrateRawBootOptions()
    {
        return LoadOrder.Select(i => ReadRawLoadOption(i));
    }

    /// <summary>
    /// Reads the boot option from NVRAM, issues it to the delegate for processing and updates the variable at the specified index
    /// </summary>
    /// <param name="bootOptionIndex"></param>
    /// <param name="UpdateOptionAction"></param>
    public static void EditLoadOption(BootOptionIndex bootOptionIndex, Action<FirmwareBootOption> UpdateOptionAction)
    {
        FirmwareBootOption bootOption = ReadLoadOption(bootOptionIndex);
        UpdateOptionAction(bootOption);
        WriteFirmwareLoadOption(bootOption, bootOptionIndex);
    }

    /// <summary>
    /// Creates a new load option at the first free index, writes into it a serialized copy of the <see cref="LoadOptionBase"/> instance passed to the function, and returns the index of the new entry. Specify the <paramref name="AddFirst"/> parameter as <see langword="true"/> to add the option as the first boot option
    /// </summary>
    /// <param name="loadOption"></param>
    /// <param name="AddFirst"></param>
    /// <returns></returns>
    /// <exception cref="FreeLoadOptionIndexNotFound"></exception>
    public static BootOptionIndex CreateLoadOption(LoadOptionBase loadOption, bool AddFirst)
    {
        // Getting free variable name
        BootOptionIndex? freeLoadOptionIndex = GetFirstFreeLoadOptionName();
        if (!freeLoadOptionIndex.HasValue)
            throw new FreeLoadOptionIndexNotFound("Failed to find free loadOption name");

        // Creating variable
        BootOptionIndex newLoadOptionIndex = freeLoadOptionIndex.Value;
        WriteFirmwareLoadOption(loadOption, newLoadOptionIndex);

        // Setting new boot order
        FirmwareGlobalEnvironment.BootOrder = AddFirst
            ? [newLoadOptionIndex, ..FirmwareGlobalEnvironment.BootOrder]
            : [..FirmwareGlobalEnvironment.BootOrder, newLoadOptionIndex];

        return newLoadOptionIndex;
    }

    /// <summary>
    /// Writes serialized copy of the <see cref="LoadOptionBase"/> instance passed to the function into existing load option variable at the specified index
    /// </summary>
    /// <param name="loadOption"></param>
    /// <param name="bootOptionIndex"></param>
    public static void UpdateLoadOption(LoadOptionBase loadOption, BootOptionIndex bootOptionIndex)
    {
        // Updating variable
        WriteFirmwareLoadOption(loadOption, bootOptionIndex);
    }

    /// <summary>
    /// Reads the native representation of the boot option from NVRAM
    /// </summary>
    /// <param name="bootOptionIndex"></param>
    /// <returns></returns>
    public static EFI_LOAD_OPTION ReadRawLoadOption(BootOptionIndex bootOptionIndex)
    {
        return ReadFirmwareLoadOption(bootOptionIndex);
    }

    /// <summary>
    /// Reads boot option from NVRAM
    /// </summary>
    /// <param name="bootOptionIndex"></param>
    /// <returns></returns>
    public static FirmwareBootOption ReadLoadOption(BootOptionIndex bootOptionIndex)
    {
        EFI_LOAD_OPTION loadOption = ReadFirmwareLoadOption(bootOptionIndex);
        return new FirmwareBootOption(loadOption);
    }

    /// <summary>
    /// Reads a boot option from NVRAM and converts it to the specified type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bootOptionIndex"></param>
    /// <returns></returns>
    public static T ReadLoadOption<T>(BootOptionIndex bootOptionIndex) where T : LoadOptionBase, new()
    {
        EFI_LOAD_OPTION loadOption = ReadFirmwareLoadOption(bootOptionIndex);
        return (T)Activator.CreateInstance(typeof(T), loadOption);
    }

    private static EFI_LOAD_OPTION ReadFirmwareLoadOption(BootOptionIndex loadOptionIndex)
    {
        // Getting variable data
        IntPtr pointer = FirmwareUtilities.GetGlobalEnvironmentVariable(loadOptionIndex, out int DataLength, 1024);
        using BinaryReader reader = new BinaryReader(new MemoryPointerStream(pointer, DataLength, true));

        // Marshalling variable
        EFI_LOAD_OPTION loadOption = LoadOptionMarshaller.BinaryReaderToStructure(reader);
        Marshal.FreeHGlobal(pointer);
        return loadOption;
    }

    private static void WriteFirmwareLoadOption(LoadOptionBase loadOption, BootOptionIndex bootOptionIndex)
    {
        /*
        // Checking for End protocol
        if (!(loadOption.OptionProtocols.Last() is DevicePathProtocolEnd))
            throw new InvalidLoadOptionStrcutreException("Load option does not have DevicePathProtocolEnd at the end of the OptionProtocols list. This protocol is necessary for correct recording");
        */

        // Marshalling structure to unmanaged memory pointer
        int structureLength = LoadOptionMarshaller.GetStrcutureLength(loadOption);
        IntPtr pointer = Marshal.AllocHGlobal(structureLength);
        
        using (BinaryWriter writer = new BinaryWriter(new MemoryPointerStream(pointer, structureLength, true), Encoding.Unicode, true))
            LoadOptionMarshaller.MarshalToBinaryWriter(loadOption, writer);
#if DEBUG
        byte[] DebugData = new byte[structureLength];
        Marshal.Copy(pointer, DebugData, 0, structureLength);
#endif
        // Writing variable to firmware
        FirmwareUtilities.SetGlobalEnvironmentVariable(bootOptionIndex, pointer, structureLength);
        Marshal.FreeHGlobal(pointer);
    }

    private static BootOptionIndex? GetFirstFreeLoadOptionName()
    {
        for (ushort i = 0; i < 256; i++)
        {
            if (!LoadOrder.Contains(i))
                return i;
        }

        return null;
    }
}
