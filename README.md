# BG3 Localization Merger

BG3 Localization Merger is a program designed to merge two languages together within Baldur's Gate 3, a video game.

[中文說明](docs/README.zh-TW.md)

![BG3 Localization Merger interface](docs/imgs/merger_screenshot.webp)

## Features

### Merge Categories

This tool allows you to merge strings from various categories:

- Dialogues
- Books
- Items
- Status
- Characters
- Quests
- Tooltips
- Hints
- Miscs

### Merge Unconditionally

A cautionary option is available to merge **all** strings in the game. This can cause overflow in some user interfaces. Use with care.

## Installation

Follow these steps to install and use the tool:

1. Download the [latest release](/../../releases/latest).
2. Extract the downloaded files to a location of your choice.
3. Run `BG3LocalizationMerger.exe`.

## Usage

### 1. Unpack the Official Packages

_You can skip this step if you are merging unconditionally._

To proceed, follow these instructions:

- Install BG3 Modder's Multitool:
  - Download and run [BG3 Modder's Multitool](https://github.com/ShinyHobo/BG3-Modders-Multitool/releases).
  - Complete the basic configuration by clicking the **star icon** in the bottom right corner.
  - If necessary, download [LSLib](https://github.com/Norbyte/lslib/releases) first.
  - Only the LSLib `divine.exe` and `bg3.exe` fields are required.
- Unpack the following packages by selecting **Unpack .pak Files**:
  - `Game.pak`
  - `Gustab.pak`
  - `Shared.pak`
  - Additionally, unpack all available patch files (`Patch*.pak`).
- After unpacking, click **Decompress Files** and wait for the process to complete.
- Close the tool.
- The unpacked data can be found in `[BG3 Modder's Multitool Folder]/UnpackedData`.

### 2. Fill in the Fields in BG3 Localization Merger

Proceed with these steps to fill in the required information:

- **Unpacked Data Folder**: Provide the path to the `UnpackedData` folder from [step 1](#1-unpack-the-official-packages).
  - Skip this if you are merging unconditionally.
- **Language Pack**: Locate the localization pack file for your game language.
  - Find this file in `[BG3 Game Folder]/Data/Localization`.
  - Example: `[BG3 Game Folder]/Data/Localization/English.pak`.
- **Reference Language Pack**: Select the second language pack.
  - Example: `[BG3 Game Folder]/Data/Localization/ChineseTraditional/ChineseTraditional.pak`.
- **Export File Path**: Specify the location to save the merged language pack.
  - It's recommended to place it in the `[BG3 Game Folder]/Data` directory to ensure it overrides the original language packs.
  - Example: `[BG3 Game Folder]/Data/MergedLocalization.pak`.
- Toggle the categories you wish to merge.

### 3. Start Merging
- Click **Merge** and wait for the process to complete.
  - If you're merging unconditionally, click **Merge Everything** instead.

### 4. Launch the Game and Review the Results

That's it! If you encounter any issues or have any suggestions, please consider [checking for existing issues](/../../issues) or [opening a new issue](/../../issues/new).

## Updating

To ensure the language pack stays up-to-date, follow these steps whenever a new patch is released:

1. [Unpack and decompress](#1-unpack-the-official-packages) new latest patch file (Patch*.pak).
2. Overwrite the previous merged pack by [merging again](#3-start-merging)


## Screenshots

### Dialogues
![Merged dialogue](docs/imgs/dialog_screenshot.webp)

### Books
![A book](docs/imgs/books_screenshot.webp)

### Hints
![Hints on loading screen](docs/imgs/hints_screenshot.webp)

### Items
![An item popup](docs/imgs/item_screenshot.webp)

### Quests
![A quest in the Journal](docs/imgs/quest_screenshot.webp)

### Status
![Character status](docs/imgs/status_screenshot.webp)