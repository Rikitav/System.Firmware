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

namespace System.Firmware.BootService.LoadOption;

/// <summary>
/// Basic implementation of <see cref="LoadOptionBase"/>
/// </summary>
public class FirmwareBootOption : LoadOptionBase
{
    /// <summary>
    /// Initializes a new instance of the FirmwareBootOption class with default attribute and description values.
    /// </summary>
    /// <remarks>
    /// The Attributes property is set to LoadOptionAttributes.CATEGORY_BOOT, and the Description property is initialized as an empty string.
    /// This constructor is typically used when creating a boot option with default settings before customizing its properties.
    /// </remarks>
    public FirmwareBootOption()
    {
        Attributes = LoadOptionAttributes.CATEGORY_BOOT;
        Description = string.Empty;
    }

    /// <summary>
    /// Create new <see cref="FirmwareBootOption"/> load option instance from <see cref="LoadOptionAttributes"/> and Option description
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="description"></param>
    public FirmwareBootOption(LoadOptionAttributes attributes, string description)
    {
        Attributes = attributes;
        Description = description;
    }

    /// <summary>
    /// Create new <see cref="FirmwareBootOption"/> load option instance from <see cref="LoadOptionAttributes"/>, Option description, <see cref="LoadOptionBase.OptionalData"/> and <see cref="DevicePathProtocolBase"/>s
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="description"></param>
    /// <param name="optionalData"></param>
    /// <param name="protocols"></param>
    public FirmwareBootOption(LoadOptionAttributes attributes, string description, DevicePathProtocolBase[] protocols, byte[] optionalData) : this(attributes, description)
    {
        Protocols = protocols;
        OptionalData = optionalData;
    }
}
