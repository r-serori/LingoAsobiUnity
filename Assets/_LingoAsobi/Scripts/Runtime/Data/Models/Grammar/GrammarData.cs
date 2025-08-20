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
    public List<GrammarFloor> floors;
  }

  [Serializable]
  public class GrammarFloor
  {
    public int id;
    public string title;
    public string description;
    public int order;
    public List<GrammarLesson> lessons;
  }

  [Serializable]
  public class GrammarLesson
  {
    public int id;
    public string title;
    public string description;
    public string examples;
    public string explanation;
    public int order;
    public List<GrammarQuestion> questions;
  }

  [Serializable]
  public class GrammarQuestion
  {
    public int id;
    public string title;
    public string question;
    public string answer;
    public string explanation;
    public int order;
  }
}