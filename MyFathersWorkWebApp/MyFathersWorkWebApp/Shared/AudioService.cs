using Microsoft.JSInterop;

namespace MyFathersWorkWebApp;

// Thin wrapper around wwwroot/js/audio.js (window.mfwAudio). Only UI code calls it (click handlers, Pages/Gameplay.razor
// after a render, Layout/MainLayout.razor on navigation) - never scenario code, which also runs in mirror simulations,
// undo and load. Every call is synchronous (Blazor WebAssembly runs JS in-process) and never throws.
public sealed class AudioService(IJSRuntime js)
{
    private MusicTrack? _RequestedMusic;
    private bool        _WelcomePlayed;

    public bool EffectsOn => Invoke("mfwAudio.isEffectsOn", true);
    public bool MusicOn   => Invoke("mfwAudio.isMusicOn",   true);

    public void Play(SoundCue cue, int delayMs = 0)
    {
        (string? clip, string channel) = cue switch
        {
            SoundCue.Click            => ("sfx-click",             "click"),
            SoundCue.TransitionCover  => ("sfx-transition-cover",  "effects"),
            SoundCue.TransitionReveal => ("sfx-transition-reveal", "effects"),
            SoundCue.Welcome          => ("sfx-welcome",           "effects"),
            SoundCue.Footsteps        => ("sfx-footsteps",         "effects"),
            SoundCue.Setup            => ("sfx-setup",             "popup"),
            SoundCue.EndOfRound       => ("sfx-end-of-round",      "popup"),
            SoundCue.SpecialEvent     => ("sfx-special-event",     "popup"),
            _                         => ((string?)null, string.Empty)
        };

        if (clip != null) InvokeVoid("mfwAudio.play", clip, channel, delayMs);
    }

    public void PlayClick() => Play(SoundCue.Click);

    // App start sound, once per app session (dropped by the browser when no user gesture happened yet)
    public void PlayWelcomeOnce()
    {
        if (_WelcomePlayed) return;
        _WelcomePlayed = true;
        Play(SoundCue.Welcome);
    }

    // Declarative: call with the track the current screen wants; the same track again changes nothing
    public void SetMusic(MusicTrack track)
    {
        if (_RequestedMusic == track) return;
        _RequestedMusic = track;

        string? name = track switch
        {
            MusicTrack.Title            => "music-title",
            MusicTrack.CostOfDisease    => "music-cost-of-disease",
            MusicTrack.FearOfTheUnknown => "music-fear-of-the-unknown",
            MusicTrack.Ending           => "music-ending",
            _                           => null
        };

        InvokeVoid("mfwAudio.setMusic", name, track != MusicTrack.Ending);
    }

    public static MusicTrack ScenarioTheme(ScenarioId scenarioId) => scenarioId switch
    {
        ScenarioId.CostOfDisease => MusicTrack.CostOfDisease,
        ScenarioId.FearOfUnknown => MusicTrack.FearOfTheUnknown,
        _                        => MusicTrack.None
    };

    public void SetEffectsOn(bool on) => InvokeVoid("mfwAudio.setEffectsOn", on);
    public void SetMusicOn(bool on)   => InvokeVoid("mfwAudio.setMusicOn",   on);

    // True after the first user gesture; before it the browser blocks playback
    public bool IsUnlocked => Invoke("mfwAudio.isUnlocked", false);

    #region Voice-over (docs/AUDIO_TRANSITIONS_MAP.md "Voice-over")

    private NarrationVoice? _Narration;

    // Narration setting (localStorage via audio.js); read once, then cached
    public NarrationVoice Narration => _Narration ??= Invoke("mfwAudio.getVoice", "male") switch
    {
        "female" => NarrationVoice.Female,
        "off"    => NarrationVoice.Off,
        _        => NarrationVoice.Male
    };

    // Stores the setting and stops the current narration (a change never starts one)
    public void SetNarration(NarrationVoice narration)
    {
        _Narration = narration;
        InvokeVoid("mfwAudio.setVoice", VoiceName(narration));
    }

    // Plays the clip of a narrated window (GameplayWindow.VoiceOver) in the given voice (Off plays the female voice),
    // optionally after the thunder lead-in. Returns the request id reported to the listener, or 0 when nothing started.
    public int VoicePlay(string clip, NarrationVoice voice, bool leadIn) =>
        Invoke("mfwAudio.voicePlay", 0, clip, voice == NarrationVoice.Male ? "male" : "female", leadIn);

    public void VoiceStop()   => InvokeVoid("mfwAudio.voiceStop");
    public void VoicePause()  => InvokeVoid("mfwAudio.voicePause");
    public void VoiceResume() => InvokeVoid("mfwAudio.voiceResume");

    // One listener with a [JSInvokable] OnVoiceState(int id, string state) - "speaking" or "stopped"
    public void VoiceSubscribe<T>(DotNetObjectReference<T> listener) where T : class => InvokeVoid("mfwAudio.voiceSubscribe", listener);
    public void VoiceUnsubscribe()                                                     => InvokeVoid("mfwAudio.voiceUnsubscribe");

    private static string VoiceName(NarrationVoice narration) => narration switch
    {
        NarrationVoice.Female => "female",
        NarrationVoice.Off    => "off",
        _                     => "male"
    };

    #endregion

    private void InvokeVoid(string identifier, params object?[] args)
    {
        try
        {
            if (js is IJSInProcessRuntime inProcess) inProcess.InvokeVoid(identifier, args);
            else _ = js.InvokeVoidAsync(identifier, args).AsTask().ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Audio: {identifier} failed: {exception.Message}");
        }
    }

    private T Invoke<T>(string identifier, T fallback, params object?[] args)
    {
        try
        {
            return js is IJSInProcessRuntime inProcess ? inProcess.Invoke<T>(identifier, args) : fallback;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Audio: {identifier} failed: {exception.Message}");
            return fallback;
        }
    }
}
