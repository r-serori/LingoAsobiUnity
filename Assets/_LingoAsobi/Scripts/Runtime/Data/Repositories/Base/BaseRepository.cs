using System;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data;

/// <summary>
/// リポジトリの基底クラス - 共通機能を提供
/// </summary>
public abstract class BaseRepository : IRepository
{
  protected readonly APIClient api;
  protected readonly DataCache cache;
  protected readonly LocalDataStorage localStorage;
  protected readonly string repositoryName;

  protected BaseRepository(APIClient api, DataCache cache, LocalDataStorage localStorage, string repositoryName)
  {
    this.api = api ?? throw new ArgumentNullException(nameof(api));
    this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
    this.repositoryName = repositoryName;
  }

  #region Cache Management

  /// <summary>
  /// データをキャッシュに保存
  /// </summary>
  protected void SetCache<T>(string key, T data, TimeSpan? expiry = null)
  {
    cache.Set($"{repositoryName}_{key}", data, expiry ?? TimeSpan.FromMinutes(10));
  }

  /// <summary>
  /// キャッシュからデータを取得
  /// </summary>
  protected T GetCache<T>(string key) where T : class
  {
    return cache.Get<T>($"{repositoryName}_{key}");
  }

  /// <summary>
  /// キャッシュが有効かチェック
  /// </summary>
  protected bool IsCacheValid(string key)
  {
    return cache.IsValid($"{repositoryName}_{key}");
  }

  /// <summary>
  /// キャッシュを無効化
  /// </summary>
  protected void InvalidateCache(string key)
  {
    cache.Remove($"{repositoryName}_{key}");
  }

  #endregion

  #region Data Loading Strategy

  /// <summary>
  /// データ取得の統一メソッド
  /// 1. キャッシュチェック
  /// 2. オンライン時はAPI取得
  /// 3. オフライン時はローカル取得
  /// 4. フォールバック処理
  /// </summary>
  protected async Task<T> GetDataAsync<T>(string key, Func<Task<T>> apiFetcher, Func<T> localFetcher = null) where T : class
  {
    // 1. キャッシュから取得を試行
    var cachedData = GetCache<T>(key);
    if (cachedData != null && IsCacheValid(key))
    {
      Debug.Log($"[{repositoryName}] Cache hit for {key}");
      return cachedData;
    }

    // 2. オンライン時はAPIから取得
    if (IsOnline())
    {
      try
      {
        var apiData = await apiFetcher();
        if (apiData != null)
        {
          SetCache(key, apiData);
          Debug.Log($"[{repositoryName}] API data loaded for {key}");
          return apiData;
        }
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[{repositoryName}] API fetch failed for {key}: {ex.Message}");
      }
    }

    // 3. ローカルストレージから取得
    if (localFetcher != null)
    {
      try
      {
        var localData = localFetcher();
        if (localData != null)
        {
          SetCache(key, localData, TimeSpan.FromHours(1)); // 短い有効期限
          Debug.Log($"[{repositoryName}] Local data loaded for {key}");
          return localData;
        }
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[{repositoryName}] Local fetch failed for {key}: {ex.Message}");
      }
    }

    // 4. フォールバック - 古いキャッシュを返す
    if (cachedData != null)
    {
      Debug.LogWarning($"[{repositoryName}] Using stale cache for {key}");
      return cachedData;
    }

    // 5. 全て失敗
    Debug.LogError($"[{repositoryName}] Failed to load data for {key}");
    return null;
  }

  #endregion

  #region Utility Methods

  protected bool IsOnline()
  {
    return Application.internetReachability != NetworkReachability.NotReachable;
  }

  protected string GetLocalKey(string key)
  {
    return $"{repositoryName}_{key}";
  }

  /// <summary>
  /// エラー処理の統一メソッド
  /// </summary>
  protected void HandleError(string operation, Exception ex)
  {
    Debug.LogError($"[{repositoryName}] {operation} failed: {ex.Message}");

    // エラー報告（Analytics等に送信）
    // AnalyticsManager.ReportError($"{repositoryName}_{operation}", ex);
  }

  #endregion

  #region Abstract Methods

  /// <summary>
  /// 全データのリフレッシュ（継承先で実装）
  /// </summary>
  public abstract Task RefreshAllAsync();

  /// <summary>
  /// ローカル保存（継承先で実装）
  /// </summary>
  public abstract void SaveToLocal();

  /// <summary>
  /// ローカル読み込み（継承先で実装）
  /// </summary>
  public abstract void LoadFromLocal();

  #endregion
}

/// <summary>
/// データキャッシュクラス
/// </summary>
public class DataCache
{
  private readonly Dictionary<string, CacheEntry> cache = new();

  public void Set<T>(string key, T data, TimeSpan expiry)
  {
    cache[key] = new CacheEntry(data, DateTime.Now + expiry);
  }

  public T Get<T>(string key) where T : class
  {
    if (cache.TryGetValue(key, out var entry) && !entry.IsExpired)
    {
      return entry.Data as T;
    }
    return null;
  }

  public bool IsValid(string key)
  {
    return cache.TryGetValue(key, out var entry) && !entry.IsExpired;
  }

  public void Remove(string key)
  {
    cache.Remove(key);
  }

  public void Clear()
  {
    cache.Clear();
  }
}

/// <summary>
/// キャッシュエントリ
/// </summary>
public class CacheEntry
{
  public object Data { get; }
  public DateTime ExpiryTime { get; }
  public bool IsExpired => DateTime.Now > ExpiryTime;

  public CacheEntry(object data, DateTime expiryTime)
  {
    Data = data;
    ExpiryTime = expiryTime;
  }
}