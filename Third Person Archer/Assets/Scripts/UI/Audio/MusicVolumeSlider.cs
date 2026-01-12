using MoreMountains.Tools;

public class MusicVolumeSlider : VolumeSlider
{
    protected override MMSoundManager.MMSoundManagerTracks Track =>
        MMSoundManager.MMSoundManagerTracks.Music;

    protected override string PlayerPrefsKey => "music_volume";
}