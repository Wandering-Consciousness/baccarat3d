using UnityEngine;
using System.Collections;

public class MenuSounds : MonoBehaviour {
    public AudioSource menuOpenAudioSrc;
    public AudioSource menuCloseAudioSrc;

	public void menuOpen() {
        LogUtils.LogEvent(Consts.FE_BTN_MENU_OPEN);
        menuOpenAudioSrc.Play();
    }

    public void menuClose() {
        LogUtils.LogEvent(Consts.FE_BTN_MENU_CLOSE);
        menuCloseAudioSrc.Play();
    }
}
