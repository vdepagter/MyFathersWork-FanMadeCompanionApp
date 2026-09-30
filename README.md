## 🧪 My Father's Work: Fan-Made Companion App

This is a fan-made web and console-based companion app for *My Father’s Work* — created by the community to enhance the gameplay experience beyond the limitations of the official app by Renegade.

### ✨ Why this exists
The official app for *My Father's Work* lacks essential features, contains errors, and doesn’t support community-driven improvements. This project was built to:

- Provide a **fully accurate version** of all scenarios, including original text
- Offer an **abridged version** of the narrative for quicker play sessions
- Add functionality, fix bugs, and allow for more consistent gameplay
- Make it easier to bring *My Father’s Work* back to the table

### 📍 Where the project stands

| Scenario | Status |
|---|---|
| **The Cost of Disease** | ✅ Playable from setup to the final ending: all 3 Generations, every story branch, final scoring, tie-breakers and all 8 endings |
| **Fear of the Unknown** | ❌ Not started (selecting it returns to the main menu) |
| **A Time of War** | ❌ Not started |

**Still to do**
- Port **Fear of the Unknown** and **A Time of War**. Their story text is available (in `MyFathersWorkConsole/.../Processor/*.txt`), but parts of the app are still hard-wired to The Cost of Disease and need generalizing first. Both also rely on extra mechanics, such as a battle and time travel.
- Share the final scoring and tie-breaker screens between scenarios (they currently live inside The Cost of Disease).
- Abridged (TL;DR) story option.
- More languages: the menus exist in English and Polish, the story text only in English.
- Two open questions for The Cost of Disease: the wording of the second tie-breaker step, and one building-tile rule in Generation III.
- Nice-to-haves from the official app: endings and achievements gallery, sharing, voice-over.

### 🎲 Using the app

**Playing a game**
1. **Home**: *New Game*, *Continue* (the last auto-save) or *Load Game* (paste or upload a save).
2. **Players**: choose 2–4 players and enter each scientist's name.
3. **Village**: name the town your family inherits.
4. **Scenario**: pick a scenario, then the language of the story.
5. **Gameplay**: read the story and follow the setup popups. Each round has a screen with the rules that are active, where you click links when something happens during play. The top bar shows the current round (I‑1 … III‑3). Use ⬅ to undo a step and ⏸ to pause, save or return to the menu.
6. At the end of Generation III, enter everyone's score. The app resolves any tie, shows the rankings and reveals your ending.

The game saves itself in the browser whenever you return to a round screen.

**Running locally**
1. Install the [.NET SDK](https://dotnet.microsoft.com/download) and [Git LFS](https://git-lfs.com/). The images are stored with LFS, so run `git lfs pull` after cloning.
2. `dotnet run --project MyFathersWorkWebApp/MyFathersWorkWebApp/MyFathersWorkWebApp.csproj`
3. Open http://localhost:5166

### 🔧 How the scenarios are ported
1. **Source:** the official app was built in Unity, and its stories were written in Twine and compiled to C#. The decompiled story code for all three scenarios is the starting point.
2. **Make it readable:** the console project (`MyFathersWorkConsole`) converts that code into plain, readable passages. Helper scripts in `tools/` show any passage with its original link texts.
3. **Map the flow:** follow the links from each Generation's start to see which passages belong where, and which choices lead to which branch.
4. **Rebuild:** each passage becomes a screen or a setup popup in the web app. Each round becomes a screen with that round's rules, and all text goes into editable `.csv` files.
5. **Verify:** a script checks that every text the code asks for exists. Every branch is then played through in the browser, using crafted save files to jump straight to later Generations.

Developer details (conventions, pitfalls, how to add the next scenario) are in [`CLAUDE.md`](CLAUDE.md).

**Challenges encountered**
- **No real source code for the stories:** the story logic only exists as decompiled, machine-generated code. The console conversion also lost many link texts, which had to be recovered.
- **Bugs in the original stories:** some conditions are always true or can never be reached, and a few rules contradict each other. Each case needed a deliberate choice between copying the original and fixing it.
- **Randomness, undo and saves:** every random roll is decided in advance, so undoing a step or loading a save replays exactly the same result.
- **Rules outside the story:** scoring and tie-breakers were separate screens in the original app, and some of their rules were never written down.
- **Gaps in the earlier port:** checking completeness turned up missing and duplicated texts in Generations I and II, which are now fixed.

### 📚 Features
- Original scenario text, with abridged (TL;DR) options planned
- Logic scripting for decision trees and player interactions
- Easy-to-edit scenario files stored in readable `.csv` format
- Undo, auto-save and shareable save files
- GitHub-based collaboration — open to community contributions!
- Clean UI version to run on any browser

### 🤝 Community-Driven
- All help with translating content into other languages is welcome!
- Instructions and shared Google Sheets will guide contributors.
- The project aims to remain respectful to the original IP, while filling gaps the publisher has left.

### ⚠️ Disclaimer
This is a nonprofit fan project created by players, for players. We believe this app helps promote and improve the game, not compete with it.
