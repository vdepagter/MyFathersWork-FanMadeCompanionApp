namespace MyFathersWorkApp;

// Sound effects (docs/AUDIO_TRANSITIONS_MAP.md 2.1). Played only by the UI (AudioService), never by scenario code.
public enum SoundCue
{
    None             = 0,
    Click            = 1,
    TransitionCover  = 2,
    TransitionReveal = 3,
    Setup            = 4,
    EndOfRound       = 5,
    SpecialEvent     = 6,
    Welcome          = 7,
    Footsteps        = 8
}
