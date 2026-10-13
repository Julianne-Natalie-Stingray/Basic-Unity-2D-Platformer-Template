using UnityEngine;

/// <summary>
/// 通过动画事件向 Core 音频服务请求音效.
/// 归属: Player.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerAnimationAudio : MonoBehaviour
{
    [SerializeField] private AudioId deathAudio = AudioId.None;
    [SerializeField] private AudioId moveAudio = AudioId.None;
    [SerializeField] private AudioId interactAudio = AudioId.None;

    private ISoundHandle deathHandle;
    private ISoundHandle moveHandle;
    private ISoundHandle interactHandle;

    /// <summary>死亡动画事件: 允许在死亡面板冻结游戏后继续发声.</summary>
    public void PlayDeathAudio()
        => Play(deathAudio, true, ref deathHandle);

    /// <summary>移动动画事件: 可在每个落脚帧调用.</summary>
    public void PlayMoveAudio()
    {
        Motor motor = GetComponentInParent<Motor>();
        if (motor != null && motor.IsDead)
            return;

        Play(moveAudio, false, ref moveHandle);
    }

    /// <summary>交互动画事件: 在需要发声的帧调用.</summary>
    public void PlayInteractAudio()
        => Play(interactAudio, false, ref interactHandle);

    private bool Play(AudioId id, bool surviveFreeze, ref ISoundHandle handle)
    {
        if (!isActiveAndEnabled || id == AudioId.None)
            return false;
        if (!surviveFreeze && Time.timeScale <= 0f)
            return false;

        CoreFacade core = CoreFacade.Instance;
        if (core == null || core.Audio == null)
            return false;

        handle?.Stop();
        handle = core.Audio.CreateBuilder()
            .WithPosition(transform.position)
            .WithFollowTarget(transform)
            .WithSurviveFreeze(surviveFreeze)
            .WithAllowWhileFrozen(surviveFreeze)
            .Play(id);
        return handle != null;
    }

    private void OnDisable()
    {
        deathHandle?.Stop();
        moveHandle?.Stop();
        interactHandle?.Stop();
        deathHandle = null;
        moveHandle = null;
        interactHandle = null;
    }
}
