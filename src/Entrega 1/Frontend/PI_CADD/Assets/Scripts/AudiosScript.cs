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
        Debug.Log("Volume musica: " + valor);
        volumeMusica = valor;
        musicaSource.volume = valor;
    }

    public void MudarVolumeEfeitos(float valor)
    {
        Debug.Log("Volume efeitos: " + valor);
        volumeEfeitos = valor;
        efeitosSource.volume = valor;
    }

    public void TocarMusica(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Musica não atribuída!");
            return;
        }

        if (musicaSource.clip == clip) return;

        musicaSource.clip = clip;
        musicaSource.loop = true;
        musicaSource.volume = volumeMusica;
        musicaSource.Play();
    }

    public void TocarEfeito(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("Efeito não atribuído!");
            return;
        }

        efeitosSource.PlayOneShot(clip, volumeEfeitos);
    }
}

