using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace LibSharpUpdater;

public class FileVersion(int major, int minor, int patch = 0, string build = "") : IComparable<FileVersion>
{
    public virtual bool IsGreaterThan(FileVersion other)
    {
        if (Major != other.Major)
            return Major > other.Major;

        if (Minor != other.Minor)
            return Minor > other.Minor;

        if (Patch != other.Patch)
            return Patch > other.Patch;

        if (!BuildTypeIntMap.TryGetValue(Build, out var a))
            throw new NotSupportedException($"Build type \"{Build}\" is not supported.");

        if (!BuildTypeIntMap.TryGetValue(other.Build, out var b))
            throw new NotSupportedException($"Build type \"{other.Build}\" is not supported.");

        return a > b;
    }
    public override bool Equals(object obj)
    {
        if (obj is not FileVersion other) return false;
        return other.Major == Major && other.Minor == Minor && other.Patch == Patch && other.Build == Build;
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Major.GetHashCode();
            hash = hash * 31 + Minor.GetHashCode();
            hash = hash * 31 + Patch.GetHashCode();
            hash = hash * 31 + (Build?.GetHashCode() ?? 0);
            return hash;
        }
    }
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"{Major}.{Minor}");
        if(Patch != 0) sb.Append($".{Patch}");
        if(Build.Length > 0) sb.Append(Build);

        return sb.ToString();
    }
    public virtual int CompareTo(FileVersion other)
    {
        if (this < other) return -1;
        else if(this == other) return 0;
        else return 1;
    }

    public static bool TryParse(string? str, out FileVersion? version)
    {
        if (CustomParser != null) return CustomParser(str, out version);
        version = null;
        if (string.IsNullOrWhiteSpace(str)) return false;

        var match = DefaultVersionRegex.Match(str);
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups[1].Value, out int major)) return false;
        if (!int.TryParse(match.Groups[2].Value, out int minor)) return false;

        int patch = 0;
        string build = string.Empty;

        if (match.Groups[4].Success)
            if (!int.TryParse(match.Groups[4].Value, out patch)) return false;
        if (match.Groups[5].Success)
            build = match.Groups[5].Value;

        version = new(major, minor, patch, build);
        return true;
    }
    public static FileVersion Parse(string str)
    {
        if (!TryParse(str, out var version))
            throw new FormatException("Invalid version format.");

        return version!;
    }

    public static bool operator >(FileVersion a, FileVersion b) => a.IsGreaterThan(b);
    public static bool operator <(FileVersion a, FileVersion b) => !a.IsGreaterThan(b) && !a.Equals(b);
    public static bool operator >=(FileVersion a, FileVersion b) => a.Equals(b) || a.IsGreaterThan(b);
    public static bool operator <=(FileVersion a, FileVersion b) => !a.IsGreaterThan(b);
    public static bool operator ==(FileVersion a, FileVersion b) => a.Equals(b);
    public static bool operator !=(FileVersion a, FileVersion b) => !a.Equals(b);

    public int Major { get; } = major;
    public int Minor { get; } = minor;
    public int Patch { get; } = patch;
    public string Build { get; } = build;

    public static FileVersionParsingHandler? CustomParser { get; set; }
    public static Regex DefaultVersionRegex { get; } = new(@"^v?(\d{1,3})\.(\d{1,3})(\.(\d{1,5}))?(\w+)?$");
    public static ReadOnlyDictionary<string, int> BuildTypeIntMap { get; } = new(new Dictionary<string, int>
    {
        { "", 0 },
        { "public", 0 },
        { "rc", 1 },
        { "alpha", 2 }
    });
}

public delegate bool FileVersionParsingHandler(string? str, out FileVersion? version);