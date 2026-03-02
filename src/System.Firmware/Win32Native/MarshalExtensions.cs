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
using System.IO;
using System.Text;

namespace System.Firmware.Win32Native;

public static partial class MarshalExtensions
{
    public static unsafe T[] PinnedMemoryToArray<T>(this IntPtr memory, int length) where T : struct
    {
        ReadOnlySpan<T> span = new ReadOnlySpan<T>(memory.ToPointer(), length);
        return span.ToArray();
    }

    public static BinaryWriter WriteCstyleWideString(this BinaryWriter writer, string value)
    {
        if (value == null)
            return writer;

        byte[] bytes = Encoding.Unicode.GetBytes(value);
        byte[] terminator = Encoding.Unicode.GetBytes("\0");

        writer.Write(bytes);
        writer.Write(terminator);

        return writer;
    }

    public static string ReadCstyleWideString(this BinaryReader reader)
    {
        StringBuilder builder = new StringBuilder();
        for (ushort chr = reader.ReadUInt16(); chr != 0; chr = reader.ReadUInt16())
            builder.Append((char)chr);

        return builder.ToString();
    }

    public static ushort GetCstyleWideStringLength(this string value)
    {
        if (value == null)
            return 0;
        
        return (ushort)((value.Length + 1) * sizeof(ushort));
    }

    public static byte[] ReadRemainingBytes(this BinaryReader reader)
    {
        int remainingBytesLength = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
        return reader.ReadBytes(remainingBytesLength);
    }

    public static BinaryWriter WriteGuid(this BinaryWriter writer, Guid guid)
    {
        writer.Write(guid.ToByteArray());
        return writer;
    }

    public static Guid ReadGuid(this BinaryReader reader)
    {
        byte[] guidBytes = reader.ReadBytes(16);
        return new Guid(guidBytes);
    }
}
