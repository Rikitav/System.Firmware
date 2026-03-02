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
using System.Firmware.BootService.Win32Native;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Firmware.BootService;

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
        get => [.. FirmwareGlobalEnvironment.BootOrder];
        set => FirmwareGlobalEnvironment.BootOrder = [.. value.Select(x => x)];
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
        LoadOrder = [.. LoadOrder.Where(x => x != bootOptionIndex)];
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<FirmwareBootOption> EnumerateBootOptions()
    {
        return LoadOrder.Select(ReadLoadOption);
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<T> EnumrateBootOptions<T>() where T : LoadOptionBase, new()
    {
        return LoadOrder.Select(ReadLoadOption<T>);
    }

    /// <summary>
    /// Lists all load options in boot order
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<EFI_LOAD_OPTION> EnumrateRawBootOptions()
    {
        return LoadOrder.Select(ReadRawLoadOption);
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
        using BinaryReader reader = ReadFirmwareLoadOption(bootOptionIndex);
        return reader.ReadRawLoadOption();
    }

    /// <summary>
    /// Reads boot option from NVRAM
    /// </summary>
    /// <param name="bootOptionIndex"></param>
    /// <returns></returns>
    public static FirmwareBootOption ReadLoadOption(BootOptionIndex bootOptionIndex)
    {
        using BinaryReader reader = ReadFirmwareLoadOption(bootOptionIndex);
        return reader.ReadLoadOption<FirmwareBootOption>();
    }

    /// <summary>
    /// Reads a boot option from NVRAM and converts it to the specified type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bootOptionIndex"></param>
    /// <returns></returns>
    public static T ReadLoadOption<T>(BootOptionIndex bootOptionIndex) where T : LoadOptionBase, new()
    {
        using BinaryReader reader = ReadFirmwareLoadOption(bootOptionIndex);
        return reader.ReadLoadOption<T>();
    }

    private static BinaryReader ReadFirmwareLoadOption(BootOptionIndex loadOptionIndex)
    {
        // Getting variable data
        IntPtr pointer = FirmwareUtilities.GetGlobalEnvironmentVariable(loadOptionIndex, out int DataLength, 1024);
        return new BinaryReader(new MemoryPointerStream(pointer, DataLength, false));
    }

    private static void WriteFirmwareLoadOption(LoadOptionBase loadOption, BootOptionIndex bootOptionIndex)
    {
        // Marshalling structure to unmanaged memory pointer
        int structureLength = loadOption.GetStrcutureLength();
        using MemoryPointerStream pointer = new MemoryPointerStream(structureLength);

        // Serializing structure to memory
        using (BinaryWriter writer = new BinaryWriter(pointer, Encoding.Unicode, true))
            writer.WriteLoadOption(loadOption);

        // Writing variable to firmware
        FirmwareUtilities.SetGlobalEnvironmentVariable(bootOptionIndex, pointer.Buffer, structureLength);
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
