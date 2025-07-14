using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private AudioSource _bgmAudioSrc;
    [SerializeField]
    private AudioSource _sfxAudioSrc;

    [SerializeField]
    private AudioClip _lobbyAndRoomBgmClip;

    public bool BgmIsPlaying
    {
        get { return _bgmAudioSrc.isPlaying; }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayLobbyAndRoomBGM() { _bgmAudioSrc.PlayOneShot(_lobbyAndRoomBgmClip); }

    public void StopLobbyAndRoomBGM() { _bgmAudioSrc.Stop(); }

    public void PlaySfx(AudioClip clip)
    {
        _sfxAudioSrc.PlayOneShot(clip);
    }
}
