using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScrpit : MonoBehaviour
{
    
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;

    public void IniciarJogo()
    {
       
    }
    public void AbrirOpções()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);

    }
    public void FecharOpções()
    {
        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }
    public void AbrirCreditos()
    {
        painelCreditos.SetActive(true);
        painelMenuInicial.SetActive(false);
    }
    public void SairCreditos()
    {
        painelCreditos.SetActive(false);
        painelMenuInicial.SetActive(true);
    }
}