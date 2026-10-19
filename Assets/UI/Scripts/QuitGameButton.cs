using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 退出游戏的按钮; 编辑器内停止播放.
/// 归属: UI.
/// </summary>
[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public sealed class QuitGameButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Quit);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(Quit);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
