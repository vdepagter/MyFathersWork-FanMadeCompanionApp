namespace MyFathersWorkWebApp;

// Voice-over setting (docs/AUDIO_TRANSITIONS_MAP.md "Voice-over"), stored by wwwroot/js/audio.js in localStorage.
// Default Male, as in the Unity app (PlayerPrefs "AudioTrack" 0).
public enum NarrationVoice
{
    Female = 0,
    Male   = 1,
    Off    = 2
}
