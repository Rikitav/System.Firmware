using System;
using System.IO;

namespace System.Firmware;

/// <summary>
/// Represents a strongly-typed volume path based on a unique volume identifier (GUID). Provides comparison, equality,
/// and conversion operations for working with volume paths in a type-safe manner.
/// </summary>
/// <remarks>
/// Use this struct to encapsulate and manipulate volume paths identified by GUIDs,
/// enabling type-safe operations and conversions to and from related types such as Guid, string, and DirectoryInfo.
/// Instances are immutable and can be compared or used in collections that require equality or ordering.
/// Implicit conversions simplify interoperability with APIs that accept Guid or string representations of volume paths.
/// </remarks>
/// <param name="volumeIdentificator">The unique identifier (GUID) that specifies the volume path associated with this instance.</param>
public readonly struct VolumePath(Guid volumeIdentificator) : IEquatable<VolumePath>, IEquatable<Guid>, IComparable<VolumePath>, IComparable<Guid>
{
    private readonly Guid _dentificator = volumeIdentificator;

    /// <summary>
    /// Gets the unique identifier associated with this instance.
    /// </summary>
    public Guid Identificator => _dentificator;

    /// <summary>
    /// Initializes a new instance of the VolumePath class using the specified string representation of a GUID.
    /// </summary>
    /// <param name="guidStr">A string containing the GUID that identifies the volume path. Must be a valid GUID format.</param>
    public VolumePath(string guidStr) : this(Guid.Parse(guidStr)) { }

    /// <inheritdoc/>
    public int CompareTo(VolumePath other) => _dentificator.CompareTo(other._dentificator);

    /// <inheritdoc/>
    public int CompareTo(Guid other) => _dentificator.CompareTo(other);

    /// <inheritdoc/>
    public bool Equals(VolumePath other) => _dentificator.Equals(other._dentificator);

    /// <inheritdoc/>
    public bool Equals(Guid other) => _dentificator.Equals(other);

    /// <summary>
    /// Returns the string representation of the volume in the Windows device path format.
    /// </summary>
    /// <remarks>
    /// This format is commonly used for referencing volumes in Windows APIs and system utilities.
    /// The returned string can be used for operations that require the full device path of a volume.
    /// </remarks>
    /// <returns>
    /// A string containing the volume path in the format "\\?\Volume{GUID}\",
    /// where GUID is the value of the <c>Identificator</c> property.
    /// </returns>
    public override readonly string ToString() => string.Concat(@"\\?\Volume{", Identificator.ToString(), @"}\");

    /// <summary>
    /// Converts a <see cref="VolumePath"/> instance to its associated <see cref="Guid"/> identifier.
    /// </summary>
    /// <param name="path">The <see cref="VolumePath"/> object to convert to a <see cref="Guid"/>.</param>
    public static implicit operator Guid(VolumePath path) => path.Identificator;

    /// <summary>
    /// Defines an implicit conversion from a <see cref="System.Guid"/> to a <c>VolumePath</c> instance.
    /// </summary>
    /// <param name="guid">The <see cref="System.Guid"/> value representing the unique identifier of the volume path to convert.</param>
    public static implicit operator VolumePath(Guid guid) => new VolumePath(guid);

    /// <summary>
    /// Converts a <see cref="VolumePath"/> instance to its string representation.
    /// </summary>
    /// <param name="path">The <see cref="VolumePath"/> instance to convert.</param>
    public static implicit operator string(VolumePath path) => path.ToString();

    /// <summary>
    /// Converts a string containing a volume GUID path to a <see cref="VolumePath"/> instance.
    /// </summary>
    /// <param name="guidStr">A string representing the volume GUID path to convert. Must not be null or empty.</param>
    public static implicit operator VolumePath(string guidStr) => new VolumePath(guidStr);

    /// <summary>
    /// Converts a <see cref="VolumePath"/> instance to a <see cref="DirectoryInfo"/> representing the same file system path.
    /// </summary>
    /// <param name="path">The <see cref="VolumePath"/> to convert to a <see cref="DirectoryInfo"/>.</param>
    public static implicit operator DirectoryInfo(VolumePath path) => new DirectoryInfo(path.ToString());
}
