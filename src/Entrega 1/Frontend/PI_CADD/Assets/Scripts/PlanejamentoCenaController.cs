using UnityEngine;
using UnityEngine.SceneManagement;

public class PlanejamentoCenaController : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string cenaPerseguicao = "CenaPrincipal";
    [SerializeField] private string cenaMenu = "Menu";

    [Header("Atalhos")]
    [SerializeField] private bool habilitarAtalhoEnter = true;

    [Header("Planejamento")]
    [SerializeField] private bool congelarRigidbodiesDaCena = true;

    private void Start()
    {
        if (congelarRigidbodiesDaCena)
            CongelarRigidbodiesExistentes();
    }

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
}
