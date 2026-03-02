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
using System.ComponentModel;

namespace System.Firmware.BootService
{
    /// <summary>
    /// Invalid descriptor value
    /// </summary>
    public class InvalidHandleValueException : Win32Exception
    {
        /// <inheritdoc/>
        public InvalidHandleValueException(int lastError)
            : base(lastError) { }
    }

    /// <summary>
    /// Failed to find free loadOption index
    /// </summary>
    public class FreeLoadOptionIndexNotFound : Exception
    {
        /// <inheritdoc/>
        public FreeLoadOptionIndexNotFound(string Message)
            : base(Message) { }
    }

    /// <summary>
    /// The registering type has the wrong parent
    /// </summary>
    public class InvalidInheritedClassException : Exception
    {
        /// <inheritdoc/>
        public InvalidInheritedClassException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Failed to cast DevicePathProtocol
    /// </summary>
    public class DeviceProtocolCastingException : Exception
    {
        /// <inheritdoc/>
        public DeviceProtocolCastingException(string message)
            : base(message) { }
    }

    /// <summary>
    /// Load option has incorrect structure
    /// </summary>
    public class InvalidLoadOptionStrcutreException : Exception
    {
        /// <inheritdoc/>
        public InvalidLoadOptionStrcutreException(string message)
            : base(message) { }
    }

    /// <summary>
    /// The wrapper class does not contain a defining attribute
    /// </summary>
    public class MissingDevicePathProtocolWrapperAttributeException : Exception
    {
        /// <inheritdoc/>
        public MissingDevicePathProtocolWrapperAttributeException(string message)
            : base(message) { }
    }

    /// <summary>
    /// The wrapper class has the wrong structure
    /// </summary>
    public class InvalidDevicePathProtocolStructureException : Exception
    {
        /// <inheritdoc/>
        public InvalidDevicePathProtocolStructureException(string message)
            : base(message) { }
    }
}
