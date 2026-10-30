<p align="center">
  <img src="MyFathersWorkApp/MyFathersWorkApp/wwwroot/images/logo.png" alt="My Father's Work" width="300" />
</p>

# My Father's Work: Fan-Made Companion App

A free, fan-made companion app for the board game *My Father's Work* by Renegade Game Studios. It runs in any modern
browser on a phone, tablet or computer, with nothing to install.

### ▶️ Play now: https://vdepagter.github.io/myfathersworkapp/

## Why this exists

The official companion app is no longer available in the app stores. An
[official web app](https://renegadegamestudios.com/playmyfatherswork) still exists, but it is no longer maintained.

Renegade made the source of the original app publicly available for projects like this one. This app converts that
source into a new web app, fixes the issues found in the original stories, and adds features and extra narration.

This repository is a fork of [Deusald's fan-made companion app](https://github.com/Deusald/MyFathersWork-FanMadeCompanionApp),
which ported the first two Generations of *The Cost of Disease*. Starting from there, this fork:

- checked those existing ports passage by passage against the original source and filled the gaps it found,
- ported the rest: *The Cost of Disease* Generation III, *Fear of the Unknown* and *A Time of War*, each with scoring,
  tie-breakers and all 8 endings.

## Scenarios

| Scenario | Status |
|---|---|
| **The Cost of Disease** | ✅ Complete, played at the table |
| **Fear of the Unknown** | ✅ Complete, played at the table |
| **A Time of War** | ✅ Complete, played through in the browser; waiting for a game at the table |

## Features

**Everything the official app did**
- The full original story text of all three scenarios, for 2 to 4 players
- Setup instructions with pictures, round screens with the active rules, and the end-of-round and end-of-generation steps
- Final score entry, tie-breakers, rankings and all endings
- Music, sound effects, page transitions and the original male and female voice recordings
- The Log Book of events you have played through, now for every scenario (the official app had one for *The Cost of
  Disease* only)

**Added in this app**
- **Oliver**, a narrator who reads all story text aloud (*Fear of the Unknown* so far; the other scenarios are next).
  Private pass-the-device passages only play when the reader presses play.
- **Back (⬅)** undoes any step, all the way to the start of the game.
- **Auto-save**: close the page and continue later on the exact same screen.
- **Save and load** a game as text or a file, to continue on another device.
- **Play along with the official app**: if someone else runs the official app, this app asks for its random outcomes
  so both stay in step.
- **Endings Gallery**: the endings you have reached per scenario, remembered players and a win tally, with its own
  export and import.
- **Help** pages that explain setup, the round screens, secret pages, bids and votes.
- Separate volume sliders for music, effects and speech.
- Many fixes to the original stories: unreachable or always-true conditions, rules that contradicted each other,
  missing and duplicated text. Each deviation is written down in [`CLAUDE.md`](CLAUDE.md).

## Still to do

- Play *A Time of War* at the table.
- Oliver's narration for *The Cost of Disease* and *A Time of War*.
- Test on iPhone and iPad (Safari).

## Feedback and bug reports

Found a bug, a typo in a story, or have an idea? Please let us know on GitHub:

1. Sign in to GitHub (creating an account is free).
2. Go to the [Issues page](https://github.com/vdepagter/myfathersworkapp/issues) and press **New issue**.
3. Give it a short title and describe what happened. These details help a lot:
   - the scenario, Generation and screen you were on,
   - what you expected and what happened instead,
   - your device and browser (for example "iPhone, Safari" or "Windows laptop, Chrome"),
   - a screenshot, if you can. You can paste or drag it straight into the issue.
4. Press **Create** (or **Submit new issue**).

If your report is about a game in progress, a saved game helps us see exactly where you were: open the pause menu, press
**Local Save Game**, then **Download File** and drag that file into the issue.

No GitHub account? You can also ask in the **My Father's Work Fanmade App** Discord (link below).

## Running it on your own computer

You only need this if you want to change the app. To play, just open the link above.

1. **Install the .NET SDK.** Go to https://dotnet.microsoft.com/download, download the **.NET 9 SDK** (not just the
   runtime) for your system and run the installer. To check it worked, open a terminal (on Windows: press Start, type
   `cmd` and press Enter) and type `dotnet --version`. It should print a number starting with 9 or higher.
2. **Install Git and Git LFS.** Get Git from https://git-scm.com/downloads (the Windows installer includes Git LFS).
   On macOS or Linux, also install Git LFS from https://git-lfs.com/. Then run `git lfs install` once in a terminal.
   The images and audio are stored with Git LFS, so without it the app shows broken pictures and plays no sound.
3. **Download the app.** In the terminal, go to the folder where you want it (for example `cd Documents`) and run:
   ```
   git clone https://github.com/vdepagter/myfathersworkapp.git
   cd myfathersworkapp
   ```
   (The green **Code > Download ZIP** button on GitHub does not include the images and audio, so use `git clone`.)
4. **Start it.** In that same `myfathersworkapp` folder, run:
   ```
   dotnet run --project MyFathersWorkApp/MyFathersWorkApp/MyFathersWorkApp.csproj
   ```
   On Windows you can instead double-click `launch.bat`. The first start takes a minute.
5. **Open it** at http://localhost:5166 in your browser. Press Ctrl+C in the terminal to stop it.

To get the latest version later, run `git pull` in the `myfathersworkapp` folder.

## How the scenarios were ported

1. **Source:** the official app was built in Unity, and its stories were written in Twine and compiled to C#. The
   decompiled story code of all three scenarios (in [`source/`](source)) is the starting point.
2. **Make it readable:** helper scripts in `tools/` turn that code into readable passages with their original link texts.
3. **Map the flow:** follow the links from each Generation's start to see which passages belong where, and which
   choices lead to which branch.
4. **Rebuild:** each passage becomes a screen or a setup popup. Each round becomes a screen with that round's rules,
   and all text goes into editable `.csv` files.
5. **Verify:** scripts check that every text the code asks for exists. Every branch is then played through in the
   browser, using crafted save files to jump straight to later Generations.

Developer details (conventions, pitfalls, deliberate deviations from the original) are in [`CLAUDE.md`](CLAUDE.md).
Current work is tracked in [`HANDOFF.md`](HANDOFF.md).

## Other community projects

- [Deusald's fan-made companion app](https://github.com/Deusald/MyFathersWork-FanMadeCompanionApp), the project this
  app is based on
- [Chronicle](https://shrukan.github.io/chronicle/)
- [Masterwork](https://github.com/GameGhost/Masterwork)
- The **My Father's Work Fanmade App** Discord, also run by Deusald: https://discord.gg/QvGD44Ck44

## License and credits

*My Father's Work*, its stories, artwork, music and recordings are © Renegade Game Studios. The original app's source
is published under the [Creative Commons Attribution-NonCommercial 4.0](LICENSE) license, and this app is shared under
the same terms: free to use, share and adapt for non-commercial purposes, with credit to Renegade Game Studios. This
app changes the original: the stories were ported to a new web app, bugs were fixed, and features and narration were
added.

Oliver's narration is generated with [ElevenLabs](https://elevenlabs.io) text-to-speech (voice "Oliver Silk") on a paid
plan.

This is a non-profit fan project, made by players for players. It is not affiliated with or endorsed by Renegade Game
Studios.
