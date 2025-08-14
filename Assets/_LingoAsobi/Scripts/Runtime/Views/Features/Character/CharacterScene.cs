using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Data.Repositories;

namespace Scripts.Runtime.Views.Features.Character
{
  /// <summary>
  /// キャラクター画面のシーン管理クラス
  /// </summary>
  public class CharacterScene : BaseScene
  {
    [Header("Character Scene References")]
    [SerializeField] private CharacterListView characterListView;
    [SerializeField] private CharacterDetailView characterDetailView;
    [SerializeField] private Button backButton;
    [SerializeField] private Toggle showAllToggle;
    [SerializeField] private Toggle showUnlockedToggle;
    [SerializeField] private Toggle showFavoriteToggle;

    private CharacterData selectedCharacter;

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();

      // キャラクターリストを初期化
      if (characterListView != null)
      {
        await characterListView.LoadCharactersAsync();
      }
    }

    protected override void InitializeViews()
    {
      base.InitializeViews();

      // ボタンのイベントを設定
      if (backButton != null)
        backButton.onClick.AddListener(() => _ = OnBackButtonPressed());

      // トグルのイベントを設定
      if (showAllToggle != null)
        showAllToggle.onValueChanged.AddListener(OnFilterChanged);

      if (showUnlockedToggle != null)
        showUnlockedToggle.onValueChanged.AddListener(OnFilterChanged);

      if (showFavoriteToggle != null)
        showFavoriteToggle.onValueChanged.AddListener(OnFilterChanged);

      // リストビューのイベントを設定
      if (characterListView != null)
      {
        characterListView.OnCharacterSelected += OnCharacterSelected;
      }
    }

    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // デフォルトでリストビューを表示
      await ShowViewAsync<CharacterListView>();
    }

    #endregion

    #region Character Selection

    /// <summary>
    /// キャラクターが選択された時の処理
    /// </summary>
    private async void OnCharacterSelected(CharacterData character)
    {
      selectedCharacter = character;

      Debug.Log($"[CharacterScene] Character selected: {character.characterName}");

      // 詳細ビューを表示
      if (characterDetailView != null)
      {
        characterDetailView.SetCharacter(character);
        await ShowViewAsync<CharacterDetailView>();
      }
    }

    /// <summary>
    /// リストビューに戻る
    /// </summary>
    public async Task BackToListView()
    {
      await ShowViewAsync<CharacterListView>();
    }

    #endregion

    #region Filter

    /// <summary>
    /// フィルター変更時の処理
    /// </summary>
    private void OnFilterChanged(bool value)
    {
      if (!value) return; // トグルがオフになった場合は無視

      CharacterListView.FilterType filter = CharacterListView.FilterType.All;

      if (showAllToggle != null && showAllToggle.isOn)
        filter = CharacterListView.FilterType.All;
      else if (showUnlockedToggle != null && showUnlockedToggle.isOn)
        filter = CharacterListView.FilterType.Unlocked;
      else if (showFavoriteToggle != null && showFavoriteToggle.isOn)
        filter = CharacterListView.FilterType.Favorite;

      if (characterListView != null)
      {
        characterListView.ApplyFilter(filter);
      }
    }

    #endregion

    #region Event Handling

    protected override void SubscribeToEvents()
    {
      base.SubscribeToEvents();

      // キャラクター関連のイベントを購読
      EventBus.Instance.Subscribe<SceneTransitionEvent>(OnSceneTransition);
    }

    protected override void UnsubscribeFromEvents()
    {
      base.UnsubscribeFromEvents();

      EventBus.Instance.Unsubscribe<SceneTransitionEvent>(OnSceneTransition);
    }

    private void OnSceneTransition(SceneTransitionEvent e)
    {
      // キャラクター解放アイテムの場合
      if (e.SceneName.StartsWith("character_unlock_"))
      {
        string characterId = e.SceneName.Replace("character_unlock_", "");
        _ = UnlockCharacter(characterId);
      }
    }

    /// <summary>
    /// キャラクターを解放
    /// </summary>
    private async Task UnlockCharacter(string characterId)
    {
      var repository = CharacterRepository.Instance;
      bool success = await repository.UnlockCharacterAsync(characterId);

      if (success)
      {
        // リストを更新
        if (characterListView != null)
        {
          await characterListView.RefreshAsync();
        }

        // 解放演出を表示
        ShowCharacterUnlockEffect(characterId);
      }
    }

    /// <summary>
    /// キャラクター解放演出
    /// </summary>
    private void ShowCharacterUnlockEffect(string characterId)
    {
      // TODO: 解放演出を実装
      Debug.Log($"[CharacterScene] Character unlocked: {characterId}");
    }

    #endregion
  }
}