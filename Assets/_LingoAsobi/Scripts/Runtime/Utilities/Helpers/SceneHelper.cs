using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Core;

namespace Scripts.Runtime.Utilities.Helpers
{
  /// <summary>
  /// シーン管理ヘルパークラス
  /// シーンの読み込み、遷移、管理を簡潔に行う
  /// </summary>
  public class SceneHelper : MonoBehaviour
  {
    // シングルトンインスタンス
    private static SceneHelper _instance;
    public static SceneHelper Instance
    {
      get
      {
        if (_instance == null)
        {
          GameObject go = new GameObject("SceneHelper");
          _instance = go.AddComponent<SceneHelper>();
          DontDestroyOnLoad(go);
        }
        return _instance;
      }
    }

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

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);

      _currentSceneName = SceneManager.GetActiveScene().name;
      SceneManager.sceneLoaded += OnSceneLoadedCallback;
      SceneManager.sceneUnloaded += OnSceneUnloadedCallback;
    }

    private void OnDestroy()
    {
      SceneManager.sceneLoaded -= OnSceneLoadedCallback;
      SceneManager.sceneUnloaded -= OnSceneUnloadedCallback;
    }

    #region Public Methods

    /// <summary>
    /// シーンを読み込む（フェード付き）
    /// </summary>
    public async Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
    {
      if (_isTransitioning)
      {
        Debug.LogWarning($"[SceneHelper] Already transitioning to a scene");
        return;
      }

      if (string.IsNullOrEmpty(sceneName))
      {
        Debug.LogError("[SceneHelper] Scene name is null or empty");
        return;
      }

      _isTransitioning = true;

      string previousScene = _currentSceneName;
      OnSceneTransitionStart?.Invoke(previousScene, sceneName);

      // イベントを発行
      EventBus.Instance.Publish(new SceneTransitionEvent
      {
        FromScene = previousScene,
        ToScene = sceneName
      });

      try
      {
        // フェードアウト
        if (_fadeCanvasGroup != null)
        {
          await FadeOut();
        }

        // ローディング画面を表示
        if (showLoadingScreen)
        {
          await ShowLoadingScreen();
        }

        // シーンを非同期で読み込む
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 読み込み進捗を監視
        while (!asyncLoad.isDone)
        {
          float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

          if (asyncLoad.progress >= 0.9f)
          {
            // シーンのアクティベーション
            asyncLoad.allowSceneActivation = true;
          }

          await Task.Yield();
        }

        _currentSceneName = sceneName;

        // ローディング画面を非表示
        if (showLoadingScreen)
        {
          await HideLoadingScreen();
        }

        // フェードイン
        if (_fadeCanvasGroup != null)
        {
          await FadeIn();
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"[SceneHelper] Failed to load scene: {e.Message}");
      }
      finally
      {
        _isTransitioning = false;
      }
    }

    /// <summary>
    /// シーンを即座に読み込む
    /// </summary>
    public void LoadSceneImmediate(string sceneName)
    {
      if (_isTransitioning)
      {
        Debug.LogWarning("[SceneHelper] Already transitioning");
        return;
      }

      SceneManager.LoadScene(sceneName);
      _currentSceneName = sceneName;
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

      Debug.Log($"[SceneHelper] Additive scene loaded: {sceneName}");
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

        Debug.Log($"[SceneHelper] Scene unloaded: {sceneName}");
      }
    }

    /// <summary>
    /// 現在のシーンをリロード
    /// </summary>
    public async Task ReloadCurrentSceneAsync()
    {
      await LoadSceneAsync(_currentSceneName);
    }

    /// <summary>
    /// ホーム画面に戻る
    /// </summary>
    public async Task GoToHomeAsync()
    {
      await LoadSceneAsync(GameConstants.Scenes.Home);
    }

    /// <summary>
    /// タイトル画面に戻る
    /// </summary>
    public async Task GoToTitleAsync()
    {
      await LoadSceneAsync(GameConstants.Scenes.Title);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// シーン読み込み完了コールバック
    /// </summary>
    private void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
      Debug.Log($"[SceneHelper] Scene loaded: {scene.name}");
      OnSceneLoaded?.Invoke(scene.name);
    }

    /// <summary>
    /// シーンアンロード完了コールバック
    /// </summary>
    private void OnSceneUnloadedCallback(Scene scene)
    {
      Debug.Log($"[SceneHelper] Scene unloaded: {scene.name}");
      OnSceneUnloaded?.Invoke(scene.name);
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    private async Task FadeOut()
    {
      if (_fadeCanvasGroup == null) return;

      float duration = GameConstants.Animation.SceneTransitionDuration;
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
    /// フェードイン
    /// </summary>
    private async Task FadeIn()
    {
      if (_fadeCanvasGroup == null) return;

      float duration = GameConstants.Animation.SceneTransitionDuration;
      float elapsed = 0;

      while (elapsed < duration)
      {
        elapsed += Time.deltaTime;
        _fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
        await Task.Yield();
      }

      _fadeCanvasGroup.alpha = 0;
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
      _fadeCanvasGroup.blocksRaycasts = true;

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

    #endregion

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