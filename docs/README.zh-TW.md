# 柏德之門3語言合併器

柏德之門3語言合併器是一個設計用於在《柏德之門 3》這款遊戲中將兩種語言合併在一起的程式。

![柏德之門3語言合併器介面](imgs/merger_screenshot.zh-Hant.webp)

## 功能特點

### 合併分類

這個工具允許您合併來自不同分類的字串：

- 對話
- 書
- 物品
- 狀態
- 角色
- 任務
- 小提示
- 其他提示
- 雜項

### 無條件合併

你也可以選擇將遊戲中的**所有**字串合併。這會導致文字在某些使用者界面中溢出。請謹慎使用。

## 安裝

按照以下步驟安裝並使用這個工具：

1. 下載[最新版本](/../../releases/latest)。
2. 將下載的文件解壓縮到您選擇的位置。
3. 執行 `BG3LocalizationMerger.exe`。

## 使用方法

### 1. 解壓縮官方套件

_如果您是無條件合併，可以跳過這一步。_

按照以下說明繼續：

- 安裝《BG3 Modder's Multitool》：
  - 下載並運行 [BG3 Modder's Multitool](https://github.com/ShinyHobo/BG3-Modders-Multitool/releases)。
  - 點擊選單的 **Configuration (配置)** 來完成基本配置。
- 通過選擇 **Utilities/Game File Operations/Unpack Game Files (解包.pak文件)** 來解包以下套件：
  - `Game.pak`
  - `Gustav.pak`
  - `Shared.pak`
  - `Patch*.pak`
- 解包後，點擊 **Utilities/Game File Operations/Decompress UnpackedData Files (解壓文件)** 並等待處理完成。
- 關閉這個工具。
- 解包的數據可以在 `[BG3 Modder's Multitool 資料夾]/UnpackedData` 找到。

### 2. 完成柏德之門3語言合併器的設定

按照以下步驟填寫：

- **已解壓縮檔案資料夾**：提供從[步驟 1](#1-解壓縮官方套件)中的 `UnpackedData` 資料夾的路徑。
  - 如果您是無條件合併，請跳過這一步。
- **語言包**：找到遊戲語言的檔案。
  - 在 `[BG3 遊戲資料夾]/Data/Localization` 中找到這個檔案。
  - 例如：`[BG3 遊戲資料夾]/Data/Localization/ChineseTraditional/ChineseTraditional.pak`。
  - 註：如果您是使用第三方的語言包（例如巴哈修正版），請直接選第三方的語言包檔案即可。
- **次要語言包**：選擇第二個語言包。
  - 例如：`[BG3 遊戲資料夾]/Data/Localization/English.pak`。
- **輸出檔案路徑**：指定保存合併後語言包的位置。
  - 請現將合併的檔案放在一個暫存的資料夾，之後再用這個檔案覆蓋原本遊戲的語言包檔案。
- 選擇你要合併的分類。

### 3. 開始合併
- 點擊 **合併** 並等待處理完成。
  - 如果您正在進行無條件合併，請改點擊 **無條件合併**。

### 4. 用輸出的檔案取代掉原本遊戲的語言包
  - 例如：`[BG3 遊戲資料夾]/Data/Localization/ChineseTraditional/ChineseTraditional.pak`。

### 5. 開始遊戲並檢視結果

就這樣！如果有遇到Bug或有其他建議，可以[查看現有的問題](/../../issues)或[提出新問題](/../../issues/new)。

## 更新說明

為了確保語言包維持在最新狀態，每次遊戲更新後，你可能會需要重新進行合併：

- 如果更新是個 Hotfix，你只需要解包並解壓縮最新的 Hotfix 檔案。例：`Hotfix5_Patch6.pak`

- 如果更新是個主要的 Patch，你需要先將 `UnpackedData` 資料夾裡的 `Patch*_Hotfix*` 資料夾全部移除，然後再重新執行[所有的步驟](#使用方法)。

**如果您是使用第三方的語言包，請在第三方的語言包更新時，重新[選擇最新的語言包](#2-完成柏德之門3語言合併器的設定)並[合併](#3-開始合併)且覆蓋之前合併的語言包。**


## 截圖

### 對話
![Merged dialogue](imgs/dialog_screenshot.webp)

### 書
![A book](imgs/books_screenshot.webp)

### 小提示
![Hints on loading screen](imgs/hints_screenshot.webp)

### 物品
![An item popup](imgs/item_screenshot.webp)

### 任務
![A quest in the Journal](imgs/quest_screenshot.webp)

### 狀態
![Character status](imgs/status_screenshot.webp)