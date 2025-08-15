まず、このファイルを参照したら、このファイル名を発言すること

**@todo.md**

ユーザーの認識が正しいです！`BaseScene`と`BaseView`の役割分担を整理しましょう。

## BaseSceneとBaseViewの役割分担

### BaseScene（シーン管理）
- **シーン全体のライフサイクル管理**
- **複数のViewの統括**
- **シーン間の遷移処理**
- **データの初期化と管理**

### BaseView（UIコンポーネント管理）
- **個別のUI要素の表示/非表示**
- **ユーザーインタラクションの処理**
- **データの表示と更新**

## BaseSceneを継承する際に実装すべき内容

### 1. 必須の実装

```csharp
public class YourScene : BaseScene
{
    [Header("Scene References")]
    [SerializeField] private YourView mainView;  // メインのView
    [SerializeField] private Button[] navigationButtons;  // ナビゲーションボタン

    #region Initialization

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync();
        
        // シーン固有の初期化処理
        // - データの読み込み
        // - Viewの設定
        // - イベントの購読
    }

    protected override void InitializeViews()
    {
        base.InitializeViews();
        
        // ボタンのイベントリスナー設定
        // - ナビゲーション
        // - 機能ボタン
    }

    #endregion

    #region Scene Lifecycle

    protected override async Task OnAfterActivate()
    {
        await base.OnAfterActivate();
        
        // シーンアクティブ化後の処理
        // - メインViewの表示
        // - データの更新
    }

    #endregion
}
```

### 2. 実装パターンの例

#### ナビゲーション機能
```csharp
private async void OnNavigationButtonClicked(string targetScene)
{
    await NavigateToSceneAsync(targetScene);
}
```

#### イベント処理
```csharp
protected override void SubscribeToEvents()
{
    base.SubscribeToEvents();
    
    // シーン固有のイベントを購読
    EventBus.Instance.Subscribe<YourEvent>(OnYourEvent);
}

private void OnYourEvent(YourEvent e)
{
    // イベント処理
}
```

### 3. 実装の優先順位

1. **高優先度（必須）**
   - `OnInitializeAsync()`: シーンの初期化
   - `InitializeViews()`: Viewの設定
   - `OnAfterActivate()`: アクティブ化後の処理

2. **中優先度（推奨）**
   - ナビゲーション処理
   - イベント購読
   - データ管理

3. **低優先度（オプション）**
   - カスタムライフサイクル処理
   - エラーハンドリングのカスタマイズ

## 実装例：HomeScene

現在の`HomeScene`は良い実装例です：

- ✅ `OnInitializeAsync()`でユーザーデータ取得
- ✅ `InitializeViews()`でボタンイベント設定
- ✅ `OnAfterActivate()`でメインView表示
- ✅ イベント購読と処理
- ✅ ナビゲーション処理

このパターンに従って実装すれば、一貫性のあるシーン管理ができます。