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

    public void PlayPlayerHit()  => Play(playerHit);
    public void PlayPlayerDeath() => Play(playerDeath);
    public void PlayEnemyHit()   => Play(enemyHit);
    public void PlayEnemyDeath() => Play(enemyDeath);
    public void PlayEnemyShoot() => Play(enemyShoot);
    public void PlayParry()      => Play(parry);
    public void PlayBikeHit()     => Play(bikeHit);
    public void PlayBikeParried() => Play(bikeParried);
    public void PlayBikeRev()     => Play(bikeRev);
    public void PlaySniperBeep()  => Play(sniperBeep);
    public void PlaySniperShot()  => Play(sniperShot);

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
