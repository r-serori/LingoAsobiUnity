using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Scripts.Runtime.Utilities
{
  /// <summary>
  /// LingoAsobi パフォーマンス最適化ユーティリティ
  /// メモリ管理、GC負荷軽減、UI最適化を担当
  /// </summary>
  public class PerformanceOptimizer : MonoBehaviour
  {
    [Header("メモリ管理設定")]
    [SerializeField] private bool enableAutomaticGC = true;
    [SerializeField] private float gcInterval = 30f; // 30秒間隔
    [SerializeField] private bool enableMemoryWarning = true;
    [SerializeField] private long memoryWarningThreshold = 100 * 1024 * 1024; // 100MB

    [Header("UI最適化設定")]
    [SerializeField] private bool enableCanvasOptimization = true;
    [SerializeField] private bool enableGraphicRaycasterOptimization = true;
    [SerializeField] private int maxUIUpdatesPerFrame = 5;

    [Header("オブジェクトプール設定")]
    [SerializeField] private bool enableObjectPooling = true;
    [SerializeField] private int defaultPoolSize = 10;

    [Header("デバッグ")]
    [SerializeField] private bool showPerformanceStats = true;
    [SerializeField] private float statsUpdateInterval = 1f;

    // Singleton
    private static PerformanceOptimizer _instance;
    public static PerformanceOptimizer Instance => _instance;

    // 統計情報
    public struct PerformanceStats
    {
      public long usedMemoryBytes;
      public long totalMemoryBytes;
      public float memoryUsagePercent;
      public int frameRate;
      public float deltaTime;
      public int activeGameObjects;
    }

    private PerformanceStats currentStats;
    private Coroutine gcCoroutine;
    private Coroutine statsCoroutine;

    // オブジェクトプール
    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();

    // UI最適化
    private Queue<System.Action> uiUpdateQueue = new Queue<System.Action>();
    private int uiUpdatesThisFrame = 0;

    void Awake()
    {
      if (_instance == null)
      {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeOptimizer();
      }
      else
      {
        Destroy(gameObject);
      }
    }

    void Start()
    {
      if (enableAutomaticGC)
      {
        gcCoroutine = StartCoroutine(AutomaticGCCoroutine());
      }

      if (showPerformanceStats)
      {
        statsCoroutine = StartCoroutine(UpdateStatsCoroutine());
      }
    }

    void Update()
    {
      ProcessUIUpdateQueue();
    }

    /// <summary>
    /// オプティマイザーの初期化
    /// </summary>
    private void InitializeOptimizer()
    {
      // Canvas最適化
      if (enableCanvasOptimization)
      {
        OptimizeCanvases();
      }

      // GraphicRaycaster最適化
      if (enableGraphicRaycasterOptimization)
      {
        OptimizeGraphicRaycasters();
      }

    }

    /// <summary>
    /// 自動ガベージコレクション
    /// </summary>
    private IEnumerator AutomaticGCCoroutine()
    {
      while (true)
      {
        yield return new WaitForSeconds(gcInterval);

        // メモリ使用量チェック
        long currentMemory = System.GC.GetTotalMemory(false);

        if (currentMemory > memoryWarningThreshold)
        {
          if (enableMemoryWarning)
          {
            Debug.LogWarning($"PerformanceOptimizer: メモリ使用量が閾値を超過 ({currentMemory / (1024 * 1024)}MB)");
          }

          // 強制ガベージコレクション
          System.GC.Collect();
          System.GC.WaitForPendingFinalizers();

          // リソース解放
          Resources.UnloadUnusedAssets();

        }
      }
    }

    /// <summary>
    /// パフォーマンス統計更新
    /// </summary>
    private IEnumerator UpdateStatsCoroutine()
    {
      while (true)
      {
        yield return new WaitForSeconds(statsUpdateInterval);

        UpdatePerformanceStats();

        if (showPerformanceStats)
        {
          LogPerformanceStats();
        }
      }
    }

    /// <summary>
    /// パフォーマンス統計の更新
    /// </summary>
    private void UpdatePerformanceStats()
    {
      currentStats.usedMemoryBytes = System.GC.GetTotalMemory(false);
      currentStats.totalMemoryBytes = System.GC.GetTotalMemory(true);
      currentStats.memoryUsagePercent = (float)currentStats.usedMemoryBytes / currentStats.totalMemoryBytes * 100f;
      currentStats.frameRate = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
      currentStats.deltaTime = Time.deltaTime;
      currentStats.activeGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length;
    }

    /// <summary>
    /// パフォーマンス統計のログ出力
    /// </summary>
    private void LogPerformanceStats()
    {
      // Debug.Log($"PerformanceOptimizer: メモリ使用量: {currentStats.usedMemoryBytes / (1024 * 1024)}MB " +
      //          $"({currentStats.memoryUsagePercent:F1}%), FPS: {currentStats.frameRate}, " +
      //          $"Objects: {currentStats.activeGameObjects}, DeltaTime: {currentStats.deltaTime}");
    }

    /// <summary>
    /// Canvas最適化
    /// </summary>
    private void OptimizeCanvases()
    {
      Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

      foreach (Canvas canvas in canvases)
      {
        // Pixel Perfect無効化（パフォーマンス向上）
        canvas.pixelPerfect = false;

        // Static UI要素のキャッシュ
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
          canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
        }
      }

    }

    /// <summary>
    /// GraphicRaycaster最適化
    /// </summary>
    private void OptimizeGraphicRaycasters()
    {
      UnityEngine.UI.GraphicRaycaster[] raycasters = FindObjectsByType<UnityEngine.UI.GraphicRaycaster>(FindObjectsSortMode.None);

      foreach (var raycaster in raycasters)
      {
        // 不要なGraphicRaycasterを無効化
        if (raycaster.GetComponent<Canvas>()?.sortingLayerName == "Background")
        {
          raycaster.enabled = false;
        }
      }

    }

    /// <summary>
    /// オブジェクトプールからオブジェクト取得
    /// </summary>
    public GameObject GetPooledObject(string poolKey, GameObject prefab = null)
    {
      if (!enableObjectPooling) return null;

      if (!objectPools.ContainsKey(poolKey))
      {
        objectPools[poolKey] = new Queue<GameObject>();

        // 初期プール作成
        if (prefab != null)
        {
          for (int i = 0; i < defaultPoolSize; i++)
          {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            objectPools[poolKey].Enqueue(obj);
          }
        }
      }

      Queue<GameObject> pool = objectPools[poolKey];

      if (pool.Count > 0)
      {
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
      }
      else if (prefab != null)
      {
        // プールが空の場合は新規作成
        return Instantiate(prefab);
      }

      return null;
    }

    /// <summary>
    /// オブジェクトをプールに戻す
    /// </summary>
    public void ReturnToPool(string poolKey, GameObject obj)
    {
      if (!enableObjectPooling || obj == null) return;

      obj.SetActive(false);

      if (objectPools.ContainsKey(poolKey))
      {
        objectPools[poolKey].Enqueue(obj);
      }
    }

    /// <summary>
    /// UI更新をキューに追加（フレーム分散処理）
    /// </summary>
    public void QueueUIUpdate(System.Action updateAction)
    {
      if (updateAction != null)
      {
        uiUpdateQueue.Enqueue(updateAction);
      }
    }

    /// <summary>
    /// UI更新キューの処理
    /// </summary>
    private void ProcessUIUpdateQueue()
    {
      uiUpdatesThisFrame = 0;

      while (uiUpdateQueue.Count > 0 && uiUpdatesThisFrame < maxUIUpdatesPerFrame)
      {
        try
        {
          var action = uiUpdateQueue.Dequeue();
          action?.Invoke();
          uiUpdatesThisFrame++;
        }
        catch (System.Exception ex)
        {
          Debug.LogError($"PerformanceOptimizer: UI更新エラー - {ex.Message}");
        }
      }
    }

    /// <summary>
    /// 手動メモリクリーンアップ
    /// </summary>
    [ContextMenu("Force Memory Cleanup")]
    public void ForceMemoryCleanup()
    {
      System.GC.Collect();
      System.GC.WaitForPendingFinalizers();
      Resources.UnloadUnusedAssets();

    }

    /// <summary>
    /// 現在のパフォーマンス統計取得
    /// </summary>
    public PerformanceStats GetCurrentStats()
    {
      return currentStats;
    }

    /// <summary>
    /// オプティマイザーの有効/無効切り替え
    /// </summary>
    public void SetOptimizerEnabled(bool enabled)
    {
      this.enabled = enabled;

      if (enabled)
      {
        if (enableAutomaticGC && gcCoroutine == null)
        {
          gcCoroutine = StartCoroutine(AutomaticGCCoroutine());
        }

        if (showPerformanceStats && statsCoroutine == null)
        {
          statsCoroutine = StartCoroutine(UpdateStatsCoroutine());
        }
      }
      else
      {
        if (gcCoroutine != null)
        {
          StopCoroutine(gcCoroutine);
          gcCoroutine = null;
        }

        if (statsCoroutine != null)
        {
          StopCoroutine(statsCoroutine);
          statsCoroutine = null;
        }
      }

    }

    void OnDestroy()
    {
      if (_instance == this)
      {
        _instance = null;
      }
    }

    void OnApplicationPause(bool pauseStatus)
    {
      if (pauseStatus && enableAutomaticGC)
      {
        // アプリがバックグラウンドに移行時にメモリクリーンアップ
        ForceMemoryCleanup();
      }
    }

    void OnApplicationFocus(bool hasFocus)
    {
      if (!hasFocus && enableAutomaticGC)
      {
        // フォーカス喪失時にもメモリクリーンアップ
        ForceMemoryCleanup();
      }
    }
  }
}