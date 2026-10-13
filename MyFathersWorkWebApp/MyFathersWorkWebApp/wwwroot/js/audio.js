// Music and sound effects (docs/AUDIO_TRANSITIONS_MAP.md 5.1.6), driven from C# by Shared/AudioService.cs.
// Channels as in the Unity SoundManager:
//   music   - one element, loops (except the ending track), only switches when the wanted track differs
//   effects - one at a time, a new effect cuts the previous one (screen transitions, welcome, footsteps)
//   popup   - one at a time, a new popup sound cuts the previous one (setup, end of round, special event)
//   click   - may overlap
// Browsers block audio until the first user gesture: the wanted music starts on the first gesture, effects requested
// before it are dropped. Paths are relative so they resolve against <base href> (the site runs from a sub-path).
(function () {
    "use strict";

    const EFFECTS = ["sfx-click", "sfx-transition-cover", "sfx-transition-reveal", "sfx-setup", "sfx-end-of-round",
                     "sfx-special-event", "sfx-welcome", "sfx-footsteps"];
    const MUSIC_FADE_MS = 400;
    const KEY_EFFECTS   = "audio.effects";
    const KEY_MUSIC     = "audio.music";

    const src = name => "audio/" + name + ".mp3";

    function readSetting(key) {
        try {
            return localStorage.getItem(key) !== "0";
        } catch (e) {
            return true;
        }
    }

    function writeSetting(key, on) {
        try {
            localStorage.setItem(key, on ? "1" : "0");
        } catch (e) {
            // Storage unavailable (private mode, blocked): keep the setting for this session only
        }
    }

    function tryPlay(element) {
        try {
            const promise = element.play();
            if (promise && promise.catch) promise.catch(() => { });
        } catch (e) {
            // Ignore: autoplay blocked or unsupported format
        }
    }

    function stop(element) {
        if (!element) return;
        try {
            element.pause();
            element.currentTime = 0;
        } catch (e) {
            // Ignore
        }
    }

    const settings = { effects: readSetting(KEY_EFFECTS), music: readSetting(KEY_MUSIC) };
    let unlocked = false;

    // ---- Effects --------------------------------------------------------------------------------------------------

    const clips = {};
    for (const name of EFFECTS) {
        const element   = new Audio();
        element.preload = "auto";
        element.src     = src(name);
        clips[name]     = element;
    }

    const channels = {
        effects: { current: null, timer: 0 },
        popup:   { current: null, timer: 0 }
    };

    function startOnChannel(channel, name) {
        const clip = clips[name];
        if (!clip || !settings.effects) return;
        if (channel.current && channel.current !== clip) stop(channel.current);
        channel.current = clip;
        try {
            clip.currentTime = 0;
        } catch (e) {
            // Ignore: not loaded yet
        }
        tryPlay(clip);
    }

    // channel: "effects" | "popup" | "click"; delayMs: start later (a newer request on the channel cancels it)
    function play(name, channelName, delayMs) {
        if (!settings.effects || !unlocked || !clips[name]) return;

        if (channelName === "click") {
            tryPlay(clips[name].cloneNode());
            return;
        }

        const channel = channels[channelName];
        if (!channel) return;
        clearTimeout(channel.timer);
        channel.timer = 0;

        if (delayMs > 0) {
            channel.timer = setTimeout(() => {
                channel.timer = 0;
                startOnChannel(channel, name);
            }, delayMs);
            return;
        }

        startOnChannel(channel, name);
    }

    function stopEffects() {
        for (const channel of Object.values(channels)) {
            clearTimeout(channel.timer);
            channel.timer = 0;
            stop(channel.current);
            channel.current = null;
        }
    }

    // ---- Music ----------------------------------------------------------------------------------------------------

    const music   = new Audio();
    music.preload = "none";

    let wanted      = null;      // { name, loop } or null (silence)
    let currentName = null;      // track loaded in the music element
    let switchingTo;             // track name (or null) a fade is switching to; undefined when no switch runs
    let fadeTimer   = 0;
    let musicEnded  = false;     // a non-looping track played to the end: do not restart it

    music.addEventListener("ended", () => { musicEnded = true; });

    function cancelFade() {
        clearInterval(fadeTimer);
        fadeTimer   = 0;
        switchingTo = undefined;
    }

    function fadeOut(done) {
        if (music.paused || currentName === null) {
            done();
            return;
        }

        const step = 1 / (MUSIC_FADE_MS / 40);
        fadeTimer = setInterval(() => {
            const volume = Math.max(0, music.volume - step);
            music.volume = volume;
            if (volume > 0) return;
            clearInterval(fadeTimer);
            fadeTimer = 0;
            done();
        }, 40);
    }

    function applyMusic() {
        if (!unlocked) return;

        if (!settings.music) {
            cancelFade();
            music.pause();
            music.volume = 1;
            return;
        }

        const targetName = wanted ? wanted.name : null;
        if (switchingTo !== undefined && switchingTo === targetName) return;

        if (switchingTo === undefined && currentName === targetName) {
            if (targetName !== null && music.paused && !musicEnded) tryPlay(music);
            return;
        }

        cancelFade();
        switchingTo  = targetName;
        const target = wanted;

        fadeOut(() => {
            switchingTo = undefined;
            music.pause();
            music.volume = 1;
            musicEnded   = false;

            if (!target) {
                currentName = null;
                music.removeAttribute("src");
                return;
            }

            currentName = target.name;
            music.loop  = target.loop;
            music.src   = src(target.name);
            tryPlay(music);
        });
    }

    // name: track file name without extension, or null/"" for silence
    function setMusic(name, loop) {
        const targetName = name || null;
        if ((wanted ? wanted.name : null) === targetName) return;
        wanted = targetName ? { name: targetName, loop: loop !== false } : null;
        applyMusic();
    }

    // ---- Unlock on the first user gesture -------------------------------------------------------------------------

    function onGesture() {
        unlocked = true;
        applyMusic();
    }

    for (const type of ["pointerdown", "pointerup", "keydown", "touchend"]) {
        document.addEventListener(type, onGesture, { capture: true, passive: true });
    }

    // ---- API ------------------------------------------------------------------------------------------------------

    window.mfwAudio = {
        play: play,
        setMusic: setMusic,
        isEffectsOn: () => settings.effects,
        isMusicOn: () => settings.music,
        setEffectsOn: on => {
            settings.effects = !!on;
            writeSetting(KEY_EFFECTS, settings.effects);
            if (!settings.effects) stopEffects();
        },
        setMusicOn: on => {
            settings.music = !!on;
            writeSetting(KEY_MUSIC, settings.music);
            applyMusic();
        },
        // Read-only snapshot for debugging
        state: () => ({
            unlocked: unlocked,
            effectsOn: settings.effects,
            musicOn: settings.music,
            wantedMusic: wanted ? wanted.name : null,
            currentMusic: currentName,
            musicPaused: music.paused,
            musicLoop: music.loop
        })
    };
})();
