using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Scripts.Runtime.Views.Base;
using Scripts.Runtime.Core;
using Scripts.Runtime.Data.Models.User;
using Scripts.Runtime.Utilities.Constants;
using Scripts.Runtime.Utilities.Helpers;
using System;

namespace Scripts.Runtime.Views.Features.Title
{
  public class TitleScene : BaseScene
  {
    [Header("Title Scene References")]
    [SerializeField] private TitleView titleView;

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
      await base.OnInitializeAsync();
    }

    protected override void InitializeViews()
    {
      base.InitializeViews();
    }

    protected override async Task OnAfterActivate()
    {
      await base.OnAfterActivate();

      // TitleViewを表示
      if (titleView != null)
      {
          await ShowViewAsync<TitleView>();
      }
    }

    #endregion

  }
}