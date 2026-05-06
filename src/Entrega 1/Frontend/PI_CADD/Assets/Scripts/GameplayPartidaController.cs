using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayPartidaController : MonoBehaviour
{
    public static GameplayPartidaController Instancia { get; private set; }

    [Header("Pontuacao")]
    [SerializeField] private int pontosIniciais = 100;
    [SerializeField] private int pontosMinimos = 0;
    [Header("Referencias UI (configurar na cena)")]
    [SerializeField] private TextMeshProUGUI textoPontos;
    [SerializeField] private TextMeshProUGUI textoContagem;
    [SerializeField] private Image fundoContagem;
    [SerializeField] private GameObject painelFim;
    [SerializeField] private TextMeshProUGUI tituloFim;
    [SerializeField] private TextMeshProUGUI pontosFim;
    [SerializeField] private GameObject botaoReiniciarFim;
    [SerializeField] private GameObject botaoMenuFim;
    [Header("Cenas")]
    [SerializeField] private string cenaMenu = "Menu";
    [SerializeField] private string cenaPlanejamento = "CenaPlanejamento";
    [Header("Inicio da Partida")]
    [SerializeField] private bool usarContagemInicial = true;
    [SerializeField] private int segundosContagem = 3;

    private int pontosAtuais;
    private bool partidaFinalizada = false;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        Time.timeScale = 1f;

        EncontrarReferenciasUISeNecessario();

        pontosAtuais = Mathf.Max(pontosMinimos, pontosIniciais);
        AtualizarTextoPontos();

        if (painelFim != null)
            painelFim.SetActive(false);

        if (textoContagem != null)
            textoContagem.gameObject.SetActive(false);

        if (fundoContagem != null)
            fundoContagem.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!usarContagemInicial) return;
        StartCoroutine(RodarContagemInicial());
    }

    public void DescontarPontos(int valor)
    {
        if (partidaFinalizada) return;

        pontosAtuais = Mathf.Max(pontosMinimos, pontosAtuais - Mathf.Abs(valor));
        AtualizarTextoPontos();
    }

    public void RegistrarCaptura()
    {
        if (partidaFinalizada) return;
        partidaFinalizada = true;
        MostrarTelaFinal("Capturado", $"Voce fez {pontosAtuais} pontos", false);
    }

    public void RegistrarFuga()
    {
        if (partidaFinalizada) return;
        partidaFinalizada = true;
        MostrarTelaFinal("Ele escapou", $"Voce fez {pontosAtuais} pontos", true);
    }

    private void AtualizarTextoPontos()
    {
        if (textoPontos != null)
            textoPontos.text = $"Pontos: {pontosAtuais}";
    }

    private void MostrarTelaFinal(string titulo, string textoPontosFinal, bool permitirReiniciar)
    {
        if (tituloFim != null) tituloFim.text = titulo;
        if (pontosFim != null) pontosFim.text = textoPontosFinal;
        if (botaoReiniciarFim != null) botaoReiniciarFim.SetActive(permitirReiniciar);
        if (botaoMenuFim != null) botaoMenuFim.SetActive(true);
        if (painelFim != null) painelFim.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(cenaPlanejamento))
        {
            Debug.LogError($"GameplayPartidaController: cena de planejamento '{cenaPlanejamento}' nao esta no Build Settings.");
            Scene cenaAtual = SceneManager.GetActiveScene();
            SceneManager.LoadScene(cenaAtual.name);
            return;
        }
        SceneManager.LoadScene(cenaPlanejamento);
    }

    public void VoltarMenu()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded(cenaMenu))
        {
            Debug.LogError($"GameplayPartidaController: cena '{cenaMenu}' nao esta no Build Settings.");
            return;
        }
        SceneManager.LoadScene(cenaMenu);
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;

        Time.timeScale = 1f;
    }

    private void EncontrarReferenciasUISeNecessario()
    {
        if (textoPontos == null)
        {
            GameObject obj = GameObject.Find("TextoPontos");
            if (obj != null) textoPontos = obj.GetComponent<TextMeshProUGUI>();
        }

        if (painelFim == null)
            painelFim = GameObject.Find("PainelFim");

        if (tituloFim == null)
        {
            GameObject obj = GameObject.Find("TituloFim");
            if (obj != null) tituloFim = obj.GetComponent<TextMeshProUGUI>();
        }

        if (pontosFim == null)
        {
            GameObject obj = GameObject.Find("PontosFim");
            if (obj != null) pontosFim = obj.GetComponent<TextMeshProUGUI>();
        }

        if (botaoReiniciarFim == null)
            botaoReiniciarFim = GameObject.Find("BotaoReiniciarFim");

        if (botaoMenuFim == null)
            botaoMenuFim = GameObject.Find("BotaoMenuFim");

        if (textoContagem == null)
        {
            GameObject obj = GameObject.Find("TextoContagem");
            if (obj != null) textoContagem = obj.GetComponent<TextMeshProUGUI>();
        }

        if (fundoContagem == null)
        {
            GameObject obj = GameObject.Find("FundoContagem");
            if (obj != null) fundoContagem = obj.GetComponent<Image>();
        }
    }

    private System.Collections.IEnumerator RodarContagemInicial()
    {
        if (textoContagem == null) yield break;

        Time.timeScale = 0f;
        if (fundoContagem != null) fundoContagem.gameObject.SetActive(true);
        textoContagem.gameObject.SetActive(true);

        int total = Mathf.Max(1, segundosContagem);
        for (int i = total; i >= 1; i--)
        {
            textoContagem.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        textoContagem.text = "JA!";
        yield return new WaitForSecondsRealtime(0.6f);

        textoContagem.gameObject.SetActive(false);
        if (fundoContagem != null) fundoContagem.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
