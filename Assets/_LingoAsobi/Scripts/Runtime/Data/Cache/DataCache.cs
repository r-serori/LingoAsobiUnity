using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Runtime.Data.Cache
{
  /// <summary>
  /// データキャッシュマネージャー
  /// メモリ内でデータをキャッシュし、パフォーマンスを向上
  /// </summary>
  public class DataCache
  {
    // シングルトンインスタンス
    private static readonly DataCache _instance;
    public static DataCache Instance
    {
      get
      {
        return _instance ?? new DataCache();
      }
    }

    // キャッシュストレージ
    private readonly Dictionary<string, CacheEntry> _cache;

    // 最大キャッシュサイズ（エントリ数）
    private const int MaxCacheSize = 100;

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private DataCache()
    {
      _cache = new Dictionary<string, CacheEntry>();
    }

    /// <summary>
    /// データをキャッシュに保存
    /// </summary>
    /// <typeparam name="T">データの型</typeparam>
    /// <param name="key">キャッシュキー</param>
    /// <param name="data">保存するデータ</param>
    /// <param name="expiration">有効期限</param>
    public void Set<T>(string key, T data, TimeSpan? expiration = null)
    {
      if (string.IsNullOrEmpty(key))
      {
        Debug.LogError("[DataCache] Cache key cannot be null or empty");
        return;
      }

      // キャッシュサイズの制限チェック
      if (_cache.Count >= MaxCacheSize && !_cache.ContainsKey(key))
      {
        RemoveOldestEntry();
      }

      var entry = new CacheEntry
      {
        Data = data,
        ExpirationTime = expiration.HasValue
              ? DateTime.Now.Add(expiration.Value)
              : DateTime.MaxValue,
        LastAccessTime = DateTime.Now
      };

      _cache[key] = entry;
    }

    /// <summary>
    /// キャッシュからデータを取得
    /// </summary>
    /// <typeparam name="T">データの型</typeparam>
    /// <param name="key">キャッシュキー</param>
    /// <returns>キャッシュされたデータ、存在しない場合はdefault(T)</returns>
    public T Get<T>(string key)
    {
      if (string.IsNullOrEmpty(key))
      {
        return default(T);
      }

      if (_cache.TryGetValue(key, out var entry))
      {
        // 有効期限チェック
        if (entry.ExpirationTime < DateTime.Now)
        {
          _cache.Remove(key);
          return default(T);
        }

        // アクセス時間を更新
        entry.LastAccessTime = DateTime.Now;

        try
        {
          return (T)entry.Data;
        }
        catch (InvalidCastException)
        {
          Debug.LogError($"[DataCache] Type mismatch for key: {key}");
          return default(T);
        }
      }

      return default(T);
    }

    /// <summary>
    /// キャッシュにキーが存在するか確認
    /// </summary>
    public bool Contains(string key)
    {
      if (string.IsNullOrEmpty(key))
      {
        return false;
      }

      if (_cache.TryGetValue(key, out var entry))
      {
        return entry.ExpirationTime >= DateTime.Now;
      }

      return false;
    }

    /// <summary>
    /// 特定のキーのキャッシュを削除
    /// </summary>
    public void Remove(string key)
    {
      if (_cache.Remove(key))
      {
      }
    }

    /// <summary>
    /// プレフィックスに一致するキャッシュをクリア
    /// </summary>
    public void ClearByPrefix(string prefix)
    {
      var keysToRemove = _cache.Keys
          .Where(k => k.StartsWith(prefix))
          .ToList();

      foreach (var key in keysToRemove)
      {
        _cache.Remove(key);
      }

    }

    /// <summary>
    /// すべてのキャッシュをクリア
    /// </summary>
    public void ClearAll()
    {
      int count = _cache.Count;
      _cache.Clear();
    }

    /// <summary>
    /// 期限切れのエントリを削除
    /// </summary>
    public void RemoveExpiredEntries()
    {
      var expiredKeys = _cache
          .Where(kvp => kvp.Value.ExpirationTime < DateTime.Now)
          .Select(kvp => kvp.Key)
          .ToList();

      foreach (var key in expiredKeys)
      {
        _cache.Remove(key);
      }

      if (expiredKeys.Count > 0)
      {
      }
    }

    /// <summary>
    /// 最も古いエントリを削除（LRU）
    /// </summary>
    private void RemoveOldestEntry()
    {
      if (_cache.Count == 0) return;

      var oldestEntry = _cache
          .OrderBy(kvp => kvp.Value.LastAccessTime)
          .First();

      _cache.Remove(oldestEntry.Key);
    }

    /// <summary>
    /// キャッシュ統計情報を取得
    /// </summary>
    public CacheStatistics GetStatistics()
    {
      RemoveExpiredEntries();

      return new CacheStatistics
      {
        TotalEntries = _cache.Count,
        TotalSize = EstimateCacheSize(),
        OldestEntryAge = GetOldestEntryAge(),
        ExpiringEntriesCount = CountExpiringEntries(TimeSpan.FromMinutes(5))
      };
    }

    /// <summary>
    /// キャッシュサイズを推定（簡易実装）
    /// </summary>
    private long EstimateCacheSize()
    {
      // 簡易的なサイズ推定
      return _cache.Count * 1024; // 1エントリあたり1KB と仮定
    }

    /// <summary>
    /// 最も古いエントリの経過時間
    /// </summary>
    private TimeSpan GetOldestEntryAge()
    {
      if (_cache.Count == 0) return TimeSpan.Zero;

      var oldest = _cache.Values.Min(e => e.LastAccessTime);
      return DateTime.Now - oldest;
    }

    /// <summary>
    /// 指定時間内に期限切れになるエントリ数
    /// </summary>
    private int CountExpiringEntries(TimeSpan timeSpan)
    {
      var expirationThreshold = DateTime.Now.Add(timeSpan);
      return _cache.Values.Count(e =>
          e.ExpirationTime != DateTime.MaxValue &&
          e.ExpirationTime <= expirationThreshold);
    }
  }

  /// <summary>
  /// キャッシュエントリ
  /// </summary>
  public class CacheEntry
  {
    public object Data { get; set; }
    public DateTime ExpirationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
  }

  /// <summary>
  /// キャッシュ統計情報
  /// </summary>
  public class CacheStatistics
  {
    public int TotalEntries { get; set; }
    public long TotalSize { get; set; }
    public TimeSpan OldestEntryAge { get; set; }
    public int ExpiringEntriesCount { get; set; }
  }
}