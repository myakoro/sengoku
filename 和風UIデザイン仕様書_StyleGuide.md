# 戦国立身出世SLG 0.3 和風UIデザイン仕様書（Style Guide）

## 目的
本仕様書は、Windows PCゲーム『戦国立身出世SLG 0.3』のUIデザインを統一し、実装時にデザインのブレを防ぐための基準書である。戦国時代の武家文化の雰囲気を表現した和風デザインを一貫して適用する。

---

## 1. カラーパレット（和色）

### 基本色

| 色名 | 用途 | HEX | RGB |
|:---|:---|:---|:---|
| 生成り（きなり） | 背景色（メイン） | `#f8f3e7` | rgb(248, 243, 231) |
| 墨色（すみいろ） | 文字色（メイン） | `#2b2b2b` | rgb(43, 43, 43) |
| 紅色（べにいろ） | ボタン・強調色 | `#c73940` | rgb(199, 57, 64) |
| 藍色（あいいろ） | Hover・選択状態 | `#1d3557` | rgb(29, 53, 87) |
| 木枠色（きわくいろ） | 枠線・境界線 | `#8b4513` | rgb(139, 69, 19) |

### パネル・カード色

| 色名 | 用途 | HEX | RGB |
|:---|:---|:---|:---|
| 淡生成り（あわきなり） | パネル背景 | `#f0e6d2` | rgb(240, 230, 210) |
| 木札色（きふだいろ） | ナビゲーション背景 | `#e7d8b1` | rgb(231, 216, 177) |
| 木札影（きふだかげ） | ナビゲーション下部 | `#d4c5a0` | rgb(212, 197, 160) |

### アクセント色

| 色名 | 用途 | HEX | RGB |
|:---|:---|:---|:---|
| 藍色（濃） | Hover時背景 | `#2a4a6b` | rgb(42, 74, 107) |
| 白 | 選択時文字色 | `#ffffff` | rgb(255, 255, 255) |
| 淡黄（あわき） | 注意書き背景 | `#fff8dc` | rgb(255, 248, 220) |
| 灰色 | 未実装項目 | `#888888` | rgb(136, 136, 136) |

### グラデーション

```css
/* ヘッダー・パネル用 */
background: linear-gradient(135deg, #e7d8b1 0%, #f0e6d2 100%);

/* 左ナビ用 */
background: linear-gradient(180deg, #e7d8b1 0%, #d4c5a0 100%);

/* ボタンHover用 */
background: linear-gradient(135deg, #1d3557 0%, #2a4a6b 100%);

/* ボタン通常用 */
background: linear-gradient(135deg, #e7d8b1 0%, #f0e6d2 100%);
```

---

## 2. フォント仕様

### フォントファミリー

```css
font-family: "Yu Mincho", "Hiragino Mincho ProN", "MS Mincho", serif;
```

- **日本語**: 游明朝体（Yu Mincho）を第一優先
- **代替**: ヒラギノ明朝 ProN、MS 明朝
- **英数字**: serif系で統一

### フォントサイズ階層

| 要素 | サイズ | 太さ | 行間 | 用途 |
|:---|:---|:---|:---|:---|
| 大見出し | 18px | bold | 1.5 | セクションタイトル、日付表示 |
| 中見出し | 16px | bold | 1.4 | サイドバータイトル、カテゴリ名 |
| 本文 | 14px | normal | 1.4 | ボタンテキスト、ラベル |
| 小文字 | 12px | normal | 1.3 | 注釈、補足情報 |

### フォントスタイル例

```css
/* 大見出し */
.section-title {
    font-size: 18px;
    font-weight: bold;
    line-height: 1.5;
}

/* 中見出し */
.sidebar-title {
    font-size: 16px;
    font-weight: bold;
    line-height: 1.4;
}

/* 本文 */
.body-text {
    font-size: 14px;
    font-weight: normal;
    line-height: 1.4;
}

/* 小文字 */
.caption {
    font-size: 12px;
    font-weight: normal;
    line-height: 1.3;
    font-style: italic;
    color: #666;
}
```

---

## 3. ボタンスタイル

### 通常状態

```css
.button {
    background: linear-gradient(135deg, #e7d8b1 0%, #f0e6d2 100%);
    border: 2px solid #8b4513;
    border-radius: 8px;
    padding: 15px 20px;
    color: #2b2b2b;
    font-size: 14px;
    font-weight: normal;
    cursor: pointer;
    transition: all 0.3s;
    min-width: 120px;
    text-align: center;
}
```

### Hover状態

```css
.button:hover {
    background: linear-gradient(135deg, #1d3557 0%, #2a4a6b 100%);
    color: #ffffff;
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
}
```

### Active状態

```css
.button:active {
    transform: translateY(0);
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
```

### Disabled状態

```css
.button:disabled {
    background: #e0e0e0;
    color: #888;
    cursor: not-allowed;
    border-color: #ccc;
}
```

### 札（木札）風ボタン仕様

- **角丸**: 8px（控えめな丸み）
- **枠線**: 2px solid #8b4513
- **影**: 控えめ（0 2px 4px rgba(0,0,0,0.1)）
- **Hover時の浮き**: translateY(-2px)

---

## 4. レイアウトグリッド

### Windows PC向け横長レイアウト

| 領域 | 幅/高さ | 説明 |
|:---|:---|:---|
| 左ナビ | 200px | 固定幅 |
| 中央操作領域 | flex: 1 | 可変幅（残り全て） |
| 右ステータス | 300px | 固定幅 |
| 上部日付バー | 自動 | padding: 15px 20px |

### 最小画面幅

```css
body {
    min-width: 1280px;
}
```

### Spacingルール（8pxベース）

| 用途 | サイズ |
|:---|:---|
| 極小余白 | 5px |
| 小余白 | 8px |
| 中余白 | 15px |
| 大余白 | 20px |
| 特大余白 | 30px |

```css
/* 適用例 */
.nav-item {
    padding: 12px 15px; /* 中余白 */
}

.sidebar-section {
    padding: 15px; /* 中余白 */
    margin-bottom: 20px; /* 大余白 */
}

.action-buttons {
    gap: 15px; /* 中余白 */
}
```

---

## 5. コンポーネントのデザインルール

### タブ（ナビゲーション項目）

**選択状態**
```css
.nav-item.active {
    background-color: #1d3557;
    color: #ffffff;
    font-weight: bold;
}
```

**非選択状態**
```css
.nav-item {
    background-color: transparent;
    color: #2b2b2b;
    border-bottom: 1px solid #d4c5a0;
}
```

**未実装項目**
```css
.nav-item.disabled {
    color: #888;
    font-style: italic;
    cursor: not-allowed;
}
```

### パネル（和紙風）

```css
.panel {
    background-color: #f0e6d2;
    border: 1px solid #8b4513;
    border-radius: 8px;
    padding: 15px;
    margin-bottom: 20px;
}
```

### セクションタイトル（見出し帯）

```css
.section-title {
    font-weight: bold;
    font-size: 16px;
    margin-bottom: 10px;
    color: #c73940;
    border-bottom: 1px solid #c73940;
    padding-bottom: 5px;
}
```

### 表・カード（村ステータス）

```css
.village-panel {
    background: linear-gradient(135deg, #f0e6d2 0%, #e7d8b1 100%);
    border: 2px solid #8b4513;
    border-radius: 8px;
    padding: 20px;
}

/* 村A（大）強調 */
.village-panel.village-a {
    border-color: #1d3557;
    background: linear-gradient(135deg, #e6f2ff 0%, #d1e7ff 100%);
}
```

### ログ一覧

```css
.log-area {
    min-height: 150px;
    background-color: #ffffff;
    border: 1px solid #ddd;
    border-radius: 5px;
    padding: 10px;
    font-size: 12px;
    color: #666;
}
```

### 家紋モチーフ

```css
.kamon {
    width: 40px;
    height: 40px;
    background-color: #c73940;
    border-radius: 50%;
    border: 2px solid #8b4513;
    /* 中央に「◎」などのテキストを配置 */
}
```

**配置位置**: ヘッダー左上角

---

## 6. ナビゲーションルール

### 左ナビ仕様

| 項目 | 値 |
|:---|:---|
| 幅 | 200px |
| 背景 | linear-gradient(180deg, #e7d8b1 0%, #d4c5a0 100%) |
| 枠線 | border-right: 2px solid #8b4513 |
| パディング | 20px 15px |

### ナビ項目

```css
.nav-item {
    padding: 12px 15px;
    cursor: pointer;
    border-bottom: 1px solid #d4c5a0;
    transition: background-color 0.3s;
}

.nav-item:last-child {
    border-bottom: none;
}
```

### 選択中の色

```css
.nav-item.active {
    background-color: #1d3557;
    color: #ffffff;
    font-weight: bold;
}
```

### Hover効果

```css
.nav-item:hover:not(.disabled) {
    background-color: #c73940;
    color: #ffffff;
}
```

### 未実装項目

```css
.nav-item.disabled {
    color: #888;
    font-style: italic;
}
```

---

## 7. UIトーン＆マナー（世界観の一貫性）

### デザイン方針

1. **落ち着いたトーン**: 戦国時代の武家文化を表現するため、派手な色使いを避け、落ち着いた和色を使用
2. **余白の重視**: 日本の美意識「間（ま）」を重視し、詰め込みすぎないレイアウト
3. **和紙・木枠の質感**: グラデーションと枠線で和紙・木枠の質感を表現
4. **控えめな影**: 強い影は使わず、0 2px 4px rgba(0,0,0,0.1) 程度の控えめな影
5. **統一感**: 全画面で同じカラーパレット・フォント・余白ルールを適用

### 文字間・行間

```css
/* 基本設定 */
body {
    letter-spacing: 0.05em; /* わずかな文字間 */
    line-height: 1.6; /* ゆとりのある行間 */
}

/* 見出しは詰める */
.title {
    letter-spacing: 0.1em;
}
```

### 和紙表現

- 背景色に淡い生成り色を使用
- グラデーションで微妙な濃淡を表現
- 枠線で紙の境界を明確化

### 画面全体の「間（ま）」

- コンポーネント間の余白: 15px〜20px
- セクション間の余白: 20px〜30px
- 画面端の余白: 15px〜20px

---

## 8. アクセシビリティ

### 色のコントラスト

| 組み合わせ | コントラスト比 | 評価 |
|:---|:---|:---|
| 墨色 (#2b2b2b) / 生成り (#f8f3e7) | 12.5:1 | AAA |
| 白 (#ffffff) / 藍色 (#1d3557) | 10.8:1 | AAA |
| 白 (#ffffff) / 紅色 (#c73940) | 5.2:1 | AA |

### 文字サイズ最低値

- **最小文字サイズ**: 12px
- **推奨最小サイズ**: 14px
- **重要情報**: 16px以上

### ボタンの最小押下サイズ

```css
.button {
    min-width: 120px;
    min-height: 44px; /* タッチ対応も考慮 */
    padding: 15px 20px;
}
```

---

## 9. スタイルの禁止事項

### 禁止デザイン

❌ **西洋ゲーム風のシャドウ**
```css
/* 禁止例 */
box-shadow: 0 10px 30px rgba(0,0,0,0.5);
text-shadow: 2px 2px 5px rgba(0,0,0,0.8);
```

❌ **鮮やかすぎる原色**
```css
/* 禁止例 */
color: #ff0000; /* 純粋な赤 */
background: #00ff00; /* 純粋な緑 */
```

❌ **過度なアニメーション**
```css
/* 禁止例 */
animation: spin 2s infinite;
transition: all 1s cubic-bezier(0.68, -0.55, 0.265, 1.55);
```

❌ **極端な角丸**
```css
/* 禁止例 */
border-radius: 50px; /* 大きすぎる */
border-radius: 0px; /* 完全な直角も避ける */
```

❌ **現代的UI（Material Design / Neumorphism）**
```css
/* 禁止例 */
box-shadow: 20px 20px 60px #bebebe, -20px -20px 60px #ffffff;
```

### 推奨デザイン

✅ **控えめな影**
```css
box-shadow: 0 2px 4px rgba(0,0,0,0.1);
```

✅ **和色の使用**
```css
color: #c73940; /* 紅色 */
background: #1d3557; /* 藍色 */
```

✅ **シンプルなトランジション**
```css
transition: all 0.3s;
```

✅ **適度な角丸**
```css
border-radius: 8px; /* 5px〜10px推奨 */
```

---

## 10. 実装時の注意事項

### CSS変数の活用

```css
:root {
    /* カラーパレット */
    --color-bg-main: #f8f3e7;
    --color-text-main: #2b2b2b;
    --color-accent: #c73940;
    --color-hover: #1d3557;
    --color-border: #8b4513;
    
    /* スペーシング */
    --spacing-xs: 5px;
    --spacing-sm: 8px;
    --spacing-md: 15px;
    --spacing-lg: 20px;
    --spacing-xl: 30px;
    
    /* フォント */
    --font-size-lg: 18px;
    --font-size-md: 16px;
    --font-size-base: 14px;
    --font-size-sm: 12px;
}
```

### レスポンシブ対応（将来用）

本バージョン（0.3）ではWindows PC専用だが、将来的な拡張を考慮し、固定値ではなく相対値を使用することを推奨。

### ブラウザ互換性

- Chrome/Edge: 最新版
- Firefox: 最新版
- Safari: 考慮不要（Windows専用）

---

## 11. デザインチェックリスト

実装時に以下を確認すること：

- [ ] カラーパレットの色を使用しているか
- [ ] フォントは游明朝体系か
- [ ] ボタンのHover効果が実装されているか
- [ ] 余白ルール（8pxベース）に従っているか
- [ ] 角丸は5px〜10pxの範囲か
- [ ] 影は控えめか（0 2px 4px程度）
- [ ] 禁止デザインを使用していないか
- [ ] 文字サイズは12px以上か
- [ ] ボタンの最小サイズは120px × 44pxか

---

## 改訂履歴

| バージョン | 日付 | 変更内容 |
|:---|:---|:---|
| 1.0 | 2025-11-29 | 初版作成 |

---

**本仕様書は『戦国立身出世SLG 0.3』のUIデザイン統一のための公式Style Guideである。**
