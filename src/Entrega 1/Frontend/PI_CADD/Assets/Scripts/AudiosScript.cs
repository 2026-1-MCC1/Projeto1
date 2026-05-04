using UnityEngine;

public class AudiosScript : MonoBehaviour

{
    public static AudiosScript instancia;

    [Header("Fontes de áudio")]
    public AudioSource musicaSource;
    public AudioSource efeitosSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float volumeMusica = 1f;
    [Range(0f, 1f)] public float volumeEfeitos = 1f;

    private void Awake()
    {
        // Singleton (só pode existir 1)
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AtualizarVolumes();
    }

    public void AtualizarVolumes()
    {
        musicaSource.volume = volumeMusica;
        efeitosSource.volume = volumeEfeitos;
    }

    public void MudarVolumeMusica(float valor)
    {
        volumeMusica = valor;
        musicaSource.volume = valor;
    }

    public void MudarVolumeEfeitos(float valor)
    {
        volumeEfeitos = valor;
        efeitosSource.volume = valor;
    }

    public void TocarMusica(AudioClip clip)
    {
        if (musicaSource.clip == clip) return;

        musicaSource.clip = clip;
        musicaSource.loop = true;
        musicaSource.Play();
    }

    public void TocarEfeito(AudioClip clip)
    {
        efeitosSource.PlayOneShot(clip, volumeEfeitos);
    }
}
