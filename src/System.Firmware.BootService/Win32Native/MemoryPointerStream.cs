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
using System.Runtime.InteropServices;

namespace System.Firmware.BootService.Win32Native;

internal class MemoryPointerStream : Stream, IDisposable
{
    private readonly IntPtr _BufferLength;
    private readonly IntPtr _Buffer;
    private readonly bool _LeaveOpen;
    private long _CurPos;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;
    public override long Length => _BufferLength.ToInt64();
    
    public override long Position
    {
        get => _CurPos;
        set => Seek(value, SeekOrigin.Begin);
    }

    public IntPtr NativeLength
    {
        get => _BufferLength;
    }

    public IntPtr Buffer
    {
        get => _Buffer;
    }

    public MemoryPointerStream(IntPtr buffer, int bufferLength, bool leaveOpen)
    {
        _Buffer = buffer;
        _BufferLength = new IntPtr(bufferLength);
        _LeaveOpen = leaveOpen;
    }

    public MemoryPointerStream(IntPtr buffer, IntPtr bufferLength, bool leaveOpen)
    {
        _Buffer = buffer;
        _BufferLength = bufferLength;
        _LeaveOpen = leaveOpen;
    }

    public MemoryPointerStream(int bufferLength)
    {
        _Buffer = Marshal.AllocHGlobal(bufferLength);
        _BufferLength = new IntPtr(bufferLength);
        _LeaveOpen = false;
    }

    public MemoryPointerStream(IntPtr bufferLength)
    {
        _Buffer = Marshal.AllocHGlobal(bufferLength);
        _BufferLength = bufferLength;
        _LeaveOpen = false;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_CurPos + count > Length)
            throw new ArgumentOutOfRangeException();

        for (int i = offset; i < count; i++)
            buffer[i] = Marshal.ReadByte(_Buffer, (int)_CurPos++);

        return count;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_CurPos + count > Length)
            throw new ArgumentOutOfRangeException();

        for (int i = offset; i < count; i++)
            Marshal.WriteByte(_Buffer, (int)_CurPos++, buffer[i]);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                {
                    if (offset > Length)
                        throw new ArgumentOutOfRangeException();

                    _CurPos = offset;
                    break;
                }

            case SeekOrigin.Current:
                {
                    if (_CurPos + offset > Length)
                        throw new ArgumentOutOfRangeException();

                    _CurPos += offset;
                    break;
                }

            case SeekOrigin.End:
                {
                    if (offset > Length)
                        throw new ArgumentOutOfRangeException();

                    _CurPos = Length - offset;
                    break;
                }
        }

        return _CurPos;
    }

    public override void SetLength(long value)
    {
        throw new InvalidOperationException();
    }

    public override void Flush()
    {
        _ = 0xBAD + 0xC0DE;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_LeaveOpen)
            Marshal.FreeHGlobal(_Buffer);

        base.Dispose(disposing);
    }
}
