using System.Collections;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playerHit;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip enemyHit;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip enemyShoot;
    [SerializeField] private AudioClip parry;
    [SerializeField] private AudioClip bikeEngineLoop;
    [SerializeField] private AudioClip bikeHit;
    [SerializeField] private AudioClip bikeParried;
    [SerializeField] private AudioClip bikeRev;
    [SerializeField] private AudioClip sniperBeep;
    [SerializeField] private AudioClip sniperShot;
    [SerializeField] private AudioClip swordSlash;
    [SerializeField] private AudioClip grasslandsMusic;
    [SerializeField] private AudioClip technoBluesMusic;
    [Range(0f, 1f)] public float musicVolume = 0.3f;
    [SerializeField] private AudioClip entrance;
    [SerializeField] private AudioClip entranceButton;
    [SerializeField] private AudioClip block;

    private AudioSource _musicSource;

    public void PlayPlayerHit()  => Play(playerHit);
    public void PlayPlayerDeath() => Play(playerDeath);
    public void PlayEnemyHit()   => Play(enemyHit);
    public void PlayEnemyDeath() => Play(enemyDeath);
    public void PlayEnemyShoot() => Play(enemyShoot);
    public void PlayParry()      => Play(parry);
    public void PlayBlock()      => Play(block);
    public void PlayBikeHit()     => Play(bikeHit);
    public void PlayBikeParried() => Play(bikeParried);
    public void PlayBikeRev()     => Play(bikeRev);
    public void PlaySniperBeep()  => Play(sniperBeep);
    public void PlaySniperShot()  => Play(sniperShot);
    public void PlaySwordSlash()  => Play(swordSlash);
    public void PlayEntrance() => Play(entrance);
    public void PlayEntranceButton() => Play(entranceButton);

    public void StartBikeEngine(AudioSource source)
    {
        if (source == null || bikeEngineLoop == null) return;
        source.clip = bikeEngineLoop;
        source.loop = true;
        source.Play();
    }

    public void StopBikeEngine(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        source.loop = false;
        source.clip = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        if (grasslandsMusic == null && technoBluesMusic == null) yield break;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.spatialBlend = 0f;
        _musicSource.volume = musicVolume;

        AudioClip[] playlist = { grasslandsMusic, technoBluesMusic };
        int trackIndex = 0;
        while (true)
        {
            AudioClip track = playlist[trackIndex];
            trackIndex = (trackIndex + 1) % playlist.Length;
            if (track == null) continue;

            _musicSource.clip = track;
            _musicSource.Play();
            yield return new WaitForSecondsRealtime(track.length);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
