using UnityEngine;
using UnityEngine.UI; // Buttonコンポーネントを扱うために必要
using System.Collections.Generic; // Listを扱うために必要

/// <summary>
/// タブ切り替えシステムを制御するクラス
/// </summary>
namespace Scripts.Runtime.Views.Shared
{
    public class TabController : MonoBehaviour
    {
        // Inspectorから設定する項目
        public List<GameObject> screens; // 表示/非表示を切り替えるスクリーンのリスト
        public List<Button> tabButtons; // タブとして機能するボタンのリスト

        // 選択されているタブと、非選択のタブの色
        public Color selectedTabColor = Color.white;
        public Color deselectedTabColor = new Color(0.75f, 0.75f, 0.75f); // 少し暗い灰色

        void Start()
        {
            // 起動時に最初のタブ（0番目）を選択状態にする
            SelectTab(0);
        }

        /// <summary>
        /// 指定されたインデックスのタブを選択状態にする
        /// </summary>
        /// <param name="tabIndex">選択するタブのインデックス（0から始まる）</param>
        public void SelectTab(int tabIndex)
        {
            // 全てのスクリーンを一旦非表示にする
            for (int i = 0; i < screens.Count; i++)
            {
                screens[i].SetActive(false);
                // 対応するボタンを非選択色にする
                tabButtons[i].GetComponent<Image>().color = deselectedTabColor;
            }

            // 指定されたインデックスのスクリーンだけを表示する
            if (tabIndex >= 0 && tabIndex < screens.Count)
            {
                screens[tabIndex].SetActive(true);
                // 対応するボタンを選択色にする
                tabButtons[tabIndex].GetComponent<Image>().color = selectedTabColor;
            }
            else
            {
                Debug.LogError("無効なタブのインデックスです: " + tabIndex);
            }
        }
    }
}
