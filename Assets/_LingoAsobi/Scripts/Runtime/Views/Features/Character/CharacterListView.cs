using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.Character;
using Scripts.Runtime.Data.Repositories;

namespace Scripts.Runtime.Views.Features.Character
{
  /// <summary>
  /// キャラクターリスト表示View
  /// </summary>
  public class CharacterListView : BaseView
  {
    [Header("List UI")]
    [SerializeField] private Transform characterListContainer;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Sort & Filter")]
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private TextMeshProUGUI totalCountText;
    [SerializeField] private TextMeshProUGUI unlockedCountText;

    [Header("Empty State")]
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private TextMeshProUGUI emptyStateMessage;

    // イベント
    public event Action<CharacterData> OnCharacterSelected;

    // データ
    private List<CharacterData> allCharacters = new List<CharacterData>();
    private List<CharacterData> filteredCharacters = new List<CharacterData>();
    private List<CharacterListItem> listItems = new List<CharacterListItem>();

    // フィルタータイプ
    public enum FilterType
    {
      All,
      Unlocked,
      Locked,
      Favorite,
      ByRarity
    }

    // ソートタイプ
    public enum SortType
    {
      Name,
      Level,
      Rarity,
      UnlockDate
    }

    private FilterType currentFilter = FilterType.All;
    private SortType currentSort = SortType.Name;

    #region Initialization

    protected override void SetupEventListeners()
    {
      base.SetupEventListeners();

      if (sortDropdown != null)
      {
        sortDropdown.onValueChanged.AddListener(OnSortChanged);
      }
    }

    #endregion

    #region Data Loading

    /// <summary>
    /// キャラクターデータを読み込む
    /// </summary>
    public async Task LoadCharactersAsync()
    {
      ShowLoading(true);

      try
      {
        allCharacters = await DataManager.Instance.GetAllCharactersAsync();

        ApplyFilter(currentFilter);
        ApplySort(currentSort);

        UpdateDisplay();
        UpdateCounters();
      }
      catch (Exception e)
      {
        Debug.LogError($"[CharacterListView] Failed to load characters: {e.Message}");
        ShowError("キャラクターの読み込みに失敗しました");
      }
      finally
      {
        ShowLoading(false);
      }
    }

    protected override async Task LoadDataAsync()
    {
      await LoadCharactersAsync();
    }

    #endregion

    #region Display

    protected override void UpdateDisplay()
    {
      // 既存のリストアイテムをクリア
      ClearListItems();

      if (filteredCharacters.Count == 0)
      {
        ShowEmptyState(true);
        return;
      }

      ShowEmptyState(false);

      // キャラクターアイテムを生成
      foreach (var character in filteredCharacters)
      {
        CreateListItem(character);
      }
    }

    /// <summary>
    /// リストアイテムを生成
    /// </summary>
    private void CreateListItem(CharacterData character)
    {
      if (characterItemPrefab == null || characterListContainer == null) return;

      GameObject itemObj = Instantiate(characterItemPrefab, characterListContainer);
      CharacterListItem listItem = itemObj.GetComponent<CharacterListItem>();

      if (listItem == null)
      {
        listItem = itemObj.AddComponent<CharacterListItem>();
      }

      listItem.SetData(character);
      listItem.OnClicked += () => OnCharacterItemClicked(character);

      listItems.Add(listItem);
    }

    /// <summary>
    /// リストアイテムをクリア
    /// </summary>
    private void ClearListItems()
    {
      foreach (var item in listItems)
      {
        if (item != null)
        {
          Destroy(item.gameObject);
        }
      }

      listItems.Clear();
    }

    /// <summary>
    /// カウンターを更新
    /// </summary>
    private void UpdateCounters()
    {
      if (totalCountText != null)
      {
        totalCountText.text = $"Total: {allCharacters.Count}";
      }

      if (unlockedCountText != null)
      {
        int unlockedCount = allCharacters.Count(c => c.isUnlocked);
        unlockedCountText.text = $"Unlocked: {unlockedCount}/{allCharacters.Count}";
      }
    }

    /// <summary>
    /// 空状態を表示
    /// </summary>
    private void ShowEmptyState(bool show)
    {
      if (emptyStatePanel != null)
      {
        emptyStatePanel.SetActive(show);
      }

      if (show && emptyStateMessage != null)
      {
        string message = currentFilter switch
        {
          FilterType.Unlocked => "解放済みのキャラクターがいません",
          FilterType.Locked => "未解放のキャラクターがいません",
          FilterType.Favorite => "お気に入りのキャラクターがいません",
          _ => "キャラクターが見つかりません"
        };

        emptyStateMessage.text = message;
      }
    }

    #endregion

    #region Filter & Sort

    /// <summary>
    /// フィルターを適用
    /// </summary>
    public void ApplyFilter(FilterType filter)
    {
      currentFilter = filter;

      filteredCharacters = filter switch
      {
        FilterType.All => new List<CharacterData>(allCharacters),
        FilterType.Unlocked => allCharacters.Where(c => c.isUnlocked).ToList(),
        FilterType.Locked => allCharacters.Where(c => !c.isUnlocked).ToList(),
        FilterType.Favorite => allCharacters.Where(c => c.isFavorite && c.isUnlocked).ToList(),
        _ => new List<CharacterData>(allCharacters)
      };

      ApplySort(currentSort);
      UpdateDisplay();
    }

    /// <summary>
    /// ソートを適用
    /// </summary>
    public void ApplySort(SortType sort)
    {
      currentSort = sort;

      filteredCharacters = sort switch
      {
        SortType.Name => filteredCharacters.OrderBy(c => c.characterName).ToList(),
        SortType.Level => filteredCharacters.OrderByDescending(c => c.level).ToList(),
        SortType.Rarity => filteredCharacters.OrderByDescending(c => c.rarity).ThenBy(c => c.characterName).ToList(),
        SortType.UnlockDate => filteredCharacters.OrderByDescending(c => c.unlockedDate).ToList(),
        _ => filteredCharacters
      };

      UpdateDisplay();
    }

    /// <summary>
    /// ソート変更時の処理
    /// </summary>
    private void OnSortChanged(int index)
    {
      ApplySort((SortType)index);
    }

    #endregion

    #region Interaction

    /// <summary>
    /// キャラクターアイテムがクリックされた時
    /// </summary>
    private void OnCharacterItemClicked(CharacterData character)
    {
      Debug.Log($"[CharacterListView] Character clicked: {character.characterName}");
      OnCharacterSelected?.Invoke(character);
    }

    #endregion

    #region Character List Item

    /// <summary>
    /// キャラクターリストアイテムのコンポーネント
    /// </summary>
    [System.Serializable]
    public class CharacterListItem : MonoBehaviour
    {
      [Header("UI References")]
      public Image iconImage;
      public TextMeshProUGUI nameText;
      public TextMeshProUGUI levelText;
      public Image rarityImage;
      public GameObject lockIcon;
      public GameObject favoriteIcon;
      public Button button;

      public event Action OnClicked;

      private CharacterData characterData;

      private void Awake()
      {
        if (button == null)
        {
          button = GetComponent<Button>();
        }

        if (button != null)
        {
          button.onClick.AddListener(() => OnClicked?.Invoke());
        }
      }

      public void SetData(CharacterData data)
      {
        characterData = data;

        if (nameText != null)
          nameText.text = data.characterName;

        if (levelText != null)
          levelText.text = $"Lv.{data.level}";

        if (lockIcon != null)
          lockIcon.SetActive(!data.isUnlocked);

        if (favoriteIcon != null)
          favoriteIcon.SetActive(data.isFavorite);

        // レアリティに応じた色を設定
        if (rarityImage != null)
        {
          rarityImage.color = GetRarityColor(data.rarity);
        }

        // アイコン画像を読み込む
        // TODO: Addressables or Resources.Load
      }

      private Color GetRarityColor(Rarity rarity)
      {
        return rarity switch
        {
          Rarity.Common => Utilities.Constants.GameConstants.Colors.Common,
          Rarity.Uncommon => Utilities.Constants.GameConstants.Colors.Uncommon,
          Rarity.Rare => Utilities.Constants.GameConstants.Colors.Rare,
          Rarity.Epic => Utilities.Constants.GameConstants.Colors.Epic,
          Rarity.Legendary => Utilities.Constants.GameConstants.Colors.Legendary,
          _ => Color.white
        };
      }
    }

    #endregion
  }
}