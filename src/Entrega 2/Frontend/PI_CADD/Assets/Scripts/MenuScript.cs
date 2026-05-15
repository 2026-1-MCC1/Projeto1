using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelGameOver;
    [Header("Fundo Opcoes")]
    [SerializeField] private Image imagemFundoOpcoes;
    [SerializeField] private Sprite spriteFundoOpcoes;
    [SerializeField, Range(0f, 1f)] private float opacidadeFundoOpcoes = 1f;
    [SerializeField] private bool usarCenaPlanejamento = true;
    [SerializeField] private string cenaPlanejamento = "CenaPlanejamento";
    [SerializeField] private string cenaPerseguicaoDireta = "CenaPrincipal";

    private void Awake()
    {
        GarantirFundoPainelOpcoes();
    }

    // Inicia o jogo pela cena de planejamento (quando habilitado) ou direto na perseguicao.
    public void IniciarJogo()
    {
        // Fecha o menu principal antes de trocar de cena.
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);

        // Limpa qualquer plano antigo para a nova partida começar do zero.
        PlanejamentoRuntimeData.LimparPlano();

        string cenaDestino = cenaPerseguicaoDireta;
        if (usarCenaPlanejamento && Application.CanStreamedLevelBeLoaded(cenaPlanejamento))
            cenaDestino = cenaPlanejamento;

        SceneManager.LoadScene(cenaDestino);
    }

    // Abre o painel de opcoes.
    public void AbrirOpcoes()
    {
        GarantirFundoPainelOpcoes();
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelOpcoes != null) painelOpcoes.SetActive(true);
    }

    // Fecha o painel de opcoes.
    public void FecharOpcoes()
    {
        if (painelOpcoes != null) painelOpcoes.SetActive(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
    }

    // Abre o painel de creditos.
    public void AbrirCreditos()
    {
        // Mostra créditos e esconde menu inicial.
        if (painelCreditos != null) painelCreditos.SetActive(true);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
    }

    // Fecha o painel de creditos.
    public void SairCreditos()
    {
        // Volta para o menu principal.
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
    }

    // Fecha o painel de game over.
    public void SairGameOver()
    {
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
    }

    private void GarantirFundoPainelOpcoes()
    {
        if (painelOpcoes == null) return;

        if (imagemFundoOpcoes == null)
        {
            Transform existente = painelOpcoes.transform.Find("BackgroundOpcoes");
            if (existente != null)
                imagemFundoOpcoes = existente.GetComponent<Image>();
        }

        if (imagemFundoOpcoes == null)
        {
            GameObject fundo = new GameObject("BackgroundOpcoes");
            fundo.transform.SetParent(painelOpcoes.transform, false);
            fundo.transform.SetAsFirstSibling();

            RectTransform rt = fundo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            imagemFundoOpcoes = fundo.AddComponent<Image>();
            imagemFundoOpcoes.raycastTarget = false;
            imagemFundoOpcoes.type = Image.Type.Simple;
        }

        if (imagemFundoOpcoes.sprite == null)
            imagemFundoOpcoes.sprite = ObterSpriteFundoOpcoes();

        Color corAtual = imagemFundoOpcoes.color;
        corAtual.a = Mathf.Clamp01(opacidadeFundoOpcoes);
        imagemFundoOpcoes.color = corAtual;
    }

    private Sprite ObterSpriteFundoOpcoes()
    {
        if (spriteFundoOpcoes != null) return spriteFundoOpcoes;
        if (painelMenuInicial == null) return null;

        Image[] imagens = painelMenuInicial.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imagens.Length; i++)
        {
            Image img = imagens[i];
            if (img == null || img.sprite == null) continue;
            return img.sprite;
        }

        return null;
    }
}
