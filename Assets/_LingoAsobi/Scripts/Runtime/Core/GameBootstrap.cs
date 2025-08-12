using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Core;

public class GameBootstrap : MonoBehaviour
{
  [Header("Initialization Settings")]
  [SerializeField] private float maxInitTimeout = 30f;

  private async void Start()
  {
    try
    {
      await InitializeGameAsync();
    }
    catch (Exception ex)
    {
      Debug.LogError($"Game initialization failed: {ex.Message}");
      ShowInitializationError();
    }
  }

  private async Task InitializeGameAsync()
  {
    // 1. DataManagerの存在確認
    if (DataManager.Instance == null)
    {
      Debug.LogError("DataManager not found!");
      return;
    }

    // 2. DataManagerの初期化
    ShowLoadingMessage("ゲームデータを読み込み中...");
    await DataManager.Instance.InitializeAsync();

    // 3. 初期化完了後の処理
    ShowLoadingMessage("ゲーム画面を準備中...");
    SceneManager.LoadScene("HomeScene");
  }
}
