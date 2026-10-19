/// <summary>
/// 可被环境杀死的对象: 环境只负责调用 Kill, 具体死亡表现由实现者负责.
/// 归属: Contracts.
/// </summary>
public interface IKillable
{
    /// <summary>施加死亡; 实现者需保证幂等.</summary>
    void Kill();
}
