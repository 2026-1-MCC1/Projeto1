using UnityEngine;
using UnityEngine.SceneManagement;

public class AudiosScript : MonoBehaviour
{
    public static AudiosScript instancia;

    [Header("Fontes de Audio")]
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
            return;
        }

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

    private void QuandoTrocarCena(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Menu" && musicaSource != null && musicaSource.isPlaying)
        {
            musicaSource.Stop();
        }
    }

    private void AplicarVolume()
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
        if (clip == null || musicaSource == null) return;
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;

        musicaSource.clip = clip;
        musicaSource.loop = true;
        musicaSource.volume = volumeMusica;
        musicaSource.Play();
    }

    public void TocarEfeito(AudioClip clip)
    {
        if (clip == null || efeitosSource == null) return;
        efeitosSource.PlayOneShot(clip, volumeEfeitos);
    }
}
