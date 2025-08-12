using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Scripts.Runtime.Core;
using Scripts.Runtime.DataModels;
using Scripts.Runtime.Data.Repositories;
using Scripts.Runtime.Services;
using System.Linq;

namespace Scripts.Runtime.Views.Features.Home
{
  public class HomeScene : MonoBehaviour
  {
    [SerializeField] private UserProfileView userProfileView;
    [SerializeField] private CharacterDisplayView mainCharacterView;
    [SerializeField] private CurrencyDisplayView currencyView;
    [SerializeField] private Button[] navigationButtons;

    private void Start()
    {
      SetupHomeScreen();
      SubscribeToDataEvents();
    }

    private async void SetupHomeScreen()
    {
      // データが初期化されるまで待機
      while (!DataManager.Instance.IsInitialized)
      {
        await Task.Delay(100);
      }

      // ユーザー情報表示
      await DisplayUserInfo();

      // メインキャラクター表示
      await DisplayMainCharacter();

      // 通貨情報表示
      await DisplayCurrency();

      // ナビゲーションボタンの有効化
      EnableNavigation();
    }

    private async Task DisplayUserInfo()
    {
      try
      {
        var userProfile = await DataManager.Instance.Users.GetUserProfileAsync();
        userProfileView.DisplayProfile(userProfile);
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to display user info: {ex.Message}");
      }
    }

    private async Task DisplayMainCharacter()
    {
      try
      {
        var characters = await DataManager.Instance.Characters.GetUserCharactersAsync();
        var mainCharacter = characters.FirstOrDefault(c => c.IsMainCharacter);

        if (mainCharacter != null)
        {
          mainCharacterView.DisplayCharacter(mainCharacter);
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to display main character: {ex.Message}");
      }
    }

    private async Task DisplayCurrency()
    {
      try
      {
        var currency = await DataManager.Instance.Users.GetCurrencyAsync();
        currencyView.DisplayCurrency(currency);
      }
      catch (Exception ex)
      {
        Debug.LogError($"Failed to display currency: {ex.Message}");
      }
    }

    private void SubscribeToDataEvents()
    {
      // データ更新イベントの購読
      DataManager.OnDataUpdated += OnDataUpdated;
      UserRepository.OnCurrencyUpdated += OnCurrencyUpdated;
      CharacterRepository.OnCharacterUpdated += OnCharacterUpdated;
    }

    private void OnDataUpdated(string dataType)
    {
      Debug.Log($"Data updated: {dataType}");
      // 必要に応じてUI更新
    }

    private void OnCurrencyUpdated(CurrencyData currency)
    {
      currencyView.DisplayCurrency(currency);
    }

    private void OnCharacterUpdated(CharacterData character)
    {
      if (character.IsMainCharacter)
      {
        mainCharacterView.DisplayCharacter(character);
      }
    }

    // ナビゲーションボタンの処理
    public void OnCharacterButtonClicked()
    {
      SceneManager.LoadScene("CharacterScene");
    }

    public void OnShopButtonClicked()
    {
      SceneManager.LoadScene("ShopScene");
    }

    public void OnQuestButtonClicked()
    {
      SceneManager.LoadScene("QuestScene");
    }
  }
}
