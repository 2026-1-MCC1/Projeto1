using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private bool usarCenaPlanejamento = true;
    [SerializeField] private string cenaPlanejamento = "CenaPlanejamento";
    [SerializeField] private string cenaPerseguicaoDireta = "CenaPrincipal";

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
}
