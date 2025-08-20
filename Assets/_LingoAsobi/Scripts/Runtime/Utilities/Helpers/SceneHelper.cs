using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Core;
using Scripts.Runtime.Utilities.Constants;

namespace Scripts.Runtime.Utilities.Helpers
{
  /// <summary>
  /// 改善版シーンヘルパー（既存コードとの互換性を保持）
  /// </summary>
  public class SceneHelper : MonoBehaviour
  {
    private static SceneHelper instance;
    private bool isTransitioning = false;

    // 現在のシーン名
    private string _currentSceneName;
    public string CurrentSceneName => _currentSceneName;

    // シーン遷移中フラグ
    private bool _isTransitioning;
    public bool IsTransitioning => _isTransitioning;

    // イベント
    public event Action<string, string> OnSceneTransitionStart;
    public event Action<string> OnSceneLoaded;
    public event Action<string> OnSceneUnloaded;

    // フェード用のCanvas（オプション）
    private GameObject _fadeCanvas;
    private CanvasGroup _fadeCanvasGroup;

    /// <summary>
    /// シングルトンインスタンス（既存コードとの互換性のため維持）
    /// </summary>
    public static SceneHelper Instance
    {
      get
      {
        if (instance == null)
        {
          CreateInstance();
        }
        return instance;
      }

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
      CreateInstance();
    }

    private static void CreateInstance()
    {
      if (instance != null) return;

      GameObject go = new GameObject("SceneHelper");
      instance = go.AddComponent<SceneHelper>();
      DontDestroyOnLoad(go);
    }

    private void Awake()
    {
      if (instance != null && instance != this)
      {
        Destroy(gameObject);
        return;
      }

      instance = this;

      // GameEventManagerの初期化を確実に行う
      EnsureEventManagerExists();
    }

    /// <summary>
    /// 非同期でシーンをロード（高速版）
    /// </summary>
    public async Task<bool> LoadSceneAsync(string sceneName, bool additive = false)
    {
      if (isTransitioning)
      {
        Debug.LogWarning($"[SceneHelper] Scene transition already in progress");
        return false;
      }

      if (string.IsNullOrEmpty(sceneName))
      {
        Debug.LogError("[SceneHelper] Scene name is null or empty");
        return false;
      }

      try
      {
        isTransitioning = true;

        // フェードを無効化（真っ暗にならない）
        // CreateFadeCanvas();
        // await FadeOut();

        // GameEventManagerとEventBusの確認
        EnsureEventManagerExists();

        // 遷移開始イベントを発行
        EventBus.Instance.Publish(new SceneTransitionEvent(sceneName,
            SceneTransitionEvent.TransitionPhase.Started));

        // シーンのロード開始
        var loadOperation = SceneManager.LoadSceneAsync(sceneName,
            additive ? LoadSceneMode.Additive : LoadSceneMode.Single);

        if (loadOperation == null)
        {
          throw new Exception($"Failed to start loading scene: {sceneName}");
        }

        loadOperation.allowSceneActivation = false;

        // ロード進捗の監視
        while (!loadOperation.isDone)
        {
          float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);

          // 進捗イベントを発行
          EventBus.Instance.Publish(new SceneTransitionEvent(sceneName,
              SceneTransitionEvent.TransitionPhase.Loading)
          {
            Progress = progress
          });

          // 90%まで読み込まれたらシーンをアクティベート
          if (loadOperation.progress >= 0.9f)
          {
            // 待機時間を短縮
            await Task.Delay(10); // 100ms → 10ms
            loadOperation.allowSceneActivation = true;
          }

          await Task.Yield();
        }

        // 完了イベントを発行
        EventBus.Instance.Publish(new SceneTransitionEvent(sceneName,
            SceneTransitionEvent.TransitionPhase.Completed));

        // フェードを無効化（真っ暗にならない）
        // await FadeIn();

        return true;
      }
      catch (Exception e)
      {
        Debug.LogError($"[SceneHelper] Error loading scene {sceneName}: {e.Message}");

        // 失敗イベントを発行
        EventBus.Instance.Publish(new SceneTransitionEvent(sceneName,
            SceneTransitionEvent.TransitionPhase.Failed));

        // エラー時もフェードインを無効化
        // await FadeIn();

        return false;
      }
      finally
      {
        isTransitioning = false;
      }
    }

    /// <summary>
    /// GameEventManagerが存在することを保証
    /// </summary>
    private void EnsureEventManagerExists()
    {
      var manager = GameEventManager.Instance;
      var eventBus = EventBus.Instance;

    }

    /// <summary>
    /// 現在のシーン名を取得
    /// </summary>
    public string GetCurrentSceneName()
    {
      return SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// シーンが存在するか確認
    /// </summary>
    public bool SceneExists(string sceneName)
    {
      for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
      {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
        string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        if (name == sceneName)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// 静的メソッド版（既存コードとの互換性）
    /// </summary>
    public static async Task<bool> LoadSceneStaticAsync(string sceneName, bool additive = false)
    {
      return await Instance.LoadSceneAsync(sceneName, additive);
    }

    private void OnDestroy()
    {
      if (instance == this)
      {
        instance = null;
      }
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    private async Task FadeOut()
    {
      if (_fadeCanvasGroup == null) return;

      _fadeCanvasGroup.blocksRaycasts = true;
      // フェード時間を短縮（0.1秒）
      float duration = 0.1f;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        _fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
        await Task.Yield();
      }

      _fadeCanvasGroup.alpha = 1;
    }

    /// <summary>
    /// シーンを追加で読み込む
    /// </summary>
    public async Task LoadSceneAdditiveAsync(string sceneName)
    {
      AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

      while (!asyncLoad.isDone)
      {
        await Task.Yield();
      }

    }

    /// <summary>
    /// フェードイン
    /// </summary>
    private async Task FadeIn()
    {
      if (_fadeCanvasGroup == null) return;

      // フェード時間を短縮（0.1秒）
      float duration = 0.1f;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        _fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
        await Task.Yield();
      }

      _fadeCanvasGroup.alpha = 0;
      _fadeCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// ローディング画面を表示
    /// </summary>
    private async Task ShowLoadingScreen()
    {
      // ローディングシーンがある場合は追加で読み込む
      if (SceneManager.GetSceneByName(GameConstants.Scenes.Loading).IsValid())
      {
        await LoadSceneAdditiveAsync(GameConstants.Scenes.Loading);
      }

      await Task.Delay(100); // 表示を確実にするための待機
    }

    /// <summary>
    /// シーンをアンロード
    /// </summary>
    public async Task UnloadSceneAsync(string sceneName)
    {
      if (SceneManager.GetSceneByName(sceneName).isLoaded)
      {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

        while (!asyncUnload.isDone)
        {
          await Task.Yield();
        }

      }
    }

    /// <summary>
    /// ローディング画面を非表示
    /// </summary>
    private async Task HideLoadingScreen()
    {
      // ローディングシーンをアンロード
      if (SceneManager.GetSceneByName(GameConstants.Scenes.Loading).isLoaded)
      {
        await UnloadSceneAsync(GameConstants.Scenes.Loading);
      }
    }

    /// <summary>
    /// フェード用Canvasを作成
    /// </summary>
    public void CreateFadeCanvas()
    {
      if (_fadeCanvas != null) return;

      _fadeCanvas = new GameObject("FadeCanvas");
      _fadeCanvas.transform.SetParent(transform);

      Canvas canvas = _fadeCanvas.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = GameConstants.SortOrders.Overlay;

      _fadeCanvasGroup = _fadeCanvas.AddComponent<CanvasGroup>();
      _fadeCanvasGroup.alpha = 0;
      _fadeCanvasGroup.blocksRaycasts = false; // ← 初期はブロックしない

      // 黒い背景を追加
      GameObject bg = new GameObject("Background");
      bg.transform.SetParent(_fadeCanvas.transform, false);

      RectTransform rect = bg.AddComponent<RectTransform>();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.sizeDelta = Vector2.zero;

      UnityEngine.UI.Image image = bg.AddComponent<UnityEngine.UI.Image>();
      image.color = Color.black;
    }

    #region Utility Methods

    /// <summary>
    /// シーンが存在するか確認
    /// </summary>
    public bool DoesSceneExist(string sceneName)
    {
      for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
      {
        string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
        string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

        if (name == sceneName)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// アクティブなシーンを設定
    /// </summary>
    public void SetActiveScene(string sceneName)
    {
      Scene scene = SceneManager.GetSceneByName(sceneName);
      if (scene.IsValid())
      {
        SceneManager.SetActiveScene(scene);
      }
    }

    #endregion
  }
}