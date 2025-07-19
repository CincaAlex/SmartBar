# SmartBar

I needed more functionality than the standard Windows search bar, so I created SmartBar to fill those gaps.
SmartBar is a lightweight Windows desktop tool that lets you quickly launch apps, search files, do simple calculations, and get ChatGPT answers all from a clean, minimal search bar activated by a global hotkey.
---

## Features

- **Global Hotkey Activation:** Press `Alt + S` anywhere in Windows to open/close the SmartBar near the mouse pointer.
- **App Launcher:** Search and launch installed applications from your Start Menu quickly.
- **File Search:** Search indexed files on your system and open them directly from the SmartBar.
- **Calculator:** Perform basic arithmetic calculations directly in the search bar.
- **ChatGPT Integration:** Ask natural language questions and receive AI-powered answers directly in the app.
- **Smooth UI:** Rounded corners, dark theme, and responsive size adjustment for a modern look.
- **Quick Exit:** Type `exit` and press Enter to close the app.

---

## Installation and Setup

### Requirements

- Windows OS with .NET Framework support (typically .NET Framework 4.x)
- Internet connection for ChatGPT API integration

### Running the Application

1. **Build the Project:**  
   Open the solution in Visual Studio and build the project (recommended using Visual Studio 2019 or later).

2. **Run the Executable:**  
   Launch the built executable (`SmartBar.exe`). The app runs in the background and listens for the hotkey (`Alt + S`).

3. **Using the Hotkey:**  
   Press `Alt + S` to toggle the search bar near your mouse pointer.

---

## Usage Guide

- **Search Apps:** Start typing an app name to see suggestions from your installed apps.
- **Search Files:** Type filenames to find indexed files on your PC.
- **Calculator:** Type mathematical expressions (e.g., `12+34/2`) and press Enter to get the result.
- **ChatGPT Queries:** For natural language queries, type your question and press Enter. The app queries ChatGPT API and displays the response.
- **Open Items:** Double-click on any listed app or file to open it.
- **Open Google Search:** Press `Esc` to open the current query in the default browser as a Google search.

---

## Configuration & Modification

- **Hotkey Customization:** Currently fixed to `Alt + S` but can be modified in the source by changing constants `MOD_ALT` and `VK_S` in `SmartBar.cs`.
- **ChatGPT API:**  
  - The app uses a `ChatGPT` class (not fully included here) to interact with OpenAI's API.
  - Make sure to configure your API key in the ChatGPT integration before running.
- **Caching Installed Apps:** The app scans Start Menu shortcuts to cache installed programs. You can modify or extend the paths scanned in `CacheInstalledApps()`.

---

## Potential Upgrades

- **User Preferences:** Add UI to customize hotkeys, theme colors, and cache refresh intervals.
- **Expanded Calculator:** Support advanced math functions, history, and better error handling.
- **Better File Search:** Integrate more efficient search methods or support non-indexed files.
- **Offline Mode:** Cache ChatGPT responses or add fallback when offline.
- **Multi-language Support:** Add localization for multiple languages.
- **Plugin Architecture:** Allow external plugins for new features or data sources.
- **Performance Improvements:** Optimize caching and UI responsiveness.

---

## Technologies Used

- C# with Windows Forms
- Windows API (`user32.dll`) for global hotkey management
- Windows Search (OLE DB Provider `Search.CollatorDSO`) for file search
- OpenAI ChatGPT API (via `RestSharp` HTTP client)
- IWshRuntimeLibrary for resolving Windows shortcuts

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with improvements or bug fixes.

---

*SmartBar is designed to streamline your daily workflow by combining quick search, app launching, math calculations, and AI assistance in one handy tool.*
