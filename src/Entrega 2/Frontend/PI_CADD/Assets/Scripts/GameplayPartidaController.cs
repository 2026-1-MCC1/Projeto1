using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameplayPartidaController : MonoBehaviour
{
    // Singleton da partida para outros scripts atualizarem pontuação e resultado final.
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
    [SerializeField] private bool mostrarMensagensAntesDaContagem = true;
    [SerializeField] private string mensagemInicial1 = "EVITE BATER";
    [SerializeField] private string mensagemInicial2 = "PROTEJA A CIDADE";
    [SerializeField] private float duracaoMensagemInicial = 1.2f;
    [SerializeField] private float tamanhoFonteMensagensIniciais = 96f;
    [SerializeField] private float tamanhoFonteContagemNumerica = 190f;
    [Header("Pausa (ESC)")]
    [SerializeField] private bool habilitarMenuPausa = true;

    private int pontosAtuais;
    private bool partidaFinalizada = false;
    private bool pausaAtiva = false;
    private bool contagemInicialAtiva = false;
    private GameObject painelPausa;
    private Slider sliderEfeitosPausa;
    private Button botaoContinuarPausa;
    private Button botaoReiniciarPausa;
    private Button botaoMenuPausa;

    private void Awake()
    {
        // Impede duplicar o controlador caso ele exista mais de uma vez na cena.
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        Time.timeScale = 1f;

        // Procura UI automática caso algo não esteja ligado no inspector.
        EncontrarReferenciasUISeNecessario();

        pontosAtuais = Mathf.Max(pontosMinimos, pontosIniciais);
        AtualizarTextoPontos();

        if (painelFim != null)
            painelFim.SetActive(false);

        if (textoContagem != null)
            textoContagem.gameObject.SetActive(false);

        if (fundoContagem != null)
            fundoContagem.gameObject.SetActive(false);

        CriarOuEncontrarMenuPausa();
    }

    private void Start()
    {
        if (!usarContagemInicial) return;
        StartCoroutine(RodarContagemInicial());
    }

    private void Update()
    {
        if (!habilitarMenuPausa) return;
        if (partidaFinalizada) return;
        if (contagemInicialAtiva) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (pausaAtiva)
            RetomarPartida();
        else
            PausarPartida();
    }

    public void DescontarPontos(int valor)
    {
        if (partidaFinalizada) return;

        // Nunca deixa a pontuação cair abaixo do mínimo configurado.
        pontosAtuais = Mathf.Max(pontosMinimos, pontosAtuais - Mathf.Abs(valor));
        AtualizarTextoPontos();
    }

    public void RegistrarCaptura()
    {
        if (partidaFinalizada) return;
        partidaFinalizada = true;
        // Captura = vitória do policial.
        MostrarTelaFinal("Parabéns Fugitivo Capturado!", $"Você fez {pontosAtuais} pontos", false);
    }

    public void RegistrarFuga()
    {
        if (partidaFinalizada) return;
        partidaFinalizada = true;

        // Se o fugitivo escapar, a missao falhou e a pontuacao final zera.
        pontosAtuais = 0;
        AtualizarTextoPontos();

        MostrarTelaFinal("A Missão Falhou!", $"Fugitivo Escapou!\n\nVoce fez {pontosAtuais} pontos", true);
    }

    private void AtualizarTextoPontos()
    {
        if (textoPontos != null)
            textoPontos.text = $"Pontos: {pontosAtuais}";
    }

    private void MostrarTelaFinal(string titulo, string textoPontosFinal, bool permitirReiniciar)
    {
        // A tela final sempre pausa o jogo para impedir entrada apos o resultado.
        FecharMenuPausaSilenciosamente();
        if (tituloFim != null) tituloFim.text = titulo;
        if (pontosFim != null) pontosFim.text = textoPontosFinal;
        if (botaoReiniciarFim != null) botaoReiniciarFim.SetActive(permitirReiniciar);
        if (botaoMenuFim != null) botaoMenuFim.SetActive(true);
        if (painelFim != null) painelFim.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ReiniciarPartida()
    {
        FecharMenuPausaSilenciosamente();
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
        FecharMenuPausaSilenciosamente();
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
        // Fallback para evitar cena quebrada caso alguma referencia nao tenha sido ligada no Inspector.
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

        // Pausa a simulação e mostra contagem regressiva de início.
        contagemInicialAtiva = true;
        Time.timeScale = 0f;
        if (fundoContagem != null) fundoContagem.gameObject.SetActive(true);
        textoContagem.gameObject.SetActive(true);

        if (mostrarMensagensAntesDaContagem)
        {
            float duracaoMensagem = Mathf.Max(0.2f, duracaoMensagemInicial);
            textoContagem.fontSize = Mathf.Max(24f, tamanhoFonteMensagensIniciais);

            if (!string.IsNullOrWhiteSpace(mensagemInicial1))
            {
                textoContagem.text = mensagemInicial1;
                yield return new WaitForSecondsRealtime(duracaoMensagem);
            }

            if (!string.IsNullOrWhiteSpace(mensagemInicial2))
            {
                textoContagem.text = mensagemInicial2;
                yield return new WaitForSecondsRealtime(duracaoMensagem);
            }
        }

        textoContagem.fontSize = Mathf.Max(24f, tamanhoFonteContagemNumerica);
        int total = Mathf.Max(1, segundosContagem);
        for (int i = total; i >= 1; i--)
        {
            textoContagem.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        textoContagem.text = "JÁ!";
        yield return new WaitForSecondsRealtime(0.6f);

        textoContagem.gameObject.SetActive(false);
        if (fundoContagem != null) fundoContagem.gameObject.SetActive(false);
        Time.timeScale = 1f;
        contagemInicialAtiva = false;
    }

    private void PausarPartida()
    {
        if (painelPausa == null) return;
        pausaAtiva = true;
        AtualizarSlidersPausaComVolumeAtual();
        painelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    private void RetomarPartida()
    {
        pausaAtiva = false;
        if (painelPausa != null)
            painelPausa.SetActive(false);

        Time.timeScale = 1f;
    }

    private void FecharMenuPausaSilenciosamente()
    {
        pausaAtiva = false;
        if (painelPausa != null)
            painelPausa.SetActive(false);
    }

    private void CriarOuEncontrarMenuPausa()
    {
        GameObject encontrado = GameObject.Find("PainelPausa");
        if (encontrado == null)
            encontrado = EncontrarObjetoNaCenaInclusiveInativos("PainelPausa");

        if (encontrado == null)
        {
            Debug.LogWarning("GameplayPartidaController: PainelPausa nao encontrado na cena. Crie o painel completo na Hierarchy.");
            return;
        }

        painelPausa = encontrado;
        sliderEfeitosPausa = EncontrarSliderFilho(painelPausa.transform, "SliderEfeitosPausa");
        botaoContinuarPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoContinuarPausa");
        botaoReiniciarPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoReiniciarPausa");
        botaoMenuPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoMenuPausa");

        if (sliderEfeitosPausa == null || botaoContinuarPausa == null || botaoReiniciarPausa == null || botaoMenuPausa == null)
            Debug.LogWarning("GameplayPartidaController: PainelPausa existe, mas esta incompleto. Configure SliderEfeitosPausa e botoes na cena.");

        ConectarEventosMenuPausa();
        painelPausa.SetActive(false);
    }

    private void ConectarEventosMenuPausa()
    {
        if (sliderEfeitosPausa != null)
        {
            sliderEfeitosPausa.onValueChanged.RemoveListener(AoMudarSliderEfeitosPausa);
            sliderEfeitosPausa.onValueChanged.AddListener(AoMudarSliderEfeitosPausa);
        }

        if (botaoContinuarPausa != null)
        {
            botaoContinuarPausa.onClick.RemoveListener(RetomarPartida);
            botaoContinuarPausa.onClick.AddListener(RetomarPartida);
        }

        if (botaoReiniciarPausa != null)
        {
            botaoReiniciarPausa.onClick.RemoveListener(ReiniciarPartida);
            botaoReiniciarPausa.onClick.AddListener(ReiniciarPartida);
        }

        if (botaoMenuPausa != null)
        {
            botaoMenuPausa.onClick.RemoveListener(VoltarMenu);
            botaoMenuPausa.onClick.AddListener(VoltarMenu);
        }
    }

    private void AtualizarSlidersPausaComVolumeAtual()
    {
        if (sliderEfeitosPausa != null)
            sliderEfeitosPausa.SetValueWithoutNotify(AudiosScript.ObterVolumeEfeitosGlobal());
    }

    private void AoMudarSliderEfeitosPausa(float valor)
    {
        AudiosScript.DefinirVolumeEfeitosGlobal(valor);
    }


    private static Slider EncontrarSliderFilho(Transform raiz, string nome)
    {
        Transform t = raiz.Find(nome);
        return t != null ? t.GetComponent<Slider>() : null;
    }

    private static Button EncontrarBotaoFilho(Transform raiz, string nome)
    {
        Transform t = raiz.Find(nome);
        return t != null ? t.GetComponent<Button>() : null;
    }

    private static GameObject EncontrarObjetoNaCenaInclusiveInativos(string nome)
    {
        Scene cena = SceneManager.GetActiveScene();
        if (!cena.IsValid()) return null;

        GameObject[] raizes = cena.GetRootGameObjects();
        Stack<Transform> pilha = new Stack<Transform>();
        for (int i = 0; i < raizes.Length; i++)
            pilha.Push(raizes[i].transform);

        while (pilha.Count > 0)
        {
            Transform atual = pilha.Pop();
            if (atual.name == nome)
                return atual.gameObject;

            for (int i = 0; i < atual.childCount; i++)
                pilha.Push(atual.GetChild(i));
        }

        return null;
    }
}
