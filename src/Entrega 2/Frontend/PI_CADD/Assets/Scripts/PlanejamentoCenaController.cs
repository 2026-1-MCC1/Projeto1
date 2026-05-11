using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlanejamentoCenaController : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string cenaPerseguicao = "CenaPrincipal";
    [SerializeField] private string cenaMenu = "Menu";

    [Header("Atalhos")]
    [SerializeField] private bool habilitarAtalhoEnter = true;

    [Header("Planejamento")]
    [SerializeField] private bool congelarRigidbodiesDaCena = true;
    [SerializeField] private bool bloquearAreaInicialDosCarros = true;
    [SerializeField] private float margemBloqueioCarros = 4f;
    [SerializeField] private Vector3 tamanhoMinimoBloqueio = new Vector3(14f, 2f, 14f);

    [Header("UI de Obrigatoriedade")]
    [SerializeField] private Button botaoIniciarPerseguicao;
    [SerializeField] private bool exigirAoMenosUmItem = true;

    [Header("UI de Dicas (Tutorial)")]
    [SerializeField] private bool mostrarDicasEnquantoSemItem = true;
    [SerializeField] private GameObject[] caixasDicaSemItem;
    [SerializeField] private bool mostrarDicasEmSequencia = true;
    [SerializeField] private float duracaoTransicaoDicas = 0.2f;
    [Header("Pausa (ESC)")]
    [SerializeField] private bool habilitarMenuPausa = true;

    private bool estadoAnteriorTemItens;
    private CanvasGroup[] gruposDica;
    private int indiceDicaAtual = -1;
    private bool tutorialConcluido = false;
    private Coroutine rotinaTrocaDica;
    private bool pausaAtiva = false;
    private GameObject painelPausa;
    private Slider sliderMusicaPausa;
    private Button botaoContinuarPausa;
    private Button botaoReiniciarPausa;
    private Button botaoMenuPausa;

    private void Start()
    {
        // No planejamento, "congela" física para itens da cena não saírem do lugar.
        if (congelarRigidbodiesDaCena)
            CongelarRigidbodiesExistentes();

        // Cria uma área onde o jogador não pode posicionar obstáculos sobre os carros.
        if (bloquearAreaInicialDosCarros)
            CriarBloqueioPosicionamentoDosCarros();

        PrepararTutorialDicas();
        CriarOuEncontrarMenuPausa();

        // Sincroniza botão e dicas com o estado atual do planejamento.
        AtualizarEstadoObrigatoriedadeEDeDicas(true);
    }

    private void Update()
    {
        if (habilitarMenuPausa && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausaAtiva)
                RetomarPlanejamento();
            else
                PausarPlanejamento();
            return;
        }

        if (pausaAtiva) return;

        AtualizarEstadoObrigatoriedadeEDeDicas(false);

        // Atalho Enter só funciona quando já existe item no planejamento.
        if (habilitarAtalhoEnter && PodeIniciarPerseguicao() && Input.GetKeyDown(KeyCode.Return))
            IniciarPerseguicao();
    }

    public void IniciarPerseguicao()
    {
        FecharMenuPausaSilenciosamente();

        // Regra da fase: o jogador precisa posicionar ao menos 1 item.
        if (!PodeIniciarPerseguicao())
        {
            AtualizarEstadoObrigatoriedadeEDeDicas(true);
            return;
        }

        // Só troca de cena se ela estiver adicionada no Build Settings.
        if (!Application.CanStreamedLevelBeLoaded(cenaPerseguicao))
        {
            Debug.LogError($"PlanejamentoCenaController: cena '{cenaPerseguicao}' não está no Build Settings.");
            return;
        }

        SceneManager.LoadScene(cenaPerseguicao);
    }

    public void LimparPlanejamento()
    {
        // Botão utilitário para limpar tudo que foi colocado.
        PlanejamentoRuntimeData.LimparPlano();
        ReiniciarTutorialDicas();
        AtualizarEstadoObrigatoriedadeEDeDicas(true);
    }

    public void VoltarMenu()
    {
        FecharMenuPausaSilenciosamente();
        PlanejamentoRuntimeData.LimparPlano();
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(cenaMenu))
            SceneManager.LoadScene(cenaMenu);
    }

    public void ReiniciarPlanejamento()
    {
        FecharMenuPausaSilenciosamente();
        Time.timeScale = 1f;
        Scene cenaAtual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(cenaAtual.name);
    }

    private bool PodeIniciarPerseguicao()
    {
        if (!exigirAoMenosUmItem) return true;
        return PlanejamentoRuntimeData.TemItensPlanejados;
    }

    private void AtualizarEstadoObrigatoriedadeEDeDicas(bool forcarAtualizacao)
    {
        bool temItensPlanejados = PlanejamentoRuntimeData.TemItensPlanejados;
        if (!forcarAtualizacao && temItensPlanejados == estadoAnteriorTemItens) return;

        estadoAnteriorTemItens = temItensPlanejados;
        bool podeIniciar = PodeIniciarPerseguicao();

        // Controle do botão feito por referência de UI já criada na cena.
        if (botaoIniciarPerseguicao != null)
            botaoIniciarPerseguicao.interactable = podeIniciar;

        // Exibe as caixas de dica enquanto nenhum item foi colocado.
        if (!mostrarDicasEnquantoSemItem || caixasDicaSemItem == null) return;

        if (!mostrarDicasEmSequencia)
        {
            bool mostrarDicas = !temItensPlanejados;
            for (int i = 0; i < caixasDicaSemItem.Length; i++)
            {
                if (caixasDicaSemItem[i] == null) continue;
                caixasDicaSemItem[i].SetActive(mostrarDicas);
            }
            return;
        }

        // No modo sequencial, as dicas só somem quando o jogador conclui todas.
        // Colocar o primeiro item NÃO deve esconder a dica atual.
        if (tutorialConcluido)
        {
            OcultarTodasAsDicas(true);
            return;
        }

        if (indiceDicaAtual < 0)
            MostrarDicaPorIndice(0);
    }

    public void FecharDicaAtual()
    {
        if (!mostrarDicasEmSequencia) return;
        if (indiceDicaAtual < 0) return;

        int proximoIndice = ProximoIndiceValido(indiceDicaAtual + 1);
        if (proximoIndice < 0)
        {
            tutorialConcluido = true;
            TrocarDica(-1);
            return;
        }

        TrocarDica(proximoIndice);
    }

    private void PrepararTutorialDicas()
    {
        if (caixasDicaSemItem == null || caixasDicaSemItem.Length == 0) return;

        gruposDica = new CanvasGroup[caixasDicaSemItem.Length];
        for (int i = 0; i < caixasDicaSemItem.Length; i++)
        {
            GameObject caixa = caixasDicaSemItem[i];
            if (caixa == null) continue;

            CanvasGroup grupo = caixa.GetComponent<CanvasGroup>();
            if (grupo == null)
            {
                Debug.LogWarning($"PlanejamentoCenaController: '{caixa.name}' precisa de CanvasGroup na cena para transicao de dica.");
                continue;
            }

            grupo.alpha = 0f;
            grupo.interactable = false;
            grupo.blocksRaycasts = false;
            caixa.SetActive(false);
            gruposDica[i] = grupo;
        }
    }

    private void ReiniciarTutorialDicas()
    {
        tutorialConcluido = false;
        indiceDicaAtual = -1;
        OcultarTodasAsDicas(true);
    }

    private void MostrarDicaPorIndice(int indice)
    {
        if (indice < 0 || caixasDicaSemItem == null || indice >= caixasDicaSemItem.Length) return;
        TrocarDica(indice);
    }

    private void TrocarDica(int proximoIndice)
    {
        if (rotinaTrocaDica != null)
            StopCoroutine(rotinaTrocaDica);

        rotinaTrocaDica = StartCoroutine(RotinaTrocarDica(proximoIndice));
    }

    private IEnumerator RotinaTrocarDica(int proximoIndice)
    {
        int indiceAnterior = indiceDicaAtual;

        if (indiceAnterior >= 0 && indiceAnterior < caixasDicaSemItem.Length)
            yield return FadeDica(indiceAnterior, 0f, false);

        indiceDicaAtual = -1;

        if (proximoIndice < 0)
        {
            rotinaTrocaDica = null;
            yield break;
        }

        if (proximoIndice >= caixasDicaSemItem.Length || caixasDicaSemItem[proximoIndice] == null)
        {
            rotinaTrocaDica = null;
            yield break;
        }

        caixasDicaSemItem[proximoIndice].SetActive(true);
        yield return FadeDica(proximoIndice, 1f, true);
        indiceDicaAtual = proximoIndice;
        rotinaTrocaDica = null;
    }

    private IEnumerator FadeDica(int indice, float alphaAlvo, bool habilitarInteracaoNoFinal)
    {
        if (gruposDica == null || indice < 0 || indice >= gruposDica.Length) yield break;

        CanvasGroup grupo = gruposDica[indice];
        GameObject caixa = caixasDicaSemItem[indice];
        if (grupo == null || caixa == null) yield break;

        if (duracaoTransicaoDicas <= 0.001f)
        {
            grupo.alpha = alphaAlvo;
            grupo.interactable = habilitarInteracaoNoFinal;
            grupo.blocksRaycasts = habilitarInteracaoNoFinal;
            if (alphaAlvo <= 0.001f) caixa.SetActive(false);
            yield break;
        }

        float alphaInicial = grupo.alpha;
        float tempo = 0f;

        if (alphaAlvo > alphaInicial)
        {
            grupo.interactable = false;
            grupo.blocksRaycasts = false;
        }

        while (tempo < duracaoTransicaoDicas)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / duracaoTransicaoDicas);
            grupo.alpha = Mathf.Lerp(alphaInicial, alphaAlvo, t);
            yield return null;
        }

        grupo.alpha = alphaAlvo;
        grupo.interactable = habilitarInteracaoNoFinal;
        grupo.blocksRaycasts = habilitarInteracaoNoFinal;

        if (alphaAlvo <= 0.001f)
            caixa.SetActive(false);
    }

    private void OcultarTodasAsDicas(bool imediato)
    {
        if (caixasDicaSemItem == null) return;

        if (rotinaTrocaDica != null)
        {
            StopCoroutine(rotinaTrocaDica);
            rotinaTrocaDica = null;
        }

        for (int i = 0; i < caixasDicaSemItem.Length; i++)
        {
            GameObject caixa = caixasDicaSemItem[i];
            if (caixa == null) continue;

            if (gruposDica != null && i < gruposDica.Length && gruposDica[i] != null)
            {
                gruposDica[i].alpha = 0f;
                gruposDica[i].interactable = false;
                gruposDica[i].blocksRaycasts = false;
            }

            if (imediato || mostrarDicasEmSequencia)
                caixa.SetActive(false);
        }

        indiceDicaAtual = -1;
    }

    private int ProximoIndiceValido(int indiceInicial)
    {
        if (caixasDicaSemItem == null) return -1;

        for (int i = Mathf.Max(0, indiceInicial); i < caixasDicaSemItem.Length; i++)
        {
            if (caixasDicaSemItem[i] != null)
                return i;
        }
        return -1;
    }

    private void PausarPlanejamento()
    {
        if (painelPausa == null) return;
        pausaAtiva = true;
        AtualizarSlidersPausaComVolumeAtual();
        painelPausa.transform.SetAsLastSibling();
        painelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    private void RetomarPlanejamento()
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
        GameObject encontrado = GameObject.Find("PainelPausaPlanejamento");
        if (encontrado == null)
            encontrado = EncontrarObjetoNaCenaInclusiveInativos("PainelPausaPlanejamento");
        if (encontrado == null)
            encontrado = GameObject.Find("PainelPausa");
        if (encontrado == null)
            encontrado = EncontrarObjetoNaCenaInclusiveInativos("PainelPausa");

        if (encontrado == null)
        {
            Debug.LogWarning("PlanejamentoCenaController: Painel de pausa nao encontrado na cena (PainelPausaPlanejamento/PainelPausa).");
            return;
        }

        painelPausa = encontrado;
        sliderMusicaPausa = EncontrarSliderFilho(painelPausa.transform, "SliderMusicaPausa");

        // Compatibilidade: se existir apenas 1 slider com nome legado de efeitos,
        // ele passa a controlar musica no planejamento.
        if (sliderMusicaPausa == null)
        {
            sliderMusicaPausa = EncontrarSliderFilho(painelPausa.transform, "SliderEfeitosPausa");
            if (sliderMusicaPausa != null)
                Debug.Log("PlanejamentoCenaController: usando slider unico como controle de musica.");
        }

        botaoContinuarPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoContinuarPausa");
        botaoReiniciarPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoReiniciarPausa");
        botaoMenuPausa = EncontrarBotaoFilho(painelPausa.transform, "BotaoMenuPausa");

        if (sliderMusicaPausa == null || botaoContinuarPausa == null || botaoReiniciarPausa == null || botaoMenuPausa == null)
            Debug.LogWarning("PlanejamentoCenaController: painel de pausa incompleto. Configure SliderMusicaPausa e os 3 botoes.");

        ConectarEventosMenuPausa();
        painelPausa.SetActive(false);
    }

    private void ConectarEventosMenuPausa()
    {
        if (sliderMusicaPausa != null)
        {
            sliderMusicaPausa.onValueChanged.RemoveListener(AoMudarSliderMusicaPausa);
            sliderMusicaPausa.onValueChanged.AddListener(AoMudarSliderMusicaPausa);
        }

        if (botaoContinuarPausa != null)
        {
            botaoContinuarPausa.onClick.RemoveListener(RetomarPlanejamento);
            botaoContinuarPausa.onClick.AddListener(RetomarPlanejamento);
        }

        if (botaoReiniciarPausa != null)
        {
            botaoReiniciarPausa.onClick.RemoveListener(ReiniciarPlanejamento);
            botaoReiniciarPausa.onClick.AddListener(ReiniciarPlanejamento);
        }

        if (botaoMenuPausa != null)
        {
            botaoMenuPausa.onClick.RemoveListener(VoltarMenu);
            botaoMenuPausa.onClick.AddListener(VoltarMenu);
        }
    }

    private void AtualizarSlidersPausaComVolumeAtual()
    {
        if (AudiosScript.instancia == null) return;

        if (sliderMusicaPausa != null)
            sliderMusicaPausa.SetValueWithoutNotify(Mathf.Clamp01(AudiosScript.instancia.volumeMusica));
    }

    private void AoMudarSliderMusicaPausa(float valor)
    {
        if (AudiosScript.instancia != null)
            AudiosScript.instancia.MudarVolumeMusica(valor);
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

    private void CongelarRigidbodiesExistentes()
    {
        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null) continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void CriarBloqueioPosicionamentoDosCarros()
    {
        if (GameObject.Find("AreaBloqueadaCarros") != null) return;

        PolicialScript[] policiais = FindObjectsByType<PolicialScript>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        FugitivoScript[] fugitivos = FindObjectsByType<FugitivoScript>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (policiais.Length == 0 || fugitivos.Length == 0) return;

        Transform tPolicial = policiais[0].transform;
        Transform tFugitivo = fugitivos[0].transform;

        // Calcula um volume que cobre os colliders de policial + fugitivo.
        Bounds bounds = new Bounds(tPolicial.position, Vector3.zero);
        bool encontrouAlgumCollider = false;

        Collider[] colsPolicial = tPolicial.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colsPolicial.Length; i++)
        {
            bounds.Encapsulate(colsPolicial[i].bounds);
            encontrouAlgumCollider = true;
        }

        Collider[] colsFugitivo = tFugitivo.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colsFugitivo.Length; i++)
        {
            bounds.Encapsulate(colsFugitivo[i].bounds);
            encontrouAlgumCollider = true;
        }

        if (!encontrouAlgumCollider)
            bounds.Encapsulate(tFugitivo.position);

        // Expande um pouco a área para dar margem de segurança.
        Vector3 size = bounds.size;
        size.x = Mathf.Max(tamanhoMinimoBloqueio.x, size.x + margemBloqueioCarros);
        size.y = Mathf.Max(tamanhoMinimoBloqueio.y, size.y + 1f);
        size.z = Mathf.Max(tamanhoMinimoBloqueio.z, size.z + margemBloqueioCarros);

        // Objeto trigger que marca "área proibida para posicionamento".
        GameObject area = new GameObject("AreaBloqueadaCarros");
        area.transform.position = bounds.center + new Vector3(0f, size.y * 0.5f, 0f);

        BoxCollider box = area.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        box.center = Vector3.zero;

        area.AddComponent<BloqueioPosicionamentoArea>();
    }
}
