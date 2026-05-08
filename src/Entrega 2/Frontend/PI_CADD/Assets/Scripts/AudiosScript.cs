using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudiosScript : MonoBehaviour
{
    // Singleton: uma instância global para todas as cenas.
    public static AudiosScript instancia;
    private const string ChaveVolumeMusica = "audio_volume_musica";
    private const string ChaveVolumeEfeitos = "audio_volume_efeitos";

    [Header("Fontes de Audio")]
    public AudioSource musicaSource;
    public AudioSource efeitosSource;

    [Header("Volume Inicial (20%)")]
    [Range(0f, 1f)] public float volumeMusica = 0.2f;
    [Range(0f, 1f)] public float volumeEfeitos = 0.2f;
    private Slider sliderMusicaMenu;
    private Slider sliderEfeitosMenu;

    private void Awake()
    {
        // Garante só um gerenciador de áudio vivo no jogo inteiro.
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

        CarregarPreferenciasVolume();
        AplicarVolume();
        ConfigurarSlidersMenuNaCenaAtual();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += QuandoTrocarCena;
    }

    private System.Collections.IEnumerator Start()
    {
        // Aguarda a cena estabilizar para achar sliders criados no mesmo frame.
        yield return null;
        ConfigurarSlidersMenuNaCenaAtual();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= QuandoTrocarCena;
    }

    private void QuandoTrocarCena(Scene scene, LoadSceneMode mode)
    {
        // Exemplo de regra: música toca no menu/planejamento e para no resto.
        if (scene.name != "Menu" && scene.name != "CenaPlanejamento" && musicaSource != null && musicaSource.isPlaying)
        {
            musicaSource.Stop();
        }
        else if ((scene.name == "Menu" || scene.name == "CenaPlanejamento") && musicaSource != null && !musicaSource.isPlaying && musicaSource.clip != null)
        {
            musicaSource.Play();
        }

        if (scene.name == "Menu")
            ConfigurarSlidersMenuNaCenaAtual();

        // Reaplica volume ao entrar na cena para manter consistência.
        AplicarVolume();
    }

    private void AplicarVolume()
    {
        if (musicaSource != null)
        {
            string nomeCena = SceneManager.GetActiveScene().name;
            // No planejamento, a música fica mais baixa para não atrapalhar foco.
            float multiplicadorMusica = (nomeCena == "CenaPlanejamento") ? 0.3f : 1f;
            musicaSource.volume = volumeMusica * multiplicadorMusica;
        }

        if (efeitosSource != null)
            efeitosSource.volume = volumeEfeitos;
    }

    public void MudarVolumeMusica(float valor)
    {
        // Clamp limita o valor entre 0 e 1.
        volumeMusica = Mathf.Clamp01(valor);
        SalvarPreferenciasVolume();
        AplicarVolume();
    }

    public void MudarVolumeEfeitos(float valor)
    {
        volumeEfeitos = Mathf.Clamp01(valor);
        SalvarPreferenciasVolume();
        AplicarVolume();
    }

    public void TocarMusica(AudioClip clip)
    {
        if (clip == null || musicaSource == null) return;
        // Evita reiniciar a mesma música sem necessidade.
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;

        musicaSource.clip = clip;
        musicaSource.loop = true;
        musicaSource.volume = volumeMusica;
        musicaSource.Play();
    }

    public void TocarEfeito(AudioClip clip)
    {
        TocarEfeito(clip, 1f);
    }

    public void TocarEfeito(AudioClip clip, float multiplicadorVolume)
    {
        if (clip == null || efeitosSource == null) return;
        float multiplicador = Mathf.Clamp01(multiplicadorVolume);
        efeitosSource.PlayOneShot(clip, volumeEfeitos * multiplicador);
    }

    private void CarregarPreferenciasVolume()
    {
        // Busca valores salvos no PlayerPrefs; se não existir usa padrão do inspector.
        volumeMusica = Mathf.Clamp01(PlayerPrefs.GetFloat(ChaveVolumeMusica, volumeMusica));
        volumeEfeitos = Mathf.Clamp01(PlayerPrefs.GetFloat(ChaveVolumeEfeitos, volumeEfeitos));
    }

    private void SalvarPreferenciasVolume()
    {
        PlayerPrefs.SetFloat(ChaveVolumeMusica, volumeMusica);
        PlayerPrefs.SetFloat(ChaveVolumeEfeitos, volumeEfeitos);
        PlayerPrefs.Save();
    }

    private void ConfigurarSlidersMenuNaCenaAtual()
    {
        Scene cenaAtual = SceneManager.GetActiveScene();
        if (cenaAtual.name != "Menu") return;

        // Liga slider "Musica" à função de mudar volume da música.
        GameObject objMusica = GameObject.Find("Musica");
        sliderMusicaMenu = objMusica != null ? objMusica.GetComponent<Slider>() : null;
        if (sliderMusicaMenu != null)
        {
            sliderMusicaMenu.onValueChanged.RemoveListener(MudarVolumeMusica);
            sliderMusicaMenu.SetValueWithoutNotify(volumeMusica);
            sliderMusicaMenu.onValueChanged.AddListener(MudarVolumeMusica);
        }

        // Liga slider "Volume" à função de mudar volume dos efeitos.
        GameObject objEfeitos = GameObject.Find("Volume");
        sliderEfeitosMenu = objEfeitos != null ? objEfeitos.GetComponent<Slider>() : null;
        if (sliderEfeitosMenu != null)
        {
            sliderEfeitosMenu.onValueChanged.RemoveListener(MudarVolumeEfeitos);
            sliderEfeitosMenu.SetValueWithoutNotify(volumeEfeitos);
            sliderEfeitosMenu.onValueChanged.AddListener(MudarVolumeEfeitos);
        }
    }
}
