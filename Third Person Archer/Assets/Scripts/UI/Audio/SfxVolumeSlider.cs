using MoreMountains.Tools;

public class SfxVolumeSlider : VolumeSlider
{
    protected override MMSoundManager.MMSoundManagerTracks Track =>
        MMSoundManager.MMSoundManagerTracks.Sfx;

    protected override string PlayerPrefsKey => "sfx_volume";
}
