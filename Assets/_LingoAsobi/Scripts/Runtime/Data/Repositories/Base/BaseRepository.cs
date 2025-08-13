using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Cache;
using Scripts.Runtime.Core;

namespace Scripts.Runtime.Data.Repositories.Base
{
  /// <summary>
  /// リポジトリパターンの基底実装クラス
  /// キャッシング機能とAPIクライアントへのアクセスを提供
  /// </summary>
  /// <typeparam name="T">エンティティの型</typeparam>
  public abstract class BaseRepository<T> : IRepository<T> where T : class
  {
    // キャッシュマネージャー
    protected readonly DataCache _cache;

    // APIクライアント（将来的に使用）
    protected readonly APIClient _apiClient;

    // エンドポイントのベースURL
    protected abstract string EndpointUrl { get; }

    // キャッシュキーのプレフィックス
    protected abstract string CacheKeyPrefix { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public BaseRepository()
    {
      _cache = DataCache.Instance;
      _apiClient = APIClient.Instance;
    }

    /// <summary>
    /// IDによる単一エンティティの取得
    /// </summary>
    public virtual async Task<T> GetByIdAsync(string id)
    {
      // キャッシュキーの生成
      string cacheKey = $"{CacheKeyPrefix}_{id}";

      // キャッシュから取得を試みる
      var cached = _cache.Get<T>(cacheKey);
      if (cached != null)
      {
        Debug.Log($"[{GetType().Name}] Cache hit for key: {cacheKey}");
        return cached;
      }

      // MockDataを使用（本番環境ではAPIから取得）
      var entity = await GetMockDataByIdAsync(id);

      /* 本番用コード（コメントアウト）
      try
      {
          var response = await _apiClient.GetAsync<T>($"{EndpointUrl}/{id}");
          if (response != null)
          {
              _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
          }
          return response;
      }
      catch (Exception e)
      {
          Debug.LogError($"[{GetType().Name}] API Error: {e.Message}");
          return null;
      }
      */

      // キャッシュに保存
      if (entity != null)
      {
        _cache.Set(cacheKey, entity, TimeSpan.FromMinutes(5));
      }

      return entity;
    }

    /// <summary>
    /// すべてのエンティティを取得
    /// </summary>
    public virtual async Task<List<T>> GetAllAsync()
    {
      string cacheKey = $"{CacheKeyPrefix}_all";

      var cached = _cache.Get<List<T>>(cacheKey);
      if (cached != null)
      {
        Debug.Log($"[{GetType().Name}] Cache hit for all entities");
        return cached;
      }

      // MockDataを使用
      var entities = await GetAllMockDataAsync();

      /* 本番用コード（コメントアウト）
      try
      {
          var response = await _apiClient.GetAsync<List<T>>(EndpointUrl);
          if (response != null)
          {
              _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
          }
          return response ?? new List<T>();
      }
      catch (Exception e)
      {
          Debug.LogError($"[{GetType().Name}] API Error: {e.Message}");
          return new List<T>();
      }
      */

      if (entities != null && entities.Count > 0)
      {
        _cache.Set(cacheKey, entities, TimeSpan.FromMinutes(5));
      }

      return entities ?? new List<T>();
    }

    /// <summary>
    /// エンティティの作成
    /// </summary>
    public virtual async Task<T> CreateAsync(T entity)
    {
      // MockDataでは新規作成をシミュレート
      await Task.Delay(100); // ネットワーク遅延をシミュレート

      /* 本番用コード（コメントアウト）
      try
      {
          var response = await _apiClient.PostAsync<T>(EndpointUrl, entity);
          if (response != null)
          {
              ClearCache(); // キャッシュをクリア
          }
          return response;
      }
      catch (Exception e)
      {
          Debug.LogError($"[{GetType().Name}] API Error: {e.Message}");
          return null;
      }
      */

      Debug.Log($"[{GetType().Name}] Mock: Entity created");
      ClearCache();
      return entity;
    }

    /// <summary>
    /// エンティティの更新
    /// </summary>
    public virtual async Task<bool> UpdateAsync(T entity)
    {
      // MockDataでは常に成功
      await Task.Delay(100);

      /* 本番用コード（コメントアウト）
      try
      {
          var response = await _apiClient.PutAsync<T>(EndpointUrl, entity);
          if (response != null)
          {
              ClearCache();
              return true;
          }
          return false;
      }
      catch (Exception e)
      {
          Debug.LogError($"[{GetType().Name}] API Error: {e.Message}");
          return false;
      }
      */

      Debug.Log($"[{GetType().Name}] Mock: Entity updated");
      ClearCache();
      return true;
    }

    /// <summary>
    /// エンティティの削除
    /// </summary>
    public virtual async Task<bool> DeleteAsync(string id)
    {
      // MockDataでは常に成功
      await Task.Delay(100);

      /* 本番用コード（コメントアウト）
      try
      {
          var success = await _apiClient.DeleteAsync($"{EndpointUrl}/{id}");
          if (success)
          {
              ClearCache();
          }
          return success;
      }
      catch (Exception e)
      {
          Debug.LogError($"[{GetType().Name}] API Error: {e.Message}");
          return false;
      }
      */

      Debug.Log($"[{GetType().Name}] Mock: Entity deleted - ID: {id}");
      ClearCache();
      return true;
    }

    /// <summary>
    /// 条件による検索
    /// </summary>
    public virtual async Task<List<T>> FindAsync(Func<T, bool> predicate)
    {
      var allEntities = await GetAllAsync();
      return allEntities.Where(predicate).ToList();
    }

    /// <summary>
    /// キャッシュのクリア
    /// </summary>
    public virtual void ClearCache()
    {
      _cache.ClearByPrefix(CacheKeyPrefix);
      Debug.Log($"[{GetType().Name}] Cache cleared");
    }

    #region Mock Data Methods (開発用)

    /// <summary>
    /// MockDataからIDで取得（サブクラスで実装）
    /// </summary>
    protected abstract Task<T> GetMockDataByIdAsync(string id);

    /// <summary>
    /// MockDataから全件取得（サブクラスで実装）
    /// </summary>
    protected abstract Task<List<T>> GetAllMockDataAsync();

    #endregion
  }
}