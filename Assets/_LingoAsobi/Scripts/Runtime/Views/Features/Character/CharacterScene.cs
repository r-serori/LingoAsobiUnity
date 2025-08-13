// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
// using Scripts.Runtime.Data.Repositories;
// using Scripts.Runtime.Data.Models.Character;
// using Scripts.Runtime.Data.Models.User;
// using Scripts.Runtime.Data.Models.Shop;
// using Scripts.Runtime.Data.Models.Quest;
// using Scripts.Runtime.Data.Models.Inventory;
// using Scripts.Runtime.Core;

// namespace Scripts.Runtime.Views.Features.Character
// {
//   /// <summary>
//   /// キャラクター画面のメインコントローラー
//   /// </summary>
//   public class CharacterScene : MonoBehaviour
//   {
//     [Header("UI References")]
//     [SerializeField] private CharacterListView characterListView;
//     [SerializeField] private CharacterDetailView characterDetailView;
//     [SerializeField] private Button refreshButton;
//     [SerializeField] private InputField searchField;
//     [SerializeField] private Dropdown rarityFilter;
//     [SerializeField] private LoadingIndicator loadingIndicator;
//     [SerializeField] private ErrorDialog errorDialog;

//     [Header("Settings")]
//     [SerializeField] private bool autoRefreshOnStart = true;
//     [SerializeField] private float refreshInterval = 300f; // 5分

//     // データ参照
//     private CharacterRepository characterRepo;
//     private UserRepository userRepo;

//     // UI状態管理
//     private CharacterData[] currentCharacters;
//     private CharacterData selectedCharacter;
//     private bool isLoading;

//     // フィルタリング
//     private string currentSearchQuery = "";
//     private Rarity? currentRarityFilter;

//     #region Unity Lifecycle

//     private void Start()
//     {
//       InitializeReferences();
//       SetupEventListeners();
//       _ = InitializeSceneAsync();
//     }

//     private void OnDestroy()
//     {
//       RemoveEventListeners();
//     }

//     #endregion

//     #region Initialization

//     private void InitializeReferences()
//     {
//       // DataManagerから必要なリポジトリを取得
//       if (DataManager.Instance == null)
//       {
//         Debug.LogError("DataManager.Instance is null! Make sure DataManager is initialized.");
//         return;
//       }

//       characterRepo = DataManager.Instance.Characters;
//       userRepo = DataManager.Instance.Users;

//       if (characterRepo == null || userRepo == null)
//       {
//         Debug.LogError("Required repositories are null!");
//         return;
//       }
//     }

//     private void SetupEventListeners()
//     {
//       // UI イベント
//       if (refreshButton != null)
//         refreshButton.onClick.AddListener(OnRefreshButtonClicked);

//       if (searchField != null)
//         searchField.onValueChanged.AddListener(OnSearchValueChanged);

//       if (rarityFilter != null)
//         rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);

//       // データ更新イベント
//       CharacterRepository.OnUserCharactersUpdated += OnCharactersUpdated;
//       CharacterRepository.OnCharacterUpdated += OnCharacterUpdated;
//       CharacterRepository.OnCharacterLevelUp += OnCharacterLevelUp;

//       // キャラクターリストビューイベント
//       if (characterListView != null)
//       {
//         characterListView.OnCharacterSelected += OnCharacterSelected;
//         characterListView.OnCharacterLevelUpRequested += OnLevelUpRequested;
//       }

//       // キャラクター詳細ビューイベント
//       if (characterDetailView != null)
//       {
//         characterDetailView.OnEquipItemRequested += OnEquipItemRequested;
//         characterDetailView.OnCloseRequested += OnDetailViewClosed;
//       }
//     }

//     private void RemoveEventListeners()
//     {
//       // UI イベント
//       if (refreshButton != null)
//         refreshButton.onClick.RemoveListener(OnRefreshButtonClicked);

//       if (searchField != null)
//         searchField.onValueChanged.RemoveListener(OnSearchValueChanged);

//       if (rarityFilter != null)
//         rarityFilter.onValueChanged.RemoveListener(OnRarityFilterChanged);

//       // データ更新イベント
//       CharacterRepository.OnUserCharactersUpdated -= OnCharactersUpdated;
//       CharacterRepository.OnCharacterUpdated -= OnCharacterUpdated;
//       CharacterRepository.OnCharacterLevelUp -= OnCharacterLevelUp;

//       // ビューイベント
//       if (characterListView != null)
//       {
//         characterListView.OnCharacterSelected -= OnCharacterSelected;
//         characterListView.OnCharacterLevelUpRequested -= OnLevelUpRequested;
//       }

//       if (characterDetailView != null)
//       {
//         characterDetailView.OnEquipItemRequested -= OnEquipItemRequested;
//         characterDetailView.OnCloseRequested -= OnDetailViewClosed;
//       }
//     }

//     private async Task InitializeSceneAsync()
//     {
//       try
//       {
//         ShowLoading(true);

//         // DataManagerの初期化を待機
//         if (!DataManager.Instance.IsInitialized)
//         {
//           Debug.Log("Waiting for DataManager initialization...");
//           await WaitForDataManagerInitialization();
//         }

//         // キャラクターデータを読み込み
//         await LoadCharacterDataAsync();

//         // 自動リフレッシュ設定
//         if (autoRefreshOnStart)
//         {
//           InvokeRepeating(nameof(PeriodicRefresh), refreshInterval, refreshInterval);
//         }

//         Debug.Log("CharacterScene initialized successfully");
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"CharacterScene initialization failed: {ex.Message}");
//         ShowError("キャラクター画面の初期化に失敗しました");
//       }
//       finally
//       {
//         ShowLoading(false);
//       }
//     }

//     private async Task WaitForDataManagerInitialization()
//     {
//       var maxWaitTime = 10f; // 最大10秒待機
//       var waitTime = 0f;

//       while (!DataManager.Instance.IsInitialized && waitTime < maxWaitTime)
//       {
//         await Task.Delay(100);
//         waitTime += 0.1f;
//       }

//       if (!DataManager.Instance.IsInitialized)
//       {
//         throw new TimeoutException("DataManager initialization timeout");
//       }
//     }

//     #endregion

//     #region Data Loading

//     private async Task LoadCharacterDataAsync()
//     {
//       try
//       {
//         // ユーザーキャラクターを取得
//         var characters = await characterRepo.GetUserCharactersAsync();

//         if (characters != null && characters.Length > 0)
//         {
//           currentCharacters = characters;
//           ApplyFiltersAndUpdateView();

//           Debug.Log($"Loaded {characters.Length} characters");
//         }
//         else
//         {
//           Debug.LogWarning("No characters found");
//           currentCharacters = new CharacterData[0];
//           characterListView?.SetCharacters(currentCharacters);
//         }
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Failed to load character data: {ex.Message}");
//         ShowError("キャラクターデータの読み込みに失敗しました");
//       }
//     }

//     private async void PeriodicRefresh()
//     {
//       if (isLoading) return;

//       try
//       {
//         await characterRepo.RefreshAllAsync();
//         Debug.Log("Periodic character data refresh completed");
//       }
//       catch (Exception ex)
//       {
//         Debug.LogWarning($"Periodic refresh failed: {ex.Message}");
//       }
//     }

//     #endregion

//     #region Event Handlers

//     private async void OnRefreshButtonClicked()
//     {
//       if (isLoading) return;

//       try
//       {
//         ShowLoading(true);
//         await characterRepo.RefreshAllAsync();
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Manual refresh failed: {ex.Message}");
//         ShowError("データの更新に失敗しました");
//       }
//       finally
//       {
//         ShowLoading(false);
//       }
//     }

//     private async void OnSearchValueChanged(string query)
//     {
//       currentSearchQuery = query;
//       await ApplyFiltersAsync();
//     }

//     private async void OnRarityFilterChanged(int filterIndex)
//     {
//       // ドロップダウンのインデックスをRarityに変換
//       currentRarityFilter = filterIndex == 0 ? null : (CharacterRarity?)(filterIndex - 1);
//       await ApplyFiltersAsync();
//     }

//     private void OnCharactersUpdated(CharacterData[] characters)
//     {
//       currentCharacters = characters;
//       ApplyFiltersAndUpdateView();
//     }

//     private void OnCharacterUpdated(CharacterData character)
//     {
//       // リスト内のキャラクターを更新
//       if (currentCharacters != null)
//       {
//         var index = Array.FindIndex(currentCharacters, c => c.Id == character.Id);
//         if (index >= 0)
//         {
//           currentCharacters[index] = character;
//           characterListView?.UpdateCharacter(character);
//         }
//       }

//       // 詳細ビューが開いている場合は更新
//       if (selectedCharacter?.Id == character.Id)
//       {
//         selectedCharacter = character;
//         characterDetailView?.UpdateCharacter(character);
//       }
//     }

//     private void OnCharacterLevelUp(int characterId)
//     {
//       Debug.Log($"Character {characterId} leveled up!");

//       // レベルアップエフェクトの表示など
//       characterListView?.ShowLevelUpEffect(characterId);
//     }

//     private async void OnCharacterSelected(CharacterData character)
//     {
//       selectedCharacter = character;

//       // 詳細データを取得
//       try
//       {
//         var detailData = await characterRepo.GetCharacterDetailsAsync(character.Id);
//         if (detailData != null)
//         {
//           selectedCharacter = detailData;
//         }

//         characterDetailView?.ShowCharacterDetail(selectedCharacter);
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Failed to load character details: {ex.Message}");
//         ShowError("キャラクター詳細の読み込みに失敗しました");
//       }
//     }

//     private async void OnLevelUpRequested(CharacterData character)
//     {
//       if (isLoading) return;

//       try
//       {
//         ShowLoading(true);

//         // レベルアップ条件のチェック
//         var userCurrency = await userRepo.GetCurrencyAsync();
//         if (userCurrency.Gold < character.LevelUpCost)
//         {
//           ShowError("ゴールドが不足しています");
//           return;
//         }

//         // レベルアップ実行
//         var success = await characterRepo.LevelUpCharacterAsync(character.Id);
//         if (!success)
//         {
//           ShowError("レベルアップに失敗しました");
//         }
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Level up failed: {ex.Message}");
//         ShowError("レベルアップ処理でエラーが発生しました");
//       }
//       finally
//       {
//         ShowLoading(false);
//       }
//     }

//     private async void OnEquipItemRequested(int characterId, int itemId, EquipmentSlot slot)
//     {
//       try
//       {
//         ShowLoading(true);

//         var success = await characterRepo.EquipItemAsync(characterId, itemId, slot);
//         if (!success)
//         {
//           ShowError("装備の変更に失敗しました");
//         }
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Equipment change failed: {ex.Message}");
//         ShowError("装備変更処理でエラーが発生しました");
//       }
//       finally
//       {
//         ShowLoading(false);
//       }
//     }

//     private void OnDetailViewClosed()
//     {
//       selectedCharacter = null;
//       characterDetailView?.Hide();
//     }

//     #endregion

//     #region Filtering and UI Updates

//     private async Task ApplyFiltersAsync()
//     {
//       if (currentCharacters == null) return;

//       try
//       {
//         var filteredCharacters = await characterRepo.SearchCharactersAsync(
//             currentSearchQuery,
//             currentRarityFilter
//         );

//         characterListView?.SetCharacters(filteredCharacters);
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Filter application failed: {ex.Message}");
//       }
//     }

//     private void ApplyFiltersAndUpdateView()
//     {
//       _ = ApplyFiltersAsync();
//     }

//     #endregion

//     #region UI State Management

//     private void ShowLoading(bool show)
//     {
//       isLoading = show;

//       if (loadingIndicator != null)
//         loadingIndicator.SetActive(show);

//       // インタラクション可能な要素の無効化/有効化
//       if (refreshButton != null)
//         refreshButton.interactable = !show;

//       if (searchField != null)
//         searchField.interactable = !show;

//       if (rarityFilter != null)
//         rarityFilter.interactable = !show;
//     }

//     private void ShowError(string message)
//     {
//       if (errorDialog != null)
//       {
//         errorDialog.Show(message);
//       }
//       else
//       {
//         Debug.LogError($"Error: {message}");
//       }
//     }

//     #endregion
//   }
// }