using UnityEngine;
using UnityEngine.SceneManagement;

public class AudiosScript : MonoBehaviour
{
    public static AudiosScript instancia;

    [Header("Fontes de áudio")]
    public AudioSource musicaSource;
    public AudioSource efeitosSource;

    [Header("Volume Inicial (20%)")]
    [Range(0f, 1f)] public float volumeMusica = 0.2f;
    [Range(0f, 1f)] public float volumeEfeitos = 0.2f;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 🔥 FORÇA O VOLUME LOGO NO INÍCIO
        AplicarVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += QuandoTrocarCena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= QuandoTrocarCena;
    }

    void QuandoTrocarCena(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Menu") // coloque o nome correto da sua cena
        {
            musicaSource.Stop();
        }
    }

    void AplicarVolume()
    {
        if (musicaSource != null)
            musicaSource.volume = volumeMusica;

        if (efeitosSource != null)
            efeitosSource.volume = volumeEfeitos;
    }

    public void MudarVolumeMusica(float valor)
    {
        volumeMusica = valor;
        AplicarVolume();
    }

    public void MudarVolumeEfeitos(float valor)
    {
        volumeEfeitos = valor;
        AplicarVolume();
    }

    public void TocarMusica(AudioClip clip)
    {
        if (clip == null) return;

        musicaSource.clip = clip;
        musicaSource.loop = true;

        // 🔥 GARANTE volume correto antes de tocar
        musicaSource.volume = volumeMusica;

        musicaSource.Play();
    }

    public void TocarEfeito(AudioClip clip)
    {
        if (clip == null) return;

        efeitosSource.PlayOneShot(clip, volumeEfeitos);
    }
}