using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanejamentoCenaController : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string cenaPerseguicao = "CenaPrincipal";
    [SerializeField] private string cenaMenu = "Menu";
    [Header("Atalhos")]
    [SerializeField] private bool habilitarAtalhoEnter = true;

    private void Update()
    {
        if (habilitarAtalhoEnter && Input.GetKeyDown(KeyCode.Return))
            IniciarPerseguicao();
    }

    public void IniciarPerseguicao()
    {
        if (!Application.CanStreamedLevelBeLoaded(cenaPerseguicao))
        {
            Debug.LogError($"PlanejamentoCenaController: cena '{cenaPerseguicao}' não está no Build Settings.");
            return;
        }

        SceneManager.LoadScene(cenaPerseguicao);
    }

    public void LimparPlanejamento()
    {
        PlanejamentoRuntimeData.LimparPlano();
    }

    public void VoltarMenu()
    {
        PlanejamentoRuntimeData.LimparPlano();

        if (Application.CanStreamedLevelBeLoaded(cenaMenu))
            SceneManager.LoadScene(cenaMenu);
    }
}
