# Windsurf Prompt: v0.9 軍事システム UI実装 (XAML)

以下の8つのWPF XAML Viewファイルは現在プレースホルダー（「開発中」表示のみ）の状態です。
これらを、あなたが作成した `UIモック_HTML/v0.9` のHTMLデザインに基づいて、完全なXAMLとして実装してください。

## 対象ファイルと対応するHTMLモック

1. **MilitaryFormationView.xaml**
   - 対応モック: `01_軍事編成画面.html`
   - 機能: 出陣前の軍事編成、大隊・中隊・小隊の構成確認と変更。

2. **SquadStatusPanelView.xaml**
   - 対応モック: `02_隊状態パネル.html`
   - 機能: 戦闘中や編成時の各部隊（Squad）のステータス表示（兵数、士気、疲労など）。

3. **BattlefieldUIView.xaml**
   - 対応モック: `03_戦場UI.html`
   - 機能: 戦闘メイン画面。戦況ログ、部隊配置、コマンドボタン（攻撃、移動など）。

4. **CasualtyReportView.xaml**
   - 対応モック: `04_兵士損耗レポート.html`
   - 機能: 戦闘後の損害報告。死傷者数、部隊ごとの損耗状況。

5. **BattalionManagementView.xaml**
   - 対応モック: `08_大隊管理画面.html`
   - 機能: 大隊（Battalion）レベルの詳細管理と指揮官設定。

6. **CompanyManagementView.xaml**
   - 対応モック: `09_中隊管理画面.html`
   - 機能: 中隊（Company）レベルの詳細管理。

7. **SquadDetailView.xaml**
   - 対応モック: `10_小隊詳細画面.html`
   - 機能: 小隊（Squad）の詳細ステータス、装備、兵種設定。

8. **PursuitDecisionView.xaml**
   - 対応モック: `11_追撃判定画面.html`
   - 機能: 敗走する敵部隊への追撃を行うかどうかの意思決定画面。

## 実装要件

1. **デザインの再現**:
   - HTMLモックのレイアウト、配色、フォントサイズを可能な限りWPF XAMLで再現してください。
   - 既存の `App.xaml` で定義されているリソース（`BrushBgBase`, `TextHeader`, `ButtonPrimary` など）を積極的に使用し、アプリ全体の統一感を保ってください。

2. **データバインディング**:
   - 各Viewに対応するViewModelは `SengokuSLG.ViewModels` 名前空間に既に作成済みです（例: `MilitaryFormationViewModel`）。
   - ボタンの `Command` バインディング（例: `BackCommand`）は既にプレースホルダーに含まれているので、維持してください。
   - リスト表示などは `ItemsControl` や `DataGrid` を使用し、適切なバインディングパス（例: `{Binding Squads}`}）を想定して記述してください（ViewModel側のプロパティ実装は後で行いますが、View側は完成形を目指してください）。

3. **レイアウト構造**:
   - `Grid` をメインのレイアウトコンテナとして使用し、必要に応じて `StackPanel` や `DockPanel` を組み合わせてください。
   - レスポンシブ性よりも、固定サイズ（DesignHeight=720, DesignWidth=1080想定）でのレイアウト崩れがないことを優先してください。

## 出力形式

各ファイルごとの完全なXAMLコードを出力してください。
既存の `UserControl` 定義や名前空間の宣言は維持してください。
