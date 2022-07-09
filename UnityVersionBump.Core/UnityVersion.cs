using System.Text.RegularExpressions;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core;


public class UnityVersion : IComparable
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

        if (rhs is not UnityVersion castRhs)
        {
            return 1;
        }

        return GetComparable().CompareTo(castRhs.GetComparable());
    }

    public long GetComparable()
    {
        return long.Parse($"{_currentValues[VersionPart.Major]:D4}{_currentValues[VersionPart.Minor]:D2}{_currentValues[VersionPart.Patch]:D2}{_currentValues[VersionPart.ReleaseStream]}{_currentValues[VersionPart.Build]:D3}");
    }

    public static bool operator <(UnityVersion? lhs, UnityVersion? rhs)
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

    public static bool operator >(UnityVersion? lhs, UnityVersion? rhs)
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
        ReleaseStream = 4,
        Build = 5
    }

    public enum ReleaseStreamType
    {
        Alpha = 1,
        Beta = 2,
        Patch = 3, // hot-fixes for particular issues
        Stable = 4,
        LTS
    }

    private readonly Regex _versionNameMatcher = new($"^(?<{VersionPart.Major.ToString().ToLowerInvariant()}>\\d+)\\.(?<{VersionPart.Minor.ToString().ToLowerInvariant()}>\\d+)\\.(?<{VersionPart.Patch.ToString().ToLowerInvariant()}>\\d+)(?<{VersionPart.ReleaseStream.ToString().ToLowerInvariant()}>\\w)?(?<{VersionPart.Build.ToString().ToLowerInvariant()}>\\d+)?");
    private readonly Regex _revisionValidator = new("^[a-f0-9]{12}$");

    private readonly Dictionary<VersionPart, int> _versionPartMaximumLength = new()
    {
        { VersionPart.Major, 4 },
        { VersionPart.Minor, 2 },
        { VersionPart.Patch, 2 },
        { VersionPart.ReleaseStream, 1 },
        { VersionPart.Build, 3 }
    };

    private readonly Dictionary<VersionPart, int> _currentValues = new();

    public string Revision { get; }
    public bool IsLTS { get; }
    public ReleaseStreamType ReleaseStream => (ReleaseStreamType)_currentValues[VersionPart.ReleaseStream];

    public UnityVersion(string fullVersion, string revision, bool isLTS)
    {
        if (!_revisionValidator.IsMatch(revision))
        {
            throw new InvalidRevisionSyntaxException(revision);
        }

        Revision = revision;
        IsLTS = isLTS;

        var matchAttempt = _versionNameMatcher.Match(fullVersion);
        if (!matchAttempt.Success)
        {
            throw new InvalidVersionSyntaxException(fullVersion, _versionNameMatcher.ToString());
        }

        foreach (var (partName, maximumLength) in _versionPartMaximumLength)
        {
            var partNameString = partName.ToString().ToLowerInvariant();
            if (partName == VersionPart.ReleaseStream)
            {
                if (!matchAttempt.Groups[partNameString].Success)
                {
                    _currentValues[VersionPart.ReleaseStream] = (int)ReleaseStreamType.Stable;
                    _currentValues[VersionPart.Build] = 0;
                    break;
                }
            }

            var parsedValue = matchAttempt.Groups[partNameString].Value;
            if (parsedValue.Length > maximumLength)
            {
                throw new MismatchingLengthException(fullVersion, partName.ToString(), maximumLength, parsedValue.Length);
            }

            if (partName == VersionPart.ReleaseStream)
            {
                var shorthand = matchAttempt.Groups[partNameString].Value[0];
                _currentValues[partName] = shorthand switch
                {
                    'a' => (int)ReleaseStreamType.Alpha,
                    'b' => (int)ReleaseStreamType.Beta,
                    'p' => (int)ReleaseStreamType.Patch,
                    'f' => (int)ReleaseStreamType.Stable,
                    _ => throw new UnsupportedReleaseStream(shorthand)
                };
                continue;
            }

            _currentValues[partName] = int.Parse(matchAttempt.Groups[partName.ToString().ToLowerInvariant()].Value);
        }
    }

    public int GetVersionPart(VersionPart versionPart)
    {
        if (versionPart == VersionPart.ReleaseStream)
        {
            throw new ArgumentException($"Use '{nameof(UnityVersion)}.{nameof(ReleaseStream)}()' to get type-safe information about the release stream of this release");
        }
        return _currentValues[versionPart];
    }

    #region ToString()
    /// <summary>
    /// Returns a string representation of this version identical to the format used by the Unity launcher or "About Unity" menu
    /// </summary>
    /// <returns>A string representation of this version identical to the format used by the Unity launcher of "About Unity" menu</returns>
    /// <see cref="ToString()"/>
    /// <see cref="ToUnityStringWithRevision()"/>
    public string ToUnityString()
    {
        var requiredBlock = $"{_currentValues[VersionPart.Major]}.{_currentValues[VersionPart.Minor]}.{_currentValues[VersionPart.Patch]}";
        var optionalBlock = "";
        if (_currentValues[VersionPart.Build] != 0)
        {
            if (ReleaseStream == ReleaseStreamType.Stable)
            {
                optionalBlock = $"f{_currentValues[VersionPart.Build]}";
            }
            else
            {
                optionalBlock = $"{ReleaseStream.ToString().ToLowerInvariant()[0]}{_currentValues[VersionPart.Build]}";
            }
        }

        return requiredBlock + optionalBlock;
    }

    /// <summary>
    /// Returns a string representation of this version with revision hash used by ProjectVersion.txt
    /// </summary>
    /// <returns>A string representation of this version with revision hash used by ProjectVersion.txt</returns>
    /// <see cref="ToString()"/>
    /// <see cref="ToUnityString()"/>
    public string ToUnityStringWithRevision()
    {
        return $"{ToUnityString()} ({Revision})";
    }

    /// <summary>
    /// Returns a string representing this version in both a Unity friendly string and the raw <see cref="GetComparable()"/>
    /// </summary>
    /// <returns>A string representing this version in both a Unity friendly string and the raw <see cref="GetComparable()"/></returns>
    /// <see cref="ToUnityString()"/>
    /// <see cref="ToUnityStringWithRevision()"/>
    public override string ToString()
    {
        return $"{ToUnityStringWithRevision()} ({GetComparable()})";
    }
    #endregion
}