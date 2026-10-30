// Music and sound effects (docs/AUDIO_TRANSITIONS_MAP.md 5.1.6), driven from C# by Shared/AudioService.cs.
// Channels as in the Unity SoundManager:
//   music   - one element, loops (except the ending track), only switches when the wanted track differs
//   effects - one at a time, a new effect cuts the previous one (screen transitions, welcome, footsteps)
//   popup   - one at a time, a new popup sound cuts the previous one (setup, end of round, special event)
//   click   - may overlap
//   voice   - one element for the voice-over: thunder lead-in, then the narration clip (docs/AUDIO_TRANSITIONS_MAP.md
//             "Voice-over"), or the clips of the fan-made Oliver narration played one after the other. Own setting
//             (female / male / oliver / off), independent of the music and effects toggles; the
//             music is ducked while it plays. State changes are reported to one .NET listener (Pages/Gameplay.razor).
// Volumes (pause menu sliders): music, effects and speech, 0-100 each, stored per device. The effects volume scales
// every effect (page flips stay at 60% of it), the speech volume the narration and its thunder lead-in.
// Browsers block audio until the first user gesture: the wanted music starts on the first gesture, effects requested
// before it are dropped. Paths are relative so they resolve against <base href> (the site runs from a sub-path).
(function () {
    "use strict";

    const EFFECTS = ["sfx-click", "sfx-transition-cover", "sfx-transition-reveal", "sfx-setup", "sfx-end-of-round",
                     "sfx-special-event", "sfx-welcome", "sfx-footsteps"];
    const VOLUMES       = { "sfx-transition-cover": 0.6, "sfx-transition-reveal": 0.6 }; // page flips: 60% (play test)
    const MUSIC_FADE_MS = 400;
    const KEY_EFFECTS   = "audio.effects";
    const KEY_MUSIC     = "audio.music";
    const KEY_VOICE     = "audio.voice";
    const KEY_VOLUME    = "audio.volume."; // + music | effects | voice

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

    function readVolume(kind) {
        try {
            const value = parseInt(localStorage.getItem(KEY_VOLUME + kind), 10);
            return isNaN(value) ? 100 : Math.min(100, Math.max(0, value));
        } catch (e) {
            return 100;
        }
    }

    const settings = { effects: readSetting(KEY_EFFECTS), music: readSetting(KEY_MUSIC) };
    const volumes  = { music: readVolume("music"), effects: readVolume("effects"), voice: readVolume("voice") };
    let unlocked = false;

    // ---- Effects --------------------------------------------------------------------------------------------------

    const clips = {};
    for (const name of EFFECTS) {
        const element   = new Audio();
        element.preload = "auto";
        element.src     = src(name);
        clips[name]     = element;
    }

    function applyEffectsVolume() {
        for (const name of EFFECTS) clips[name].volume = (VOLUMES[name] ?? 1) * volumes.effects / 100;
    }

    applyEffectsVolume();

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
            const clone  = clips[name].cloneNode();
            clone.volume = clips[name].volume; // volume is not an attribute: cloneNode does not copy it
            tryPlay(clone);
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
    let musicGain   = 1;         // 1, or MUSIC_DUCK while the voice-over plays (times the music volume: musicLevel)
    let duckTimer   = 0;

    music.addEventListener("ended", () => { musicEnded = true; });

    // Full volume of the music element: the music volume setting, ducked while the voice-over plays
    const musicLevel = () => musicGain * volumes.music / 100;

    function cancelFade() {
        clearInterval(fadeTimer);
        fadeTimer   = 0;
        switchingTo = undefined;
    }

    function fadeOut(done) {
        clearInterval(duckTimer);
        duckTimer = 0;

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
            clearInterval(duckTimer);
            duckTimer = 0;
            music.pause();
            music.volume = musicLevel();
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
            music.volume = musicLevel();
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

    // ---- Music ducking while the voice-over plays ------------------------------------------------------------------

    const MUSIC_DUCK = 0.3;

    function duckMusic(on) {
        const gain = on ? MUSIC_DUCK : 1;
        if (musicGain === gain) return;
        musicGain = gain;
        rampMusic();
    }

    // Moves the music element to musicLevel() in a short ramp (ducking, a volume change)
    function rampMusic() {
        const target = musicLevel();
        clearInterval(duckTimer);
        duckTimer = 0;
        if (fadeTimer) return; // a track switch is fading out; it starts the next track at musicLevel()

        // Short ramp instead of a jump
        duckTimer = setInterval(() => {
            const volume = music.volume;
            const next   = volume < target ? Math.min(target, volume + 0.07) : Math.max(target, volume - 0.07);
            music.volume = next;
            if (next !== target) return;
            clearInterval(duckTimer);
            duckTimer = 0;
        }, 30);
    }

    // ---- Voice-over -----------------------------------------------------------------------------------------------
    // Unity (SoundManager.OnVOPlayOneTime): 4.5 s of the thunder clip, then the narration clip on the same source.
    // Pause menu: pause; resume during the lead-in skips the rest of it (SoundManager.OnVOResume).

    const VOICES        = ["female", "male", "oliver", "off"];
    const VOICE_DEFAULT = "male"; // Unity: PlayerPrefs "AudioTrack" defaults to 0 = male (ViewAudioTrack, ViewMainMenu)
    const LEAD_IN_MS    = 4500;
    const THUNDER       = src("sfx-vo-thunder");

    function readVoice() {
        try {
            const value = localStorage.getItem(KEY_VOICE);
            return VOICES.indexOf(value) >= 0 ? value : VOICE_DEFAULT;
        } catch (e) {
            return VOICE_DEFAULT;
        }
    }

    const voiceElement   = new Audio();
    voiceElement.preload = "none";
    voiceElement.volume  = volumes.voice / 100;

    const voice = {
        setting:  readVoice(),
        id:       0,        // current request (0 = none); reported with every state change
        counter:  0,
        phase:    null,     // null | "leadin" | "clip"
        clip:     null,     // url of the narration clip of the current request
        queue:    [],       // urls still to play after clip (Oliver narration: one clip per text chunk)
        timer:    0,        // lead-in timer
        paused:   false,    // paused by the pause menu
        listener: null      // DotNetObjectReference (Pages/Gameplay.razor), method OnVoiceState(id, state)
    };

    function notifyVoice(id, state) {
        if (!voice.listener || !id) return;
        try {
            const promise = voice.listener.invokeMethodAsync("OnVoiceState", id, state);
            if (promise && promise.catch) promise.catch(() => { });
        } catch (e) {
            // Listener already disposed
        }
    }

    function startVoiceSource(url) {
        const id    = voice.id;
        const phase = voice.phase;
        voiceElement.src = url;
        try {
            const promise = voiceElement.play();
            if (promise && promise.catch) {
                promise.catch(() => {
                    // Blocked or failed (an interrupted play() rejects too: ignore when the request moved on)
                    if (voice.id === id && voice.phase === phase && !voice.paused) endVoice();
                });
            }
        } catch (e) {
            endVoice();
        }
    }

    function startClip() {
        clearTimeout(voice.timer);
        voice.timer = 0;
        voice.phase = "clip";
        startVoiceSource(voice.clip);
    }

    // Stops the current request (if any) and reports "stopped" for it
    function endVoice() {
        const id = voice.id;
        clearTimeout(voice.timer);
        voice.timer  = 0;
        voice.id     = 0;
        voice.phase  = null;
        voice.clip   = null;
        voice.queue  = [];
        voice.paused = false;
        try {
            voiceElement.pause();
            voiceElement.removeAttribute("src");
            voiceElement.load();
        } catch (e) {
            // Ignore
        }
        duckMusic(false);
        notifyVoice(id, "stopped");
    }

    // name: clip file name (the Unity passage name); which: "female" | "male";
    // leadIn: thunder first.
    // Returns the request id, or 0 when nothing starts (audio not unlocked by a user gesture yet).
    function voicePlay(name, which, leadIn) {
        endVoice();
        if (!unlocked || !name) return 0;

        voice.id   = ++voice.counter;
        voice.clip = "audio/vo/" + (which === "male" ? "male" : "female") + "/" + name + ".mp3";
        duckMusic(true);

        if (leadIn) {
            voice.phase = "leadin";
            startVoiceSource(THUNDER);
            voice.timer = setTimeout(startClip, LEAD_IN_MS);
        } else {
            startClip();
        }

        return voice.id;
    }

    // urls: the clips of one Oliver narration, played in order without a lead-in. Returns the request id, or 0.
    function voicePlayList(urls) {
        endVoice();
        if (!unlocked || !urls || !urls.length) return 0;

        voice.id    = ++voice.counter;
        voice.clip  = urls[0];
        voice.queue = urls.slice(1);
        duckMusic(true);
        startClip();
        return voice.id;
    }

    let preload = null; // warms the browser cache for the next clip of a list, so the gap between clips stays short

    voiceElement.addEventListener("playing", () => {
        if (voice.phase !== "clip") return;
        if (voice.queue.length) {
            preload         = new Audio();
            preload.preload = "auto";
            preload.src     = voice.queue[0];
        }
        notifyVoice(voice.id, "speaking");
    });
    voiceElement.addEventListener("ended", () => {
        if (voice.phase === "clip" && voice.queue.length) {
            voice.clip = voice.queue.shift();
            startVoiceSource(voice.clip);
        }
        else if (voice.phase === "clip") endVoice();
        else if (voice.phase === "leadin") startClip(); // lead-in clip shorter than LEAD_IN_MS
    });
    voiceElement.addEventListener("error", () => {
        // Missing or unsupported file (not the src removal in endVoice)
        if (voice.phase && voiceElement.getAttribute("src")) endVoice();
    });

    function voicePause() {
        if (!voice.phase || voice.paused) return;
        voice.paused = true;
        clearTimeout(voice.timer);
        voice.timer = 0;
        try {
            voiceElement.pause();
        } catch (e) {
            // Ignore
        }
    }

    function voiceResume() {
        if (!voice.paused) return;
        voice.paused = false;
        if (voice.phase === "leadin") {
            startClip();
            return;
        }
        if (voice.phase === "clip") {
            const id = voice.id;
            try {
                const promise = voiceElement.play();
                if (promise && promise.catch) promise.catch(() => { if (voice.id === id && !voice.paused) endVoice(); });
            } catch (e) {
                endVoice();
            }
        }
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
        // kind: "music" | "effects" | "voice"; value 0-100
        getVolume: kind => volumes[kind] ?? 100,
        setVolume: (kind, value) => {
            if (!(kind in volumes)) return;
            volumes[kind] = Math.min(100, Math.max(0, Math.round(Number(value) || 0)));
            try {
                localStorage.setItem(KEY_VOLUME + kind, String(volumes[kind]));
            } catch (e) {
                // Storage unavailable: keep the volume for this session only
            }
            if (kind === "effects") applyEffectsVolume();
            else if (kind === "voice") voiceElement.volume = volumes.voice / 100;
            else rampMusic();
        },
        isUnlocked: () => unlocked,
        // Voice-over
        getVoice: () => voice.setting,
        setVoice: which => {
            voice.setting = VOICES.indexOf(which) >= 0 ? which : VOICE_DEFAULT;
            try {
                localStorage.setItem(KEY_VOICE, voice.setting);
            } catch (e) {
                // Storage unavailable: keep the setting for this session only
            }
            endVoice(); // a change stops the current narration, it does not start a new one
        },
        voicePlay: voicePlay,
        voicePlayList: voicePlayList,
        voiceStop: endVoice,
        voicePause: voicePause,
        voiceResume: voiceResume,
        voiceSubscribe: listener => { voice.listener = listener || null; },
        voiceUnsubscribe: () => { voice.listener = null; },
        // Read-only snapshot for debugging
        state: () => ({
            unlocked: unlocked,
            effectsOn: settings.effects,
            musicOn: settings.music,
            wantedMusic: wanted ? wanted.name : null,
            currentMusic: currentName,
            musicPaused: music.paused,
            musicLoop: music.loop,
            musicVolume: music.volume,
            musicGain: musicGain,
            volumes: Object.assign({}, volumes),
            voice: voice.setting,
            voiceId: voice.id,
            voicePhase: voice.phase,
            voicePaused: voice.paused,
            voiceClip: voice.clip,
            voiceSrc: voiceElement.getAttribute("src"),
            voiceElementPaused: voiceElement.paused,
            voiceTime: voiceElement.currentTime
        })
    };
})();
