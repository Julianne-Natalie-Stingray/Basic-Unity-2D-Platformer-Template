using UnityEngine;

/// <summary>
/// 结算场景的退出动作.
/// 归属: UI.
/// </summary>
[DisallowMultipleComponent]
public sealed class FinalSceneScreen : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
