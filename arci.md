# 1. プロジェクト固有のアセットを管理するメインフォルダ
mkdir _LingoAsobi

# 2. アート関連のアセット
mkdir -p _LingoAsobi/Art/Animations         # アニメーションファイル (.anim)
mkdir -p _LingoAsobi/Art/Fonts             # フォントファイル (.ttf, .otf)
mkdir -p _LingoAsobi/Art/Materials         # マテリアル (.mat)
mkdir -p _LingoAsobi/Art/Shaders           # シェーダーファイル (.shader)
mkdir -p _LingoAsobi/Art/Sprites           # スプライト、画像ファイル (.png, .jpg)
mkdir -p _LingoAsobi/Art/UI                # UI用の画像素材

# 3. オーディオ関連のアセット
mkdir -p _LingoAsobi/Audio/Music           # BGM
mkdir -p _LingoAsobi/Audio/SFX             # 効果音 (Sound Effects)

# 4. プレハブ (再利用可能なオブジェクト)
mkdir -p _LingoAsobi/Prefabs/Characters    # キャラクターのプレハブ
mkdir -p _LingoAsobi/Prefabs/UI            # UI要素のプレハブ (ボタン、パネルなど)
mkdir -p _LingoAsobi/Prefabs/Environment   # 背景やステージ要素のプレハab

# 5. シーン (画面)
mkdir -p _LingoAsobi/Scenes/_Levels        # ゲームのレベルやステージのシーン
mkdir -p _LingoAsobi/Scenes/_System        # タイトル、ホームなどのシステム画面シーン

# 6. スクリプト (C#のコード)
mkdir -p _LingoAsobi/Scripts/Gameplay      # ゲームプレイのコアロジック
mkdir -p _LingoAsobi/Scripts/Managers      # ゲーム全体を管理するマネージャークラス
mkdir -p _LingoAsobi/Scripts/UI            # UIの挙動を制御するスクリプト
mkdir -p _LingoAsobi/Scripts/Data          # データ構造を定義するクラス (マスターデータなど)
mkdir -p _LingoAsobi/Scripts/Utilities     # 汎用的な便利クラス

# 7. テスト用のスクリプト (Unity Test Framework)
mkdir -p _LingoAsobi/Tests/EditMode        # Edit Modeのテスト
mkdir -p _LingoAsobi/Tests/PlayMode        # Play Modeのテスト

# 8. サードパーティ製のアセット (Asset Storeなど)
mkdir _ThirdParty
# 例: DOTweenをこの中に入れる
# mkdir _ThirdParty/DOTween

# 9. Unityが特別に扱うフォルダ (ルートに配置)
mkdir Editor                             # Unityエディタを拡張するスクリプト用
mkdir Plugins                            # ネイティブプラグイン用
mkdir Resources                          # Resources.Loadで動的に読み込むアセット用
mkdir StreamingAssets                    # ビルド時にそのままコピーされるファイル用

# 完了メッセージ
echo "✅ Unityプロジェクトのディレクトリ構成を作成しました。"

