using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core;

[ExcludeFromCodeCoverage]
public class ToStringJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override bool CanRead => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return new PackageVersion((string)reader.Value);
    }
}

[JsonConverter(typeof(ToStringJsonConverter))]
public class PackageVersion : IComparable
{
    #region IComparable
    public override bool Equals(object? rhs)
    {
        return CompareTo(rhs) == 0;
    }

    public override int GetHashCode()
    {
        return GetComparable().GetHashCode();
    }

    public int CompareTo(object? rhs)
    {
        if (rhs == null)
        {
            return 1;
        }

        if (ReferenceEquals(this, rhs))
        {
            return 0;
        }

        if (rhs is not PackageVersion castRhs)
        {
            return 1;
        }

        return GetComparable().CompareTo(castRhs.GetComparable());
    }

    public long GetComparable()
    {
        var suffixNumber = 0;
        if (_currentValues.ContainsKey(VersionPart.SuffixNumber))
        {
            suffixNumber = _currentValues[VersionPart.SuffixNumber];
        }
        return long.Parse($"{_currentValues[VersionPart.Major]:D4}{_currentValues[VersionPart.Minor]:D2}{_currentValues[VersionPart.Patch]:D2}{suffixNumber:D3}");
    }


    public static bool operator <(PackageVersion? lhs, PackageVersion? rhs)
    {
        if (lhs == null)
        {
            return true;
        }
        if (rhs == null)
        {
            return false;
        }
        return lhs.CompareTo(rhs) == -1;
    }

    public static bool operator >(PackageVersion? lhs, PackageVersion? rhs)
    {
        if (lhs == null)
        {
            return false;
        }
        if (rhs == null)
        {
            return true;
        }
        return lhs.CompareTo(rhs) == 1;
    }
    #endregion

    public enum VersionPart
    {
        Major = 1,
        Minor = 2,
        Patch = 3,
        Suffix = 4,
        SuffixNumber = 5
    };

    private readonly Regex _versionNameMatcher = new($"^(?<{VersionPart.Major.ToString().ToLowerInvariant()}>\\d+)\\.(?<{VersionPart.Minor.ToString().ToLowerInvariant()}>\\d+)\\.(?<{VersionPart.Patch.ToString().ToLowerInvariant()}>\\d+)-*(?<{VersionPart.Suffix.ToString().ToLowerInvariant()}>[\\D]*(?<{VersionPart.SuffixNumber.ToString().ToLowerInvariant()}>\\d*)[\\D]*)?");

    private readonly Dictionary<VersionPart, int> _currentValues = new();
    public readonly string _suffix = "";

    public bool IsPreview { get; }

    public PackageVersion(string fullVersion)
    {
        var matchAttempt = _versionNameMatcher.Match(fullVersion);
        if (!matchAttempt.Success)
        {
            throw new InvalidVersionSyntaxException(fullVersion, $"Needs to match regex: {_versionNameMatcher}");
        }

        foreach (var partName in Enum.GetValues<VersionPart>())
        {
            if (partName == VersionPart.Suffix)
            {
                _suffix = matchAttempt.Groups[partName.ToString().ToLowerInvariant()].Value;
                continue;
            }

            if (string.IsNullOrEmpty(matchAttempt.Groups[partName.ToString().ToLowerInvariant()].Value))
            {
                continue;
            }

            if (partName == VersionPart.SuffixNumber)
            {
                _currentValues[partName] = int.Parse(matchAttempt.Groups[partName.ToString().ToLowerInvariant()].Value)+1;
                continue;
            }

            _currentValues[partName] = int.Parse(matchAttempt.Groups[partName.ToString().ToLowerInvariant()].Value);
        }

        if (!string.IsNullOrEmpty(_suffix) && !_currentValues.ContainsKey(VersionPart.SuffixNumber))
        {
            _currentValues[VersionPart.SuffixNumber] = 1;
        }

        IsPreview = _currentValues.ContainsKey(VersionPart.SuffixNumber);
    }

    public int GetVersionPart(VersionPart versionPart)
    {
        return _currentValues[versionPart];
    }
    
    /// <summary>
    /// Returns a string representing this version in both a Unity friendly string and the raw <see cref="GetComparable()"/>
    /// </summary>
    /// <returns>A string representing this version in both a Unity friendly string and the raw <see cref="GetComparable()"/></returns>
    public override string ToString()
    {
        var requiredBlock = $"{_currentValues[VersionPart.Major]}.{_currentValues[VersionPart.Minor]}.{_currentValues[VersionPart.Patch]}";
        var optionalBlock = "";
        if (!string.IsNullOrWhiteSpace(_suffix))
        {
            optionalBlock = "-" + _suffix;
        }

        return requiredBlock + optionalBlock;
    }
}