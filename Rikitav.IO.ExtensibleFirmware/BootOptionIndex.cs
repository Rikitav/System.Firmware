using System;

namespace Rikitav.IO.ExtensibleFirmware;

/// <summary>
/// Represents a boot option index, which is a 16-bit unsigned integer value used to identify a specific boot option in the system's firmware.
/// The structure provides an implicit conversion to and from ushort, as well as a string representation in the format "BootXXXX", where XXXX is the hexadecimal representation of the index value.
/// </summary>
/// <param name="value"></param>
public readonly struct BootOptionIndex(ushort value) : IEquatable<BootOptionIndex>, IEquatable<ushort>, IComparable<BootOptionIndex>, IComparable<ushort>
{
    private readonly ushort _value = value;
    private readonly string _stringValue = $"Boot{value:X4}";

    /// <inheritdoc/>
    public int CompareTo(BootOptionIndex other) => _value.CompareTo(other._value);

    /// <inheritdoc/>
    public int CompareTo(ushort other) => _value.CompareTo(other);

    /// <inheritdoc/>
    public bool Equals(BootOptionIndex other) => _value.Equals(other._value);

    /// <inheritdoc/>
    public bool Equals(ushort other) => _value.Equals(other);

    /// <summary>
    /// Returns a string that represents boot option index in format "BootXXXX", where XXXX is the hexadecimal representation of the index value.
    /// </summary>
    public override readonly string ToString() => _stringValue;

    /// <summary>
    /// Impictly converts <see cref="BootOptionIndex"/> structure to <see langword="ushort"/>
    /// </summary>
    /// <param name="index"></param>
    public static implicit operator ushort(BootOptionIndex index) => index._value;

    /// <summary>
    /// Implicitly converts <see langword="ushort"/> value to <see cref="BootOptionIndex"/> structure.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator BootOptionIndex(ushort value) => new BootOptionIndex(value);

    /// <summary>
    /// Converts a BootOptionIndex instance to its string representation.
    /// </summary>
    /// <param name="index"></param>
    public static implicit operator string(BootOptionIndex index) => index.ToString();
}
