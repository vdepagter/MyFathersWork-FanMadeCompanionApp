namespace MyFathersWorkWebApp;

// Volume slider channels (Shared/VolumeSliders.razor), stored by wwwroot/js/audio.js in localStorage, 0-100 each
public enum AudioVolume
{
    Music   = 0,
    Effects = 1,
    Speech  = 2
}
