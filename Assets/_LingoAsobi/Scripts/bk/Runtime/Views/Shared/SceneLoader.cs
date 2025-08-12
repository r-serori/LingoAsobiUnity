using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理に必須

/// <summary>
/// シーンの読み込みを管理するクラス
/// </summary>
namespace Scripts.Runtime.Views.Shared
{
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// 指定された名前のシーンを読み込む
        /// </summary>
        /// <param name="sceneName">読み込むシーンのファイル名</param>
        public void LoadScene(string sceneName)
        {
            // sceneNameが空でないことを確認
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name cannot be empty!");
                return;
            }

            Debug.Log($"Loading scene: {sceneName}...");
            SceneManager.LoadScene(sceneName);
        }
    }
}