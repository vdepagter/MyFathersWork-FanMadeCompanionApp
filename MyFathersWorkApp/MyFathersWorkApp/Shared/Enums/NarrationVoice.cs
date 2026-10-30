namespace MyFathersWorkApp;

// Voice-over setting (docs/AUDIO_TRANSITIONS_MAP.md "Voice-over"), stored by wwwroot/js/audio.js in localStorage.
// Default Male, as in the Unity app (PlayerPrefs "AudioTrack" 0). Female and Male play the original recordings only;
// Oliver is the fan-made AI narration of all story text (Shared/Narration.cs), falling back to the original male
// recording where a window has no Oliver clips yet.
public enum NarrationVoice
{
    Female = 0,
    Male   = 1,
    Off    = 2,
    Oliver = 3
}
