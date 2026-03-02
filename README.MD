# ExtensibleFirmware
![Markdown](https://github.com/Rikitav/System.Firmware/blob/main/form/banner.png)

## Description
ExtensibleFirmware is a .NET class library for working with UEFI firmware. It provides tools for editing the boot process, allowing developers to modify and configure boot parameters for enhanced flexibility.

## Features
- Getting EFI system partition
- Editing and creating boot entries.
- Editing global variables for firmware.

## Installation
```bash
# Install via NuGet Package manager
Install-Package System.Firmware

# Install via dotnet CLI
dotnet add package System.Firmware 
```

## Usage Examples

### Example 1: Getting ESP
```csharp
using System.Firmware.SystemPartition;
using System.IO;

namespace Examples
{
    class Program
    {
        public static void Main()
        {
            // Getting 'EFI system partition' volume path
            string EspVolumePath = EfiPartition.GetFullPath();

            // Example reading config file from ESP using volume path, instead of using MountVol
            string configText = File.ReadAllText(Path.Combine(EspVolumePath, "EFI", "Ubuntu", "grub.cfg"));

            // Dumping result
            Console.WriteLine(configText);
        }
    }
}
```

### Example 2: Reading boot option
```csharp
using System.Firmware.BootService;
using System.Firmware.BootService.DevicePathProtocols;
using System.Firmware.BootService.LoadOption;
using System;

namespace Examples
{
    class Program
    {
        public static void Main()
        {
            // Reading boot option
            FirmwareBootOption bootOption = FirmwareBootService.ReadLoadOption(0x0003); // <-- Set here your variable index
            
            // Showing basic information
            Console.WriteLine("Option name : \"{0}\"", bootOption.Description);
            Console.WriteLine("Attributes  : {0}", bootOption.Attributes);
            
            // Enumerating all protocols
            foreach (DevicePathProtocolBase protocol in bootOption.OptionProtocols)
                Console.WriteLine(protocol.ToString()); // Getting string representation of protocol
        }
    }
}
```

### Example 3: Enumearating boot options
```csharp
using System.Firmware.BootService;
using System.Firmware.BootService.DevicePathProtocols;
using System.Firmware.BootService.LoadOption;

namespace System.Firmware.TestsApplication
{
    public static class BootManagerTests
    {
        public static void RunTests()
        {
            // Enumerating all boot options in boot order
            int index = 0;
            foreach (FirmwareBootOption bootOption in FirmwareBootService.EnumerateBootOptions())
            {
                // Showing basic information
                Console.WriteLine("\n===[ Boot option #" + index++ + " ]" + new string('=', 60));
                Console.WriteLine("Option name : \"{0}\"", bootOption.Description);
               Console.WriteLine("Attributes  : {0}", bootOption.Attributes);

                // Enumerating all protocols
                foreach (DevicePathProtocolBase protocol in bootOption.OptionProtocols)
                    Console.WriteLine(protocol.ToString()); // Getting string representation of protocol
            }
        }
    }
}
```

### Example 4: Creating boot option
```csharp
using System.Firmware.BootService;
using System.Firmware.BootService.DevicePathProtocols;
using System.Firmware.BootService.LoadOption;
using System.Firmware.MediaDevicePathProtocols;
using System;

namespace Examples
{
    class Program
    {
        public static void Main()
        {
            // Setting device path protocols
            // This protocols instructions boot service how to load your option
            DevicePathProtocolBase[] protocols = new DevicePathProtocolBase[]
            {
                new HardDriveMediaDevicePath(new Guid("XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX")), // The partition on which the bootloader is located
                new FilePathMediaDevicePath("EFI\\MyApplication\\bootx64.efi") // Path to the EFI application to be loaded
            };

            // Creating simple load option
            FirmwareBootOption bootOption = new FirmwareBootOption(LoadOptionAttributes.ACTIVE, "MyLoader", Array.Empty<byte>(), protocols);

            // Creating new load option
            ushort newLoadOptionIndex = FirmwareBootService.CreateLoadOption(bootOption, true);

            // Logging new boot option index
            Console.WriteLine("Boot option sucessfully created, new option index : {0}", newLoadOptionIndex);
        }
    }
}
```

## Requirements
Most functionality will work on every Windows system that supports uefi, except EfiExecutableInfo and VolumenName pathes, the will only work on Windows 8 or above

## Resources
If you want to learn more about UEFI and how this library works, read the documentation at the following links :
- [Boot manager](https://uefi.org/specs/UEFI/2.9_A/03_Boot_Manager.html)
- [Device path protocols](https://uefi.org/specs/UEFI/2.9_A/10_Protocols_Device_Path_Protocol.html)
- [Load options](https://uefi.org/specs/UEFI/2.9_A/03_Boot_Manager.html#load-options)
- [Global environment variables](https://uefi.org/specs/UEFI/2.9_A/03_Boot_Manager.html#globally-defined-variables)

## License
This project is licensed under the GNU GPL License. See [LICENSE](LICENSE) for details.
