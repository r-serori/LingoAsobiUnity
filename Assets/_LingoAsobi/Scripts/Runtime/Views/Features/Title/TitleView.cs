using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;
using UnityEngine.SceneManagement;

namespace Scripts.Runtime.Views.Features.Title
{
  public class TitleView : BaseView
  {
    [Header("Title View References")]
    [SerializeField] private Button continueDataButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button noticeButton;
    [SerializeField] private Button startButton;

    private UserProfile currentUser;

    #region Initialization

    protected override void GetUIReferences()
    {
      base.GetUIReferences();

      // UI要素の参照を追加で取得（必要に応じて）
    }

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      SetupButtons();
    }

    #endregion

    protected override void UpdateDisplay()
    {
        base.UpdateDisplay();
        
        // UIの初期化
        SetupButtons();
    }

    private void SetupButtons()
    {
      continueDataButton?.onClick.AddListener(OnContinueDataButtonClicked);
      settingsButton?.onClick.AddListener(OnSettingsButtonClicked);
      noticeButton?.onClick.AddListener(OnNoticeButtonClicked);
      startButton?.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnContinueDataButtonClicked()
    {
      Debug.Log("[TitleView] OnContinueDataButtonClicked");
    }

    private void OnSettingsButtonClicked()
    {
      Debug.Log("[TitleView] OnSettingsButtonClicked");
    }

    private void OnNoticeButtonClicked()
    {
      Debug.Log("[TitleView] OnNoticeButtonClicked");
    }

    private void OnStartButtonClicked()
    {
      Debug.Log("[TitleView] OnStartButtonClicked");
      SceneManager.LoadScene("HomeScene");
    }
  }
}