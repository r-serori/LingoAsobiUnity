using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.Views.Shared;

namespace Scripts.Runtime.Views.Shared.Header
{
  /// <summary>
  /// プロフィール画像表示コンポーネント
  /// Userデータからプロフィール画像を表示し、最適化された画像読み込みを提供
  /// </summary>
  public class ProfileImageView : OptimizedImageView
  {
    [Header("Profile Settings")]
    [SerializeField]
    [Tooltip("プロフィール画像が見つからない場合のデフォルト画像パス")]
    private string defaultProfileImagePath = "Characters/default_profile";

    [SerializeField]
    [Tooltip("プロフィール画像を円形にマスクするかどうか")]
    private bool useCircularMask = true;

    [SerializeField]
    [Tooltip("読み込み中表示用のスピナー（オプション）")]
    private GameObject loadingSpinner;

    private User currentUser;
    private Image imageComponent;

    protected override void Awake()
    {
      base.Awake(); // OptimizedImageViewの初期化

      imageComponent = GetComponent<Image>();
      if (imageComponent == null)
      {
        Debug.LogError($"ProfileImageView: Image コンポーネントが見つかりません: {gameObject.name}");
      }

      // 円形マスク設定
      if (useCircularMask && imageComponent != null)
      {
        ApplyCircularMask();
      }
    }

    /// <summary>
    /// ユーザー情報を設定してプロフィール画像を表示
    /// </summary>
    /// <param name="user">表示するユーザー情報</param>
    public void SetUser(User user)
    {
      if (user == null)
      {
        Debug.LogWarning("ProfileImageView: User が null です。デフォルト画像を表示します。");
        LoadDefaultImage();
        return;
      }

      currentUser = user;

      // ユーザーアイコンのパスをチェック
      string imagePath = !string.IsNullOrEmpty(user.userIconUrl)
          ? user.userIconUrl
          : defaultProfileImagePath;

      // OptimizedImageViewの機能を使用して画像読み込み
      SetImageWithFallback(imagePath);

      Debug.Log($"ProfileImageView: ユーザー画像読み込み開始 - {user.userName} ({imagePath})");
    }

    /// <summary>
    /// フォールバック機能付きで画像を設定
    /// </summary>
    private async void SetImageWithFallback(string imagePath)
    {
      try
      {
        // ローディング表示
        ShowLoading(true);

        // OptimizedImageViewの機能を使用
        SetImage(imagePath);

        // 読み込み完了を少し待ってからローディングを非表示
        // （OptimizedImageViewが内部でローディング管理している場合はそちらを使用）
        await System.Threading.Tasks.Task.Delay(100);

        // 画像読み込み失敗時のチェック
        if (imageComponent.sprite == null && imagePath != defaultProfileImagePath)
        {
          Debug.LogWarning($"ProfileImageView: 画像読み込み失敗、デフォルト画像を表示: {imagePath}");
          SetImage(defaultProfileImagePath);
        }
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"ProfileImageView: 画像読み込みエラー: {ex.Message}");
        LoadDefaultImage();
      }
      finally
      {
        ShowLoading(false);
      }
    }

    /// <summary>
    /// デフォルト画像を読み込み
    /// </summary>
    public void LoadDefaultImage()
    {
      currentUser = null;
      SetImage(defaultProfileImagePath);
      Debug.Log("ProfileImageView: デフォルト画像を表示");
    }

    /// <summary>
    /// 現在のユーザー情報を更新（画像再読み込みなし）
    /// </summary>
    /// <param name="user">更新するユーザー情報</param>
    public void UpdateUser(User user)
    {
      if (currentUser != null && user != null &&
          currentUser.userIconUrl == user.userIconUrl)
      {
        // 画像パスが同じ場合は再読み込みしない
        currentUser = user;
        return;
      }

      SetUser(user);
    }

    /// <summary>
    /// 円形マスクを適用
    /// </summary>
    private void ApplyCircularMask()
    {
      if (imageComponent != null)
      {
        // Image の Type を Filled に設定して円形に
        imageComponent.type = Image.Type.Filled;
        imageComponent.fillMethod = Image.FillMethod.Radial360;

        // または、Maskコンポーネントを使用する場合
        var mask = GetComponent<Mask>();
        if (mask == null)
        {
          mask = gameObject.AddComponent<Mask>();
          mask.showMaskGraphic = false;
        }
      }
    }

    /// <summary>
    /// ローディング表示の制御
    /// </summary>
    private void ShowLoading(bool show)
    {
      if (loadingSpinner != null)
      {
        loadingSpinner.SetActive(show);
      }
    }

    /// <summary>
    /// プロフィール画像を強制リフレッシュ
    /// </summary>
    [ContextMenu("Refresh Profile Image")]
    public void RefreshProfileImage()
    {
      if (currentUser != null)
      {
        SetUser(currentUser);
      }
      else
      {
        LoadDefaultImage();
      }
    }

    /// <summary>
    /// デバッグ情報表示
    /// </summary>
    [ContextMenu("Show Debug Info")]
    private void ShowDebugInfo()
    {
      if (currentUser != null)
      {
        Debug.Log($"ProfileImageView Debug:" +
                 $"\n- User: {currentUser.userName}" +
                 $"\n- Icon Path: {currentUser.userIconUrl}" +
                 $"\n- Current Sprite: {(imageComponent?.sprite?.name ?? "None")}" +
                 $"\n- Default Path: {defaultProfileImagePath}");
      }
      else
      {
        Debug.Log("ProfileImageView Debug: No user set");
      }
    }
  }
}