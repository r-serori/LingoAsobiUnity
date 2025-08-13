// using UnityEngine;
// using System;
// using System.Threading.Tasks;
// using Scripts.Runtime.Core;
// using Scripts.Runtime.DataModels;
// using Scripts.Runtime.Data.Repositories;
// using Scripts.Runtime.Services;

// namespace Scripts.Runtime.Views.Features.Shop
// {
//   public class ShopScene : MonoBehaviour
//   {
//     [SerializeField] private ShopItemListView shopItemListView;
//     [SerializeField] private PurchaseConfirmDialog purchaseDialog;
//     [SerializeField] private CurrencyDisplayView currencyView;

//     private ShopRepository shopRepo;
//     private UserRepository userRepo;
//     private ShopItem[] currentShopItems;

//     private async void Start()
//     {
//       InitializeRepositories();
//       await LoadShopData();
//       SetupEventListeners();
//     }

//     private void InitializeRepositories()
//     {
//       shopRepo = DataManager.Instance.Shop;
//       userRepo = DataManager.Instance.Users;
//     }

//     private async Task LoadShopData()
//     {
//       try
//       {
//         // ショップアイテムとユーザー通貨を並行取得
//         var shopDataTask = shopRepo.GetShopItemsAsync();
//         var currencyTask = userRepo.GetCurrencyAsync();

//         await Task.WhenAll(shopDataTask, currencyTask);

//         currentShopItems = await shopDataTask;
//         var currency = await currencyTask;

//         // UI表示
//         shopItemListView.DisplayItems(currentShopItems);
//         currencyView.DisplayCurrency(currency);
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Failed to load shop data: {ex.Message}");
//         ShowErrorDialog("ショップデータの読み込みに失敗しました");
//       }
//     }

//     private void SetupEventListeners()
//     {
//       shopItemListView.OnItemPurchaseRequested += OnPurchaseRequested;
//       purchaseDialog.OnPurchaseConfirmed += OnPurchaseConfirmed;

//       // データ更新イベント
//       ShopRepository.OnShopItemsUpdated += OnShopItemsUpdated;
//       UserRepository.OnCurrencyUpdated += OnCurrencyUpdated;
//     }

//     private async void OnPurchaseRequested(ShopItem item)
//     {
//       try
//       {
//         // 購入可能かチェック
//         var currency = await userRepo.GetCurrencyAsync();

//         if (CanPurchase(item, currency))
//         {
//           purchaseDialog.ShowPurchaseConfirm(item, currency);
//         }
//         else
//         {
//           ShowErrorDialog("通貨が不足しています");
//         }
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Purchase check failed: {ex.Message}");
//         ShowErrorDialog("購入処理でエラーが発生しました");
//       }
//     }

//     private async void OnPurchaseConfirmed(ShopItem item, int quantity)
//     {
//       try
//       {
//         var success = await shopRepo.PurchaseItemAsync(item.Id, quantity);

//         if (success)
//         {
//           ShowSuccessDialog($"{item.Name} を購入しました！");

//           // 在庫更新のため、ショップデータをリフレッシュ
//           await shopRepo.RefreshShopItemAsync(item.Id);
//         }
//         else
//         {
//           ShowErrorDialog("購入に失敗しました");
//         }
//       }
//       catch (Exception ex)
//       {
//         Debug.LogError($"Purchase failed: {ex.Message}");
//         ShowErrorDialog("購入処理でエラーが発生しました");
//       }
//     }

//     private bool CanPurchase(ShopItem item, CurrencyData currency)
//     {
//       switch (item.CurrencyType)
//       {
//         case CurrencyType.Gold:
//           return currency.Gold >= item.Price;
//         case CurrencyType.Gem:
//           return currency.Gem >= item.Price;
//         default:
//           return false;
//       }
//     }

//     private void OnShopItemsUpdated(ShopItem[] items)
//     {
//       currentShopItems = items;
//       shopItemListView.DisplayItems(items);
//     }

//     private void OnCurrencyUpdated(CurrencyData currency)
//     {
//       currencyView.DisplayCurrency(currency);
//     }
//   }
// }