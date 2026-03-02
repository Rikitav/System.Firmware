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

using System;

namespace System.Firmware.BootService.DevicePathProtocols;

/// <summary>
/// Attribute describing the wrapper class for the DevicePath protocol
/// </summary>
/// <param name="type"></param>
/// <param name="subType"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DefineDevicePathProtocolAttribute(DeviceProtocolType type, byte subType) : Attribute
{
    /// <summary>
    /// Type of protocol to be wrapped
    /// </summary>
    public DeviceProtocolType Type { get; private set; } = type;

    /// <summary>
    /// SubType of protocol to be wrapped
    /// </summary>
    public byte SubType { get; private set; } = subType;
}
