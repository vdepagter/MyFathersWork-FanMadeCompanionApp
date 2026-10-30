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
    Footsteps        = 8,
    BidOpen          = 9,  // Unity ViewBiddingSystem clip: Set up-window appear
    Reveal           = 10  // Unity ViewBiddingSystem revealClip: Reveal, under the 3-2-1 countdown
}
