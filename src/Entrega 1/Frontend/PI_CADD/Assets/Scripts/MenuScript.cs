using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScrpit : MonoBehaviour
{
    
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelGameOver;

    // Função chamada quando o jogador clica em "Iniciar Jogo"
    // Aqui você pode colocar o código para carregar a cena do jogo
    public void IniciarJogo()
    {
       
    }

    // Abre o painel de opções e esconde o menu inicial
    public void AbrirOpções()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);

    }

    // Fecha o painel de opções e volta para o menu inicial
    public void FecharOpções()
    {
        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }
    // Abre o painel de créditos e esconde o menu inicial
    public void AbrirCreditos()
    {
        painelCreditos.SetActive(true);
        painelMenuInicial.SetActive(false);
    }

    // Fecha o painel de créditos e volta para o menu inicial
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