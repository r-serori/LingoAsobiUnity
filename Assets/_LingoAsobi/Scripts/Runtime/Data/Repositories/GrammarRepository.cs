using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Runtime.Data.Repositories.Base;
using Scripts.Runtime.Data.Models.Grammar;

namespace Scripts.Runtime.Data.Repositories
{
  /// <summary>
  /// 文法データのリポジトリクラス
  /// 文法情報のCRUD操作を管理
  /// </summary>
  public class GrammarRepository : BaseRepository<GrammarData>
  {
    // APIエンドポイント
    protected override string EndpointUrl => "/api/grammars";

    // キャッシュキーのプレフィックス
    protected override string CacheKeyPrefix => "grammar";

    // シングルトンインスタンス
    private static readonly GrammarRepository _instance;
    public static GrammarRepository Instance
    {
      get
      {
        return _instance ?? new GrammarRepository();
      }
    }

    /// <summary>
    /// プライベートコンストラクタ（シングルトン）
    /// </summary>
    private GrammarRepository() : base()
    {
    }

    /// <summary>
    /// トレーニングデータを取得
    /// </summary>
    public async Task<List<GrammarData>> GetGrammarAllDataAsync()
    {
      return await GetAllAsync();
    }

    protected override async Task<GrammarData> GetMockDataAsync()
    {
      return GetMockGrammars().FirstOrDefault();
    }

    /// <summary>
    /// MockDataからキャラクターを取得
    /// </summary>
    protected override async Task<GrammarData> GetMockDataByIdAsync(string id)
    {
      await Task.Delay(100); // ネットワーク遅延をシミュレート

      var grammars = GetMockGrammars();
      return grammars.FirstOrDefault(c => c.id == int.Parse(id));
    }

    /// <summary>
    /// MockDataから全キャラクターを取得
    /// </summary>
    protected override async Task<List<GrammarData>> GetAllMockDataAsync()
    {
      await Task.Delay(100);
      return GetMockGrammars();
    }

    /// <summary>
    /// Mockキャラクターデータを生成
    /// </summary>
    private List<GrammarData> GetMockGrammars()
    {
      return new List<GrammarData>
            {
                new GrammarData
                {
                    id = 1,
                    title= "be動詞、一般動詞の森",
                    order = 1,
                    description = "初対面でも使える、自己紹介も含めた自然な会話の第一歩！",
                    floors = new List<GrammarFloor>
                    {
                        new GrammarFloor
                        {
                            id = 1,
                            title = "be動詞の森 肯定文",
                            order = 1,
                            description = "am, is, are の基本を学ぼう！",
                            lessons = new List<GrammarLesson>
                            {
                                new GrammarLesson
                                {
                                    id = 1,
                                    title = "be動詞 am",
                                    order = 1,
                                    explanation = "amは、自分のことを表現するときに使うbe動詞です。Iは、私という意味です。",
                                    description = "自分のことを表現しよう！",
                                    examples = "I'm a student. / I'm from Japan.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 1,
                                            question = "私は、フリーランスデザイナーです。",
                                            answer = "I am a freelance designer.",
                                            explanation = "自分の職業や立場を言うときの基本形。I'm の後に自分のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 2,
                                            question = "彼女は、料理がとても上手です。",
                                            answer = "I am from Japan.",
                                            explanation = "I am の後に自分の国を続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 3,
                                            question = "彼は、学生です。",
                                            answer = "I am a student.",
                                            explanation = "I am の後に自分のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 2,
                                    title = "be動詞 are（You）",
                                    order = 2,
                                    explanation = "areは、話し相手がいる場合の表現に使うbe動詞です。Youは、あなたという意味です。",
                                    description = "話し相手がいる場合の表現！",
                                    examples = "You're from Japan. / You're from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 4,
                                            question = "あなたは、学生です。",
                                            answer = "You are a student.",
                                            explanation = "You are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 5,
                                            question = "あなたは、アメリカ人です。",
                                            answer = "You are from USA.",
                                            explanation = "You are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 6,
                                            question = "あなたは、学生です。",
                                            answer = "You are a student.",
                                            explanation = "You are の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                              {
                                id = 3,
                                title = "be動詞 is（She, He）",
                                order = 3,
                                description = "She, He のことを表現しよう！",
                                explanation = "isは、第三者のことを表現するときに使うbe動詞です。Sheは、彼女という意味です。Heは、彼という意味です。",
                                examples = "She's a designer. / He's from USA.",
                                questions = new List<GrammarQuestion>
                                {
                                  new GrammarQuestion
                                  {
                                    id = 7,
                                    question = "彼女は、デザイナーです。",
                                    answer = "She is a designer.",
                                    explanation = "She is の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 8,
                                    question = "彼は、アメリカ人です。",
                                    answer = "He is from USA.",
                                    explanation = "He is の後に相手のことを続けるだけ！",
                                  },
                                },
                              },
                                new GrammarLesson
                                {
                                    id = 4,
                                    title = "be動詞 are（We）",
                                    order = 4,
                                    explanation = "Weは、私たちという意味です。",
                                    description = "私たちのことを表現しよう！",
                                    examples = "We're from Japan. / We're from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 9,
                                            question = "私たちは、学生です。",
                                            answer = "We are a student.",
                                            explanation = "We are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 10,
                                            question = "私たちは、アメリカ人です。",
                                            answer = "We are from USA.",
                                            explanation = "We are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 11,
                                            question = "私たちは、学生です。",
                                            answer = "We are a student.",
                                            explanation = "We are の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 5,
                                    title = "be動詞 are（They）",
                                    order = 5,
                                    explanation = "Theyは、彼らという意味です。",
                                    description = "彼らのことを表現しよう！",
                                    examples = "They're from Japan. / They're from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 12,
                                            question = "彼らは、学生です。",
                                            answer = "They are a student.",
                                            explanation = "They are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 13,
                                            question = "彼らは、アメリカ人です。",
                                            answer = "They are from USA.",
                                            explanation = "They are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 14,
                                            question = "彼らは、日本人です。",
                                            answer = "They are Japanese.",
                                            explanation = "They are の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 6,
                                    title = "be動詞 is（That, This）",
                                    order = 6,
                                    explanation = "Thatは、そのという意味です。Thisは、このという意味です。",
                                    description = "That, This のことを表現しよう！",
                                    examples = "That's a cat. / This is a dog.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 15,
                                            question = "それは、犬です。",
                                            answer = "That is a dog.",
                                            explanation = "That is の後に相手のことを続けるだけ！",
                                        },

                                        new GrammarQuestion
                                        {
                                            id = 16,
                                            question = "これは、猫です。",
                                            answer = "This is a cat.",
                                            explanation = "This is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 17,
                                            question = "それは、犬です。",
                                            answer = "That is a dog.",
                                            explanation = "That is の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 7,
                                    title = "be動詞 is（It）",
                                    order = 7,
                                    explanation = "Itは、それという意味です。",
                                    description = "It のことを表現しよう！",
                                    examples = "That's a cat. / This is a dog. / That's a dog. / This is a cat.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 18,
                                            question = "それは、犬です。",
                                            answer = "That is a dog.",
                                            explanation = "It is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 19,
                                            question = "それは、犬です。",
                                            answer = "That is a dog.",
                                            explanation = "It is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 20,
                                            question = "それは、犬です。",
                                            answer = "That is a dog.",
                                            explanation = "It is の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 8,
                                    title = "be動詞 is（These, Those）",
                                    order = 8,
                                    explanation = "Theseは、これらという意味です。Thoseは、それらという意味です。",
                                    description = "These, Those のことを表現しよう！",
                                    examples = "These are cats. / Those are dogs.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 21,
                                            question = "これらは、猫です。",
                                            answer = "These are cats.",
                                            explanation = "These are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 22,
                                            question = "それらは、犬です。",
                                            answer = "Those are dogs.",
                                            explanation = "Those are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 23,
                                            question = "これらは、猫です。",
                                            answer = "These are cats.",
                                            explanation = "These are の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 9,
                                    title = "be動詞 is（There）",
                                    order = 9,
                                    explanation = "Thereは、そこという意味です。",
                                    description = "There のことを表現しよう！",
                                    examples = "There is a cat. / There is a dog.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 24,
                                            question = "そこには、犬がいます。",
                                            answer = "There is a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 25,
                                            question = "そこには、犬がいます。",
                                            answer = "There is a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 26,
                                            question = "そこには、犬がいます。",
                                            answer = "There is a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                    },
                                }
                            }
                        },
                        new GrammarFloor
                        {
                            id = 2,
                            title = "be動詞の森 否定文",
                            order = 2,
                            description = "am, is, are の否定系を学ぼう！",
                            lessons = new List<GrammarLesson>
                            {
                                new GrammarLesson
                                {
                                    id = 1,
                                    title = "be動詞 am",
                                    order = 1,
                                    explanation = "amは、自分のことを表現するときに使うbe動詞です。I'm の後に自分のことを続けるだけ！",
                                    description = "自分のことを表現しよう！",
                                    examples = "I'm not a student. / I'm not from Japan.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 1,
                                            question = "私は、学生ではありません。",
                                            answer = "I'm not a student.",
                                            explanation = "I'm の後に自分のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 2,
                                            question = "私は、学生ではありません。",
                                            answer = "I'm not a student.",
                                            explanation = "I'm の後に自分のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 3,
                                            question = "私は、学生ではありません。",
                                            answer = "I'm not a student.",
                                            explanation = "I'm の後に自分のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 2,
                                    title = "be動詞 are（You）",
                                    order = 2,
                                    explanation = "areは、話し相手がいる場合の表現に使うbe動詞です。You're の後に相手のことを続けるだけ！",
                                    description = "話し相手がいる場合の表現！",
                                    examples = "You're not a student. / You're not from Japan.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 4,
                                            question = "あなたは、学生ではありません。",
                                            answer = "You're not a student.",
                                            explanation = "You're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 5,
                                            question = "あなたは、学生ではありません。",
                                            answer = "You're not a student.",
                                            explanation = "You're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 6,
                                            question = "あなたは、学生ではありません。",
                                            answer = "You're not a student.",
                                            explanation = "You're の後に相手のことを続けるだけ！",
                                        },
                                    },
                                },
                                new GrammarLesson
                                {
                                    id = 3,
                                title = "be動詞 is（She, He）",
                                order = 3,
                                description = "She, He のことを表現しよう！",
                                explanation = "isは、第三者のことを表現するときに使うbe動詞です。Sheは、彼女という意味です。Heは、彼という意味です。",
                                examples = "She's not a student. / He's not from USA.",
                                questions = new List<GrammarQuestion>
                                {
                                    new GrammarQuestion
                                    {
                                        id = 7,
                                        question = "彼女は、学生ではありません。",
                                        answer = "She's not a student.",
                                        explanation = "She's の後に相手のことを続けるだけ！",
                                    },
                                    new GrammarQuestion
                                    {
                                        id = 8,
                                        question = "彼は、学生ではありません。",
                                        answer = "He's not a student.",
                                        explanation = "He's の後に相手のことを続けるだけ！",
                                    },
                                    new GrammarQuestion
                                    {
                                        id = 9,
                                        question = "彼女は、学生ではありません。",
                                        answer = "She's not a student.",
                                        explanation = "She's の後に相手のことを続けるだけ！",
                                    },
                                },
                                },
                                new GrammarLesson
                                {
                                    id = 4,
                                    title = "be動詞 are（We）",
                                    order = 4,
                                    explanation = "Weは、私たちという意味です。",
                                    description = "私たちのことを表現しよう！",
                                    examples = "We're not a student. / We're not from Japan.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 10,
                                            question = "私たちは、学生ではありません。",
                                            answer = "We're not a student.",
                                            explanation = "We're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 11,
                                            question = "私たちは、学生ではありません。",
                                            answer = "We're not a student.",
                                            explanation = "We're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 12,
                                            question = "私たちは、学生ではありません。",
                                            answer = "We're not a student.",
                                            explanation = "We're の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 5,
                                    title = "be動詞 are（They）",
                                    order = 5,
                                    explanation = "Theyは、彼らという意味です。",
                                    description = "彼らのことを表現しよう！",
                                    examples = "They're not a student. / They're not from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 13,
                                            question = "彼らは、学生ではありません。",
                                            answer = "They're not a student.",
                                            explanation = "They're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 14,
                                            question = "彼らは、学生ではありません。",
                                            answer = "They're not a student.",
                                            explanation = "They're の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 15,
                                            question = "彼らは、学生ではありません。",
                                            answer = "They're not a student.",
                                            explanation = "They're の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 6,
                                    title = "be動詞 is（That, This）",
                                    order = 6,
                                    explanation = "Thatは、そのという意味です。Thisは、このという意味です。",
                                    description = "That, This のことを表現しよう！",
                                    examples = "That's not a student. / This's not from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 16,
                                            question = "あれは、犬ではありません。",
                                            answer = "That's not a dog.",
                                            explanation = "That's の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 17,
                                            question = "あれは、猫ではありません。",
                                            answer = "That's not a cat.",
                                            explanation = "That's の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 18,
                                            question = "これは、黒ではありません。",
                                            answer = "This's not black.",
                                            explanation = "This's の後に色のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 7,
                                    title = "be動詞 is（It）",
                                    order = 7,
                                    explanation = "Itは、それという意味です。",
                                    description = "It のことを表現しよう！",
                                    examples = "It's not a student. / It's not from USA.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 19,
                                            question = "それは、犬ではありません。",
                                            answer = "It's not a dog.",
                                            explanation = "It's の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 20,
                                            question = "それは、犬ではありません。",
                                            answer = "It's not a dog.",
                                            explanation = "It's の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 21,
                                            question = "それは、犬ではありません。",
                                            answer = "It's not a dog.",
                                            explanation = "It's の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 8,
                                    title = "be動詞 is（These, Those）",
                                    order = 8,
                                    explanation = "Theseは、これらという意味です。Thoseは、それらという意味です。",
                                    description = "These, Those のことを表現しよう！",
                                    examples = "These are not cats. / Those are not dogs.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 22,
                                            question = "これらは、猫ではありません。",
                                            answer = "These are not cats.",
                                            explanation = "These are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 23,
                                            question = "それらは、犬ではありません。",
                                            answer = "Those are not dogs.",
                                            explanation = "Those are の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 24,
                                            question = "これらは、猫ではありません。",
                                            answer = "These are not cats.",
                                            explanation = "These are の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 9,
                                    title = "be動詞 is（There）",
                                    order = 9,
                                    explanation = "Thereは、そこという意味です。",
                                    description = "There のことを表現しよう！",
                                    examples = "There is not a cat. / There is not a dog.",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 25,
                                            question = "そこには、犬がいません。",
                                            answer = "There is not a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 26,
                                            question = "そこには、犬がいません。",
                                            answer = "There is not a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 27,
                                            question = "そこには、犬がいません。",
                                            answer = "There is not a dog.",
                                            explanation = "There is の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                            }
                        },
                        new GrammarFloor
                        {
                            id = 3,
                            title = "be動詞の森 疑問文",
                            order = 3,
                            description = "am, is, are の疑問系を学ぼう！",
                            lessons = new List<GrammarLesson>
                            {
                                new GrammarLesson
                                {
                                    id = 1,
                                    title = "be動詞 am",
                                    order = 1,
                                    explanation = "amは、自分のことを表現するときに使うbe動詞です。I'm の後に自分のことを続けるだけ！",
                                    description = "自分のことを表現しよう！",
                                    examples = "Am I late?",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 1,
                                            question = "遅れていますか？",
                                            answer = "Am I late?",
                                            explanation = "Am I の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 2,
                                            question = "これで合っていますか？ / 正しくやっていますか？",
                                            answer = "Am I doing this right?",
                                            explanation = "Am I の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 3,
                                            question = "私の言っていることは明確ですか？",
                                            answer = "Am I being clear?",
                                            explanation = "Am I の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 4,
                                            question = "遅れていますか？",
                                            answer = "Am I making sense? ",
                                            explanation = "私の言っていることは理解できますか？ / 筋が通っていますか？",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 5,
                                            question = "邪魔になっていますか？",
                                            answer = "Am I in the way? ",
                                            explanation = "私の言っていることは理解できますか？ / 筋が通っていますか？",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 2,
                                    title = "be動詞 are（You）",
                                    order = 2,
                                    explanation = "areは、話し相手がいる場合の表現に使うbe動詞です。You're の後に相手のことを続けるだけ！",
                                    description = "話し相手がいる場合の表現！",
                                    examples = "Are you a student?",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 1,
                                            question = "あなたは、学生ですか？",
                                            answer = "Are you a student?",
                                            explanation = "Are you の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 2,
                                            question = "あなたは、学生ですか？",
                                            answer = "Are you a student?",
                                            explanation = "Are you の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 3,
                                            question = "あなたは、学生ですか？",
                                            answer = "Are you a student?",
                                            explanation = "Are you の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 3,
                                    title = "be動詞 is（She, He）",
                                    order = 3,
                                    explanation = "Sheは、彼女という意味です。Heは、彼という意味です。",
                                    description = "She, He のことを表現しよう！",
                                    examples = "Is she a student? / Is he a student?",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 4,
                                            question = "彼女は、学生ですか？",
                                            answer = "Is she a student?",
                                            explanation = "Is she の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 5,
                                            question = "彼は、学生ですか？",
                                            answer = "Is he a student?",
                                            explanation = "Is he の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 6,
                                            question = "彼女は、学生ですか？",
                                            answer = "Is she a student?",
                                            explanation = "Is she の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 4,
                                    title = "be動詞 are（We）",
                                    order = 4,
                                    explanation = "Weは、私たちという意味です。",
                                    description = "私たちのことを表現しよう！",
                                    examples = "Are we a student? / Are we from Japan?",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                            id = 7,
                                            question = "私たちは、学生ですか？",
                                            answer = "Are we a student?",
                                            explanation = "Are we の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 8,
                                            question = "私たちは、学生ですか？",
                                            answer = "Are we a student?",
                                            explanation = "Are we の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                            id = 9,
                                            question = "私たちは、学生ですか？",
                                            answer = "Are we a student?",
                                            explanation = "Are we の後に相手のことを続けるだけ！",
                                        },
                                    }
                                },
                                new GrammarLesson
                                {
                                    id = 5,
                                    title = "be動詞 are（They）",
                                    order = 5,
                                    explanation = "Theyは、彼らという意味です。",
                                    description = "彼らのことを表現しよう！",
                                    examples = "Are they a student? / Are they from USA?",
                                    questions = new List<GrammarQuestion>
                                    {
                                        new GrammarQuestion
                                        {
                                          id = 10,
                                          question = "彼らは、学生ですか？",
                                          answer = "Are they a student?",
                                          explanation = "Are they の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 11,
                                          question = "彼らは、学生ですか？",
                                          answer = "Are they a student?",
                                          explanation = "Are they の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 12,
                                          question = "彼らは、学生ですか？",
                                          answer = "Are they a student?",
                                          explanation = "Are they の後に相手のことを続けるだけ！",
                                        },
                                        }
                                    },
                                    new GrammarLesson
                                    {
                                      id = 6,
                                      title = "be動詞 is（That, This）",
                                      order = 6,
                                      explanation = "Thatは、そのという意味です。Thisは、このという意味です。",
                                      description = "That, This のことを表現しよう！",
                                      examples = "Is that a cat? / Is this a dog?",
                                      questions = new List<GrammarQuestion>
                                      {
                                        new GrammarQuestion
                                        {
                                          id = 13,
                                          question = "あれは、猫ですか？",
                                          answer = "Is that a cat?",
                                          explanation = "Is that の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 14,
                                          question = "あれは、猫ですか？",
                                          answer = "Is that a cat?",
                                          explanation = "Is that の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 15,
                                          question = "あれは、猫ですか？",
                                          answer = "Is that a cat?",
                                          explanation = "Is that の後に相手のことを続けるだけ！",
                                        },
                                      }
                                    },
                                    new GrammarLesson
                                    {
                                      id = 7,
                                      title = "be動詞 is（It）",
                                      order = 7,
                                      explanation = "Itは、それという意味です。",
                                      description = "It のことを表現しよう！",
                                      examples = "Is it a cat? / Is it a dog?",
                                      questions = new List<GrammarQuestion>
                                      {
                                        new GrammarQuestion
                                        {
                                          id = 16,
                                          question = "あれは、猫ですか？",
                                          answer = "Is it a cat?",
                                          explanation = "Is it の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 17,
                                          question = "あれは、猫ですか？",
                                          answer = "Is it a cat?",
                                          explanation = "Is it の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 18,
                                          question = "あれは、猫ですか？",
                                          answer = "Is it a cat?",
                                          explanation = "Is it の後に相手のことを続けるだけ！",
                                        },
                                      }
                                    },
                                    new GrammarLesson
                                    {
                                      id = 8,
                                      title = "be動詞 is（These, Those）",
                                      order = 8,
                                      explanation = "Theseは、これらという意味です。Thoseは、それらという意味です。",
                                      description = "These, Those のことを表現しよう！",
                                      examples = "Are these cats? / Are those dogs?",
                                      questions = new List<GrammarQuestion>
                                      {
                                        new GrammarQuestion
                                        {
                                          id = 16,
                                          question = "あれは、猫ですか？",
                                          answer = "Are these cats?",
                                          explanation = "Are these の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 17,
                                          question = "あれは、猫ですか？",
                                          answer = "Are these cats?",
                                          explanation = "Are these の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 18,
                                          question = "あれは、猫ですか？",
                                          answer = "Are these cats?",
                                          explanation = "Are these の後に相手のことを続けるだけ！",
                                        },
                                      }
                                    },
                                    new GrammarLesson
                                    {
                                      id = 9,
                                      title = "be動詞 is（There）",
                                      order = 9,
                                      explanation = "Thereは、そこという意味です。",
                                      description = "There のことを表現しよう！",
                                      examples = "Are there cats? / Are there dogs?",
                                      questions = new List<GrammarQuestion>
                                      {
                                        new GrammarQuestion
                                        {
                                          id = 19,
                                          question = "あれは、猫ですか？",
                                          answer = "Are there cats?",
                                          explanation = "Are there の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 20,
                                          question = "あれは、猫ですか？",
                                          answer = "Are there cats?",
                                          explanation = "Are there の後に相手のことを続けるだけ！",
                                        },
                                        new GrammarQuestion
                                        {
                                          id = 21,
                                          question = "あれは、猫ですか？",
                                          answer = "Are there cats?",
                                          explanation = "Are there の後に相手のことを続けるだけ！",
                                        },
                                      }
                                    },
                                },
                            },
                            new GrammarFloor
                            {
                              id = 4,
                              title = "一般動詞の森",
                              order = 4,
                              description = "一般動詞の肯定文を学ぼう！",
                              lessons = new List<GrammarLesson>
                              {
                                new GrammarLesson
                                {
                                  id = 1,
                                  title = "一般動詞の肯定文",
                                  order = 1,
                                  explanation = "一般動詞の肯定文を学ぼう！",
                                  description = "一般動詞の肯定文を学ぼう！",
                                  examples = "I go to school. / I eat breakfast. / I sleep.",
                                  questions = new List<GrammarQuestion>
                                  {
                                    new GrammarQuestion
                                    {
                                      id = 1,
                                      question = "私は、学校に行きます。",
                                      answer = "I go to school.",
                                      explanation = "I go to school. の後に相手のことを続けるだけ！",
                                    },
                                    new GrammarQuestion
                                    {
                                      id = 2,
                                      question = "私は、朝食を食べます。",
                                      answer = "I have breakfast.",
                                      explanation = "I have breakfast. の後に相手のことを続けるだけ！",
                                    },
                                    new GrammarQuestion
                                    {
                                      id = 3,
                                      question = "私は、休みます。",
                                      answer = "I get rest.",
                                      explanation = "I get rest. の後に相手のことを続けるだけ！",
                                    },
                                }
                              },
                              new GrammarLesson
                              {
                                id = 2,
                                title = "一般動詞の否定文",
                                order = 2,
                                explanation = "一般動詞の否定文を学ぼう！",
                                description = "一般動詞の否定文を学ぼう！",
                                examples = "I don't go to school. / I don't eat breakfast. / I don't sleep.",
                                questions = new List<GrammarQuestion>
                                {
                                  new GrammarQuestion
                                  {
                                    id = 4,
                                    question = "私は、学校に行きません。",
                                    answer = "I don't go to school.",
                                    explanation = "I don't go to school. の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 5,
                                    question = "私は、朝食を食べません。",
                                    answer = "I don't eat breakfast.",
                                    explanation = "I don't eat breakfast. の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 6,
                                    question = "私は、休みません。",
                                    answer = "I don't get rest.",
                                    explanation = "I don't get rest. の後に相手のことを続けるだけ！",
                                  },
                                }
                              },
                              new GrammarLesson
                              {
                                id = 3,
                                title = "一般動詞の疑問文",
                                order = 3,
                                explanation = "一般動詞の疑問文を学ぼう！",
                                description = "一般動詞の疑問文を学ぼう！",
                                examples = "Do you go to school? / Do you eat breakfast? / Do you sleep?",
                                questions = new List<GrammarQuestion>
                                {
                                  new GrammarQuestion
                                  {
                                    id = 7,
                                    question = "あなたは、学校に行きますか？",
                                    answer = "Do you go to school?",
                                    explanation = "Do you の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 8,
                                    question = "あなたは、朝食を食べますか？",
                                    answer = "Do you eat breakfast?",
                                    explanation = "Do you の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 9,
                                    question = "あなたは、休みますか？",
                                    answer = "Do you sleep?",
                                    explanation = "Do you の後に相手のことを続けるだけ！",
                                  },
                                }
                              },
                              new GrammarLesson
                              {
                                id = 4,
                                title = "自己紹介をしてみよう",
                                order = 4,
                                explanation = "自己紹介をしてみよう！",
                                description = "自己紹介をしてみよう！",
                                examples = "I'm a student. / I'm from Japan. / I'm 20 years old.",
                                questions = new List<GrammarQuestion>
                                {
                                  new GrammarQuestion
                                  {
                                    id = 10,
                                    question = "私は、学生です。",
                                    answer = "I'm a student.",
                                    explanation = "I'm の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 11,
                                    question = "私は、日本出身です。",
                                    answer = "I'm from Japan.",
                                    explanation = "I'm の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 12,
                                    question = "私は、20歳です。",
                                    answer = "I'm 20 years old.",
                                    explanation = "I'm の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 13,
                                    question = "私は、お風呂に入ります。",
                                    answer = "I take a bath.",
                                    explanation = "I take a bath. の後に相手のことを続けるだけ！",
                                  },
                                }
                              },
                              new GrammarLesson
                              {
                                id = 5,
                                title = "人の紹介をしてみよう",
                                order = 5,
                                explanation = "人の紹介をしてみよう！",
                                description = "人の紹介をしてみよう！",
                                examples = "He is a student. / She is a student. / They are students.",
                                questions = new List<GrammarQuestion>
                                {
                                  new GrammarQuestion
                                  {
                                    id = 14,
                                    question = "彼は、学生です。",
                                    answer = "He is a student.",
                                    explanation = "He is の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 15,
                                    question = "彼女は、本を読みます。",
                                    answer = "She reads books.",
                                    explanation = "She reads books. の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 16,
                                    question = "彼らは、アニメを見ます。",
                                    answer = "They watch anime.",
                                    explanation = "They watch anime. の後に相手のことを続けるだけ！",
                                  },
                                  new GrammarQuestion
                                  {
                                    id = 17,
                                    question = "彼らは、本を読みます。",
                                    answer = "They read books.",
                                    explanation = "They read books. の後に相手のことを続けるだけ！",
                                  },
                                }
                              }

                            }
                            }

                        }
                    },
                    new GrammarData {
                      id = 2,
                      title = "5W1H疑問文の森",
                      order = 2,
                      description = "5W1H疑問文を学ぼう！",
                      floors = new List<GrammarFloor>
                      {
                        new GrammarFloor
                        {
                          id = 1,
                          title = "What",
                          order = 1,
                          description = "What の疑問文を学ぼう！",
                          lessons = new List<GrammarLesson>
                          {
                            new GrammarLesson
                            {
                              id = 1,
                              title = "What の疑問文 be動詞",
                              order = 1,
                              explanation = "What の疑問文を学ぼう！",
                              description = "What の疑問文を学ぼう！",
                              examples = "What is your name? / What is your job? / What is your hobby?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 1,
                                  question = "あなたの名前は何ですか？",
                                  answer = "What is your name?",
                                  explanation = "What is your name? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 2,
                                  question = "あなたの仕事は何ですか？",
                                  answer = "What is your job?",
                                  explanation = "What is your job? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 3,
                                  question = "あなたの趣味は何ですか？",
                                  answer = "What is your hobby?",
                                  explanation = "What is your hobby? の後に相手のことを続けるだけ！",
                                },
                              }
                            },
                            new GrammarLesson {
                              id = 2,
                              title = "What の疑問文 一般動詞",
                              order = 2,
                              explanation = "What の疑問文を学ぼう！",
                              description = "What の疑問文を学ぼう！",
                              examples = "What do you want to do? / What do you like to do? / What do you do in your free time?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 4,
                                  question = "あなたは、何をしたいですか？",
                                  answer = "What do you want to do?",
                                  explanation = "What do you want to do? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 5,
                                  question = "あなたは、何をしますか？",
                                  answer = "What do you do?",
                                  explanation = "What do you do? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 6,
                                  question = "あなたは、何をしますか？",
                                  answer = "What do you do?",
                                  explanation = "What do you do? の後に相手のことを続けるだけ！",
                                },
                              }
                            }
                          }
                        },
                        new GrammarFloor
                        {
                          id = 2,
                          title = "Where",
                          order = 2,
                          description = "Where の疑問文を学ぼう！",
                          lessons = new List<GrammarLesson>
                          {
                            new GrammarLesson
                            {
                              id = 1,
                              title = "Where の疑問文 be動詞",
                              order = 1,
                              explanation = "Where の疑問文を学ぼう！",
                              description = "Where の疑問文を学ぼう！",
                              examples = "Where is your home? / Where is your school? / Where is your office?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 7,
                                  question = "あなたの家はどこですか？",
                                  answer = "Where is your home?",
                                  explanation = "Where is your home? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 8,
                                  question = "あなたの学校はどこですか？",
                                  answer = "Where is your school?",
                                  explanation = "Where is your school? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 9,
                                  question = "あなたのオフィスはどこですか？",
                                  answer = "Where is your office?",
                                  explanation = "Where is your office? の後に相手のことを続けるだけ！",
                                },
                            }
                          },
                          new GrammarLesson
                          {
                            id = 2,
                            title = "Where の疑問文 一般動詞",
                            order = 2,
                            explanation = "Where の疑問文を学ぼう！",
                            description = "Where の疑問文を学ぼう！",
                            examples = "Where do you want to go? / Where do you like to go? / Where do you go in your free time?",
                            questions = new List<GrammarQuestion>
                            {
                              new GrammarQuestion
                              {
                                id = 10,
                                question = "あなたは、何をしたいですか？",
                                answer = "Where do you want to go?",
                                explanation = "Where do you want to go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 11,
                                question = "あなたは、何をしますか？",
                                answer = "Where do you go?",
                                explanation = "Where do you go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 12,
                                question = "あなたは、何をしますか？",
                                answer = "Where do you go?",
                                explanation = "Where do you go? の後に相手のことを続けるだけ！",
                              },
                            }
                          }
                        },
                      },
                      new GrammarFloor
                      {
                        id = 3,
                        title = "When",
                        order = 3,
                        description = "When の疑問文を学ぼう！",
                        lessons = new List<GrammarLesson>
                        {
                          new GrammarLesson
                          {
                            id = 1,
                            title = "When の疑問文 be動詞",
                            order = 1,
                            explanation = "When の疑問文を学ぼう！",
                            description = "When の疑問文を学ぼう！",
                            examples = "When is your birthday? / When is your school day? / When is your office day?",
                            questions = new List<GrammarQuestion>
                            {
                              new GrammarQuestion
                              {
                                id = 13,
                                question = "あなたの誕生日はいつですか？",
                                answer = "When is your birthday?",
                                explanation = "When is your birthday? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 14,
                                question = "あなたの学校はいつですか？",
                                answer = "When is your school day?",
                                explanation = "When is your school day? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 15,
                                question = "あなたのオフィスはいつですか？",
                                answer = "When is your office day?",
                                explanation = "When is your office day? の後に相手のことを続けるだけ！",
                              },
                            }
                          },
                          new GrammarLesson
                          {
                            id = 2,
                            title = "When の疑問文 一般動詞",
                            order = 2,
                            explanation = "When の疑問文を学ぼう！",
                            description = "When の疑問文を学ぼう！",
                            examples = "When do you want to go? / When do you like to go? / When do you go in your free time?",
                            questions = new List<GrammarQuestion>
                            {
                              new GrammarQuestion
                              {
                                id = 16,
                                question = "あなたは、何をしたいですか？",
                                answer = "When do you want to go?",
                                explanation = "When do you want to go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 17,
                                question = "あなたは、何をしますか？",
                                answer = "When do you go?",
                                explanation = "When do you go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 18,
                                question = "あなたは、何をしますか？",
                                answer = "When do you go?",
                                explanation = "When do you go? の後に相手のことを続けるだけ！",
                              },
                            }
                          }
                        }
                      },
                      new GrammarFloor
                      {
                        id = 4,
                        title = "Why",
                        order = 4,
                        description = "Why の疑問文を学ぼう！",
                        lessons = new List<GrammarLesson>
                        {
                          new GrammarLesson
                          {
                            id = 1,
                            title = "Why の疑問文 be動詞",
                            order = 1,
                            explanation = "Why の疑問文を学ぼう！",
                            description = "Why の疑問文を学ぼう！",
                            examples = "Why is your birthday? / Why is your school day? / Why is your office day?",
                            questions = new List<GrammarQuestion>
                            {
                              new GrammarQuestion
                              {
                                id = 19,
                                question = "あなたの誕生日はなぜですか？",
                                answer = "Why is your birthday?",
                                explanation = "Why is your birthday? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 20,
                                question = "あなたの学校はなぜですか？",
                                answer = "Why is your school day?",
                                explanation = "Why is your school day? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 21,
                                question = "あなたのオフィスはなぜですか？",
                                answer = "Why is your office day?",
                                explanation = "Why is your office day? の後に相手のことを続けるだけ！",
                              },
                            }
                          },
                          new GrammarLesson
                          {
                            id = 2,
                            title = "Why の疑問文 一般動詞",
                            order = 2,
                            explanation = "Why の疑問文を学ぼう！",
                            description = "Why の疑問文を学ぼう！",
                            examples = "Why do you want to go? / Why do you like to go? / Why do you go in your free time?",
                            questions = new List<GrammarQuestion>
                            {
                              new GrammarQuestion
                              {
                                id = 22,
                                question = "あなたは、何をしたいですか？",
                                answer = "Why do you want to go?",
                                explanation = "Why do you want to go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 23,
                                question = "あなたは、何をしますか？",
                                answer = "Why do you go?",
                                explanation = "Why do you go? の後に相手のことを続けるだけ！",
                              },
                              new GrammarQuestion
                              {
                                id = 24,
                                question = "あなたは、何をしますか？",
                                answer = "Why do you go?",
                                explanation = "Why do you go? の後に相手のことを続けるだけ！",
                              },
                              }
                            }
                          }
                        },
                        new GrammarFloor
                        {
                          id = 5,
                          title = "Who",
                          order = 5,
                          description = "Who の疑問文を学ぼう！",
                          lessons = new List<GrammarLesson>
                          {
                            new GrammarLesson
                            {
                              id = 1,
                              title = "Who の疑問文 be動詞",
                              order = 1,
                              explanation = "Who の疑問文を学ぼう！",
                              description = "Who の疑問文を学ぼう！",
                              examples = "Who is your friend? / Who is your family? / Who is your teacher?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 25,
                                  question = "あなたの友達は誰ですか？",
                                  answer = "Who is your friend?",
                                  explanation = "Who is your friend? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 26,
                                  question = "あなたの家族は誰ですか？",
                                  answer = "Who is your family?",
                                  explanation = "Who is your family? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 27,
                                  question = "あなたの先生は誰ですか？",
                                  answer = "Who is your teacher?",
                                  explanation = "Who is your teacher? の後に相手のことを続けるだけ！",
                                },
                              }
                            },
                            new GrammarLesson
                            {
                              id = 2,
                              title = "Who の疑問文 一般動詞",
                              order = 2,
                              explanation = "Who の疑問文を学ぼう！",
                              description = "Who の疑問文を学ぼう！",
                              examples = "Who do you want to see? / Who do you like to see? / Who do you see in your free time?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 28,
                                  question = "あなたは、誰を見たいですか？",
                                  answer = "Who do you want to see?",
                                  explanation = "Who do you want to see? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 29,
                                  question = "あなたは、誰を見たいですか？",
                                  answer = "Who do you want to see?",
                                  explanation = "Who do you want to see? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 30,
                                  question = "あなたは、誰を見たいですか？",
                                  answer = "Who do you want to see?",
                                  explanation = "Who do you want to see? の後に相手のことを続けるだけ！",
                                },
                              }
                            }
                          }
                        },
                        new GrammarFloor
                        {
                          id = 6,
                          title = "How",
                          order = 6,
                          description = "How の疑問文を学ぼう！",
                          lessons = new List<GrammarLesson>
                          {
                            new GrammarLesson
                            {
                              id = 1,
                              title = "How の疑問文 be動詞",
                              order = 1,
                              explanation = "How の疑問文を学ぼう！",
                              description = "How の疑問文を学ぼう！",
                              examples = "How is your birthday? / How is your school day? / How is your office day?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 31,
                                  question = "あなたの誕生日はどうですか？",
                                  answer = "How is your birthday?",
                                  explanation = "How is your birthday? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 32,
                                  question = "あなたの学校はどうですか？",
                                  answer = "How is your school day?",
                                  explanation = "How is your school day? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 33,
                                  question = "あなたのオフィスはどうですか？",
                                  answer = "How is your office day?",
                                  explanation = "How is your office day? の後に相手のことを続けるだけ！",
                                },
                              }
                            },
                            new GrammarLesson
                            {
                              id = 2,
                              title = "How の疑問文 一般動詞",
                              order = 2,
                              explanation = "How の疑問文を学ぼう！",
                              description = "How の疑問文を学ぼう！",
                              examples = "How do you want to go? / How do you like to go? / How do you go in your free time?",
                              questions = new List<GrammarQuestion>
                              {
                                new GrammarQuestion
                                {
                                  id = 34,
                                  question = "あなたのオフィスはどうですか？",
                                  answer = "How is your office day?",
                                  explanation = "How is your office day? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 35,
                                  question = "あなたのオフィスはどうですか？",
                                  answer = "How is your office day?",
                                  explanation = "How is your office day? の後に相手のことを続けるだけ！",
                                },
                                new GrammarQuestion
                                {
                                  id = 36,
                                  question = "あなたのオフィスはどうですか？",
                                  answer = "How is your office day?",
                                  explanation = "How is your office day? の後に相手のことを続けるだけ！",
                                },
                              }
                            }
                          }
                        }
                      }
                    }
                };
    }
  }
}
