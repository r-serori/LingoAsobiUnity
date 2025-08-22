using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Runtime.Data.Models.Grammar
{
  /// <summary>
  /// 文法データモデル
  /// ゲーム内の文法情報を管理
  /// </summary>
  [Serializable]
  public class GrammarData
  {
    public int id;
    public string title;
    public int order;
    public string description;
    public int lastCorrectedFloorOrder;
    public List<GrammarFloorData> floors;
  }

  [Serializable]
  public class GrammarFloorData
  {
    public int id;
    public string title;
    public string description;
    public int order;
    public int lastCorrectedLessonOrder;
    public List<GrammarLessonData> lessons;
  }

  [Serializable]
  public class GrammarLessonData
  {
    public int id;
    public string title;
    public string description;
    public string examples;
    public string explanation;
    public int order;
    public List<GrammarQuestionData> questions;
  }

  [Serializable]
  public class GrammarQuestionData
  {
    public int id;
    public string title;
    public string question;
    public string answer;
    public string explanation;
    public int order;
  }
}