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

using System;

namespace Rikitav.IO.ExtensibleFirmware.Win32Native;

public static partial class MarshalExtensions
{
    public static unsafe T[] PinnedMemoryToArray<T>(this IntPtr memory, int length) where T : struct
    {
        ReadOnlySpan<T> span = new ReadOnlySpan<T>(memory.ToPointer(), length);
        return span.ToArray();
    }
}
