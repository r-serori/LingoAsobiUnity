using System;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.Character
{
  /// <summary>
  /// キャラクターデータモデル
  /// ゲーム内のキャラクター情報を管理
  /// </summary>
  [Serializable]
  public class CharacterFormationData
  {
    public int id;
    public string characterId;
    [Header("キャラクターの位置")]
    public int characterOrder;
    [Header("陣形の番号")]
    public int formationNumber;
  }
}