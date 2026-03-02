using System;

namespace System.Firmware;

/// <summary>
/// Represents a boot option index, which is a 16-bit unsigned integer value used to identify a specific boot option in the system's firmware.
/// The structure provides an implicit conversion to and from ushort, as well as a string representation in the format "BootXXXX", where XXXX is the hexadecimal representation of the index value.
/// </summary>
/// <param name="value"></param>
public readonly struct BootOptionIndex(ushort value) : IEquatable<BootOptionIndex>, IEquatable<ushort>, IComparable<BootOptionIndex>, IComparable<ushort>
{
    private readonly ushort _value = value;

    /// <inheritdoc/>
    public int CompareTo(BootOptionIndex other) => _value.CompareTo(other._value);

    /// <inheritdoc/>
    public int CompareTo(ushort other) => _value.CompareTo(other);

    /// <inheritdoc/>
    public bool Equals(BootOptionIndex other) => _value.Equals(other._value);

    /// <inheritdoc/>
    public bool Equals(ushort other) => _value.Equals(other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BootOptionIndex other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _value.GetHashCode();

    /// <summary>
    /// Determines whether two BootOptionIndex instances are equal.
    /// </summary>
    /// <param name="left">The first BootOptionIndex to compare.</param>
    /// <param name="right">The second BootOptionIndex to compare.</param>
    /// <returns>true if the specified BootOptionIndex instances are equal; otherwise, false.</returns>
    public static bool operator ==(BootOptionIndex left, BootOptionIndex right) => left.Equals(right);

    /// <summary>
    /// Determines whether two BootOptionIndex instances are not equal.
    /// </summary>
    /// <param name="left">The first BootOptionIndex to compare.</param>
    /// <param name="right">The second BootOptionIndex to compare.</param>
    /// <returns>true if the specified BootOptionIndex instances are not equal; otherwise, false.</returns>
    public static bool operator !=(BootOptionIndex left, BootOptionIndex right) => !left.Equals(right);

    /// <summary>
    /// Returns a string that represents boot option index in format "BootXXXX", where XXXX is the hexadecimal representation of the index value.
    /// </summary>
    public override readonly string ToString() => $"Boot{_value:X4}";

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
