/// <summary>
/// 一次存档尝试的结果; 失败时带可直接展示/记录的原因.
/// 归属: Game (Save).
/// </summary>
public readonly struct SaveResult
{
    public bool Succeeded { get; }

    public string Error { get; }

    private SaveResult(bool succeeded, string error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static SaveResult Success()
        => new(true, string.Empty);

    public static SaveResult Failure(string error)
        => new(false, string.IsNullOrWhiteSpace(error) ? "Unknown save failure." : error);
}
