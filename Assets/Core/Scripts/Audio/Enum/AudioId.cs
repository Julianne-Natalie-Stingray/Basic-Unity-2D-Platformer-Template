using System;
using GrignardReagent.LayeredId;
using UnityEngine;

/// <summary>
/// 音频 clip 的唯一标识: 只序列化一个层级路径字符串.
/// 归属: Core (Audio).
/// </summary>
[Serializable]
public struct AudioId : ILayeredId, IEquatable<AudioId>
{
    [SerializeField] private string path;

    public string Path => path ?? string.Empty;

    public bool IsValid => !string.IsNullOrWhiteSpace(path);

    public static AudioId None => default;

    public static AudioId From(string path) => new() { path = path ?? string.Empty };

    public bool Equals(AudioId other)
        => string.Equals(Path, other.Path, StringComparison.Ordinal);

    public override bool Equals(object obj) => obj is AudioId other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path);

    public override string ToString() => IsValid ? Path : "None";

    public static bool operator ==(AudioId left, AudioId right) => left.Equals(right);

    public static bool operator !=(AudioId left, AudioId right) => !left.Equals(right);
}
