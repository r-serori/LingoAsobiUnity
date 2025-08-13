using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Runtime.Data.Repositories.Base
{
  /// <summary>
  /// リポジトリパターンの基底インターフェース
  /// データアクセス層を抽象化し、ビジネスロジックとデータ取得を分離
  /// </summary>
  /// <typeparam name="T">エンティティの型</typeparam>
  public interface IRepository<T> where T : class
  {
    /// <summary>
    /// IDによる単一エンティティの取得
    /// </summary>
    /// <param name="id">エンティティのID</param>
    /// <returns>エンティティ、見つからない場合はnull</returns>
    Task<T> GetByIdAsync(string id);

    /// <summary>
    /// すべてのエンティティを取得
    /// </summary>
    /// <returns>エンティティのリスト</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// エンティティの作成
    /// </summary>
    /// <param name="entity">作成するエンティティ</param>
    /// <returns>作成されたエンティティ</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// エンティティの更新
    /// </summary>
    /// <param name="entity">更新するエンティティ</param>
    /// <returns>更新の成功可否</returns>
    Task<bool> UpdateAsync(T entity);

    /// <summary>
    /// エンティティの削除
    /// </summary>
    /// <param name="id">削除するエンティティのID</param>
    /// <returns>削除の成功可否</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 条件による検索
    /// </summary>
    /// <param name="predicate">検索条件</param>
    /// <returns>条件に一致するエンティティのリスト</returns>
    Task<List<T>> FindAsync(Func<T, bool> predicate);

    /// <summary>
    /// キャッシュのクリア
    /// </summary>
    void ClearCache();
  }
}