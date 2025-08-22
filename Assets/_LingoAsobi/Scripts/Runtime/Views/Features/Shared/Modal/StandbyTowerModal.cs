using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Runtime.Data.Models.Character;
using System.Collections.Generic;
using Scripts.Runtime.Data.Models.Grammar;
using Scripts.Runtime.Utilities.Helpers;
using Scripts.Runtime.Views.Features.Grammar.GrammarFloor;
using Scripts.Runtime.Utilities.Constants;

namespace Scripts.Runtime.Views.Features.Shared.Modal
{
  public class StandbyTowerModal : MonoBehaviour
  {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundButton;
    [SerializeField] private Button startButton;

    private GrammarFloorData floorItem;
    private void Start()
    {
      SetupButtons();
    }

    public void SetData(GrammarFloorData floorItem)
    {
      titleText.text = floorItem.title;
      descriptionText.text = floorItem.description;
      this.floorItem = floorItem;
    }

    private void SetupButtons()
    {
      backgroundButton?.onClick.AddListener(OnBackgroundClicked);
      closeButton?.onClick.AddListener(OnCloseButtonClicked);
      startButton?.onClick.AddListener(OnStartButtonClicked);
    }

    public void OnCloseButtonClicked()
    {
      gameObject.SetActive(false);
    }

    public void OnBackgroundClicked()
    {
      gameObject.SetActive(false);
    }

    public async void OnStartButtonClicked()
    {
      GrammarFloorScene.SelectedFloorId = floorItem.id;

      await SceneHelper.Instance.LoadSceneAsync(GameConstants.Scenes.GrammarFloor);
    }
  }
}