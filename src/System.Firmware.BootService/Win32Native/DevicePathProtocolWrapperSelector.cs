using System.Firmware.BootService.DevicePathProtocols;
using System.Firmware.BootService.UefiNative;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace System.Firmware.BootService.Win32Native;

/// <summary>
/// An assistant class that tells the marshaler what type to use to represent the native <see cref="EFI_DEVICE_PATH_PROTOCOL"/> structure
/// </summary>
public static class DevicePathProtocolWrapperSelector
{
    private static readonly Dictionary<short, Type> RegisteredWrapperTypes = new Dictionary<short, Type>()
    {
        { GetWrapperIdentificator(DeviceProtocolType.End, 0xFF), typeof(DevicePathProtocolEnd) }
    };

    private static readonly string[] KnownWrapperLibraries = [
        "System.Firmware.HardwareDevicePathProtocols",
        "System.Firmware.AcpiDevicePathProtocols",
        "System.Firmware.MessagingDevicePathProtocols",
        "System.Firmware.MediaDevicePathProtocols",
        "System.Firmware.BiosBootSpecificationDevicePathProtocols"
    ];

    public static bool DisallowCastingUnknownWrappersToRawProtocol { get; set; } = false;

    static DevicePathProtocolWrapperSelector()
    {
        EnumerateAssemblies();
    }

    private static void EnumerateAssemblies()
    {
        foreach (string assemblyName in KnownWrapperLibraries)
        {
            try
            {
                Assembly assembly = Assembly.Load(assemblyName);
                Type listType = assembly.GetType("System.Firmware.BootService.DevicePathProtocolTypeList");

                if (listType == null)
                    continue;

                RegisterTypesFromGeneratedList(listType);
            }
            catch
            {
                // Library doesnt exist in domain
            }
        }
    }

    private static void RegisterTypesFromGeneratedList(Type listType)
    {
        foreach (FieldInfo field in listType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(Type))
                continue;

            Type wrapperType = (Type)field.GetValue(null)!;
            RegisterType(wrapperType);
        }
    }

    /// <summary>
    /// Retrieves the registered protocol wrapper type associated with the specified protocol type and subtype.
    /// </summary>
    /// <remarks>
    /// This method returns only types that have been previously registered using RegisterWrapperLibrary.
    /// If no matching type is found, the method returns null.
    /// </remarks>
    /// <param name="type">The protocol type for which to retrieve the registered wrapper type.</param>
    /// <param name="subType">The protocol subtype used to further identify the wrapper type. Typically represents a protocol variant or
    /// extension.</param>
    /// <returns>The registered wrapper type corresponding to the specified protocol type and subtype, or null if no matching type is registered.</returns>
    public static Type GetRegisteredType(DeviceProtocolType type, byte subType)
    {
        short wrapperIdentificator = GetWrapperIdentificator(type, subType);
        if (RegisteredWrapperTypes.TryGetValue(wrapperIdentificator, out Type? registered))
            return registered;

        if (!DisallowCastingUnknownWrappersToRawProtocol)
            return typeof(RawMediaDevicePath);

        throw new DeviceProtocolCastingException($"Failed to cast DevicePathProtocol of type {type} and subType {subType} to managed object. No wrapper found for this protocol.");
    }

    /// <summary>
    /// Registers a protocol wrapper type for later use in marshaling boot options
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterType<T>() where T : DevicePathProtocolBase
    {
        RegisterType(typeof(T));
    }

    /// <summary>
    /// Registers a protocol wrapper type for later use in marshaling boot options
    /// </summary>
    /// <param name="wrapperType"></param>
    /// <exception cref="InvalidInheritedClassException"></exception>
    /// <exception cref="MissingDevicePathProtocolWrapperAttributeException"></exception>
    public static void RegisterType(Type wrapperType)
    {
        if (!wrapperType.IsSubclassOf(typeof(DevicePathProtocolBase)))
            throw new InvalidInheritedClassException("The type of protocol wrapper must be inherited from class DevicePathProtocolBase");

        DefineDevicePathProtocolAttribute defineWrapperAttr = wrapperType.GetCustomAttribute<DefineDevicePathProtocolAttribute>();
        if (defineWrapperAttr == null)
            throw new MissingDevicePathProtocolWrapperAttributeException("The protocol wrapper does not have the required \'System.Firmware.BootService.DevicePathProtocols.DefineDevicePathProtocolAttribute\'");

        short wrapperIdentificator = GetWrapperIdentificator(defineWrapperAttr.Type, defineWrapperAttr.SubType);
        RegisteredWrapperTypes.Add(wrapperIdentificator, wrapperType);
    }

    private static short GetWrapperIdentificator(DeviceProtocolType type, byte subType)
        => (short)((byte)type | (subType << sizeof(byte)));
}
