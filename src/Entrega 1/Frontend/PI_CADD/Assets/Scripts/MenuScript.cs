using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelGameOver;

    // Fun��o chamada quando o jogador clica em "Iniciar Jogo"
    // Aqui voc� pode colocar o c�digo para carregar a cena do jogo
    public void IniciarJogo()
    {
       painelMenuInicial.SetActive(false);
       SceneManager.LoadScene("CenaPrincipal");
    }

    // Abre o painel de op��es e esconde o menu inicial
    public void AbrirOpcoes()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);

    }

    // Fecha o painel de op��es e volta para o menu inicial
    public void FecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }
    // Abre o painel de cr�ditos e esconde o menu inicial
    public void AbrirCreditos()
    {
        painelCreditos.SetActive(true);
        painelMenuInicial.SetActive(false);
    }

    // Fecha o painel de cr�ditos e volta para o menu inicial
    public void SairCreditos()
    {
        painelCreditos.SetActive(false);
        painelMenuInicial.SetActive(true);
    }

    // Fecha o painel de Game Over e volta para o menu inicial
    public void SairGameOver()
    {
        painelGameOver.SetActive(false);
        painelMenuInicial.SetActive(true);
    }

}