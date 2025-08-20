using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Data.Repositories.Base;

namespace Scripts.Runtime.Data.Repositories
{
  /// <summary>
  /// ユーザーデータのリポジトリクラス
  /// ユーザープロファイルのCRUD操作を管理
  /// </summary>
  public class UserRepository : BaseRepository<UserProfile>
  {
    // APIエンドポイント
    protected override string EndpointUrl => "/api/users";

    // キャッシュキーのプレフィックス
    protected override string CacheKeyPrefix => "user";

    // シングルトンインスタンス
    private static readonly UserRepository _instance;
    public static UserRepository Instance
    {
      get
      {
        return _instance ?? new UserRepository();
      }
    }

    // 現在のユーザープロファイル（キャッシュ用）
    private UserProfile _currentUserProfile;

    /// <summary>
    /// プライベートコンストラクタ（シングルトン）
    /// </summary>
    private UserRepository() : base()
    {
    }

    /// <summary>
    /// 現在のユーザープロファイルを取得
    /// </summary>
    public async Task<UserProfile> GetCurrentUserAsync()
    {
      try
      {
        if (_currentUserProfile != null)
        {
          return _currentUserProfile;
        }

        // PlayerPrefsから保存されたユーザーIDを取得
        string savedUserId = PlayerPrefs.GetString("CurrentUserId", "mock_user_001");

        // 無限ループを防ぐため、直接Mockデータを取得
        _currentUserProfile = await GetMockUserProfileAsync();

        if (_currentUserProfile != null)
        {
        }
        else
        {
          Debug.LogWarning("[UserRepository] Failed to load user profile");
        }

        return _currentUserProfile;
      }
      catch (Exception e)
      {
        Debug.LogError($"[UserRepository] GetCurrentUserAsync error: {e.Message}");
        Debug.LogError($"[UserRepository] Stack trace: {e.StackTrace}");

        // エラーが発生した場合は、デフォルトのMockデータを返す
        try
        {
          _currentUserProfile = await GetMockUserProfileAsync();
          return _currentUserProfile;
        }
        catch (Exception fallbackError)
        {
          Debug.LogError($"[UserRepository] Fallback error: {fallbackError.Message}");
          return null;
        }
      }
    }

    /// <summary>
    /// ログイン処理
    /// </summary>
    public async Task<UserProfile> LoginAsync(string email, string password)
    {
      // MockDataではダミーログイン処理
      await Task.Delay(500); // ネットワーク遅延をシミュレート

      /* 本番用コード（コメントアウト）
      try
      {
          var loginRequest = new { email = email, password = password };
          var response = await _apiClient.PostAsync<UserProfile>("/api/auth/login", loginRequest);

          if (response != null)
          {
              _currentUserProfile = response;
              PlayerPrefs.SetString("CurrentUserId", response.userId);
              PlayerPrefs.Save();

              // キャッシュに保存
              string cacheKey = $"{CacheKeyPrefix}_{response.userId}";
              _cache.Set(cacheKey, response, TimeSpan.FromMinutes(30));

              return response;
          }
          return null;
      }
      catch (Exception e)
      {
          Debug.LogError($"Login failed: {e.Message}");
          return null;
      }
      */

      // Mockデータを返す
      var mockUser = await GetMockUserProfileAsync();
      _currentUserProfile = mockUser;
      PlayerPrefs.SetString("CurrentUserId", mockUser.userId);
      PlayerPrefs.Save();

      return mockUser;
    }

    /// <summary>
    /// ログアウト処理
    /// </summary>
    public void Logout()
    {
      _currentUserProfile = null;
      ClearCache();
      PlayerPrefs.DeleteKey("CurrentUserId");
      PlayerPrefs.Save();

    }

    /// <summary>
    /// スタミナを消費
    /// </summary>
    public async Task<bool> ConsumeStaminaAsync(int amount)
    {
      var user = await GetCurrentUserAsync();
      if (user == null) return false;

      bool success = user.ConsumeStamina(amount);

      if (success)
      {
        // APIで更新を送信（Mockでは省略）
        await UpdateAsync(user);
      }

      return success;
    }

    /// <summary>
    /// 経験値を追加
    /// </summary>
    public async Task<bool> AddExperienceAsync(int amount)
    {
      var user = await GetCurrentUserAsync();
      if (user == null) return false;

      bool leveledUp = user.AddExperience(amount);

      // APIで更新を送信（Mockでは省略）
      await UpdateAsync(user);

      return leveledUp;
    }

    #region Mock Data Implementation

    /// <summary>
    /// MockDataからユーザーを取得
    /// </summary>
    protected override async Task<UserProfile> GetMockDataByIdAsync(string id)
    {
      try
      {

        await Task.Delay(100); // ネットワーク遅延をシミュレート

        // ScriptableObjectからMockDataを読み込む場合はここに実装
        // 今回はコードで直接生成
        var mockUser = await GetMockUserProfileAsync();

        return mockUser;
      }
      catch (Exception e)
      {
        Debug.LogError($"[UserRepository] GetMockDataByIdAsync error: {e.Message}");
        return null;
      }
    }

    /// <summary>
    /// MockDataから全ユーザーを取得
    /// </summary>
    protected override async Task<List<UserProfile>> GetAllMockDataAsync()
    {
      await Task.Delay(100);

      var users = new List<UserProfile>
            {
                await GetMockUserProfileAsync(),
                CreateMockUser("mock_user_002", "Player2", 5, 1000, 50),
                CreateMockUser("mock_user_003", "Player3", 10, 5000, 100)
            };

      return users;
    }

    /// <summary>
    /// Mockユーザープロファイルを生成
    /// </summary>
    private async Task<UserProfile> GetMockUserProfileAsync()
    {
      try
      {

        await Task.Yield(); // 非同期コンテキストを維持

        var mockUser = new UserProfile
        {
          userId = "mock_user_001",
          userName = "Ryuno",
          email = "test@example.com",
          userIconUrl = "Characters/ex_character1",
          level = 99,
          exp = 15000,
          nextLevelExp = 5000,
          gold = 99999,
          gem = 999,
          stamina = 150,
          maxStamina = 150,
          staminaRecoverySeconds = 180,
          lastStaminaUpdateTime = DateTime.Now.AddMinutes(-10),
          favoriteCharacterId = "char_001",
          enableCharacterAnimation = true,
          preferredImageQuality = ImageQuality.High,
          enableImagePreloading = true,
          enableSoundEffects = true,
          enableBGM = true,
          soundVolume = 1.0f,
          bgmVolume = 0.7f,
          createdAt = DateTime.Now.AddDays(-30),
          lastLoginAt = DateTime.Now,
          updatedAt = DateTime.Now
        };

        return mockUser;
      }
      catch (Exception e)
      {
        Debug.LogError($"[UserRepository] GetMockUserProfileAsync error: {e.Message}");
        return null;
      }
    }

    /// <summary>
    /// Mockユーザーを作成（テスト用）
    /// </summary>
    private UserProfile CreateMockUser(string id, string name, int level, int gold, int gem)
    {
      return new UserProfile
      {
        userId = id,
        userName = name,
        email = $"{name.ToLower()}@example.com",
        level = level,
        gold = gold,
        gem = gem,
        stamina = 100,
        maxStamina = 100 + (level * 5),
        favoriteCharacterId = "char_001"
      };
    }

    protected override async Task<UserProfile> GetMockDataAsync()
    {
      // return await GetMockUserProfileAsync();
      return null;
    }

    #endregion
  }
}