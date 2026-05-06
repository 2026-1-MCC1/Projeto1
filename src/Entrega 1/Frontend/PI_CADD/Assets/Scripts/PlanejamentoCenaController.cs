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
    [SerializeField] private bool bloquearAreaInicialDosCarros = true;
    [SerializeField] private float margemBloqueioCarros = 4f;
    [SerializeField] private Vector3 tamanhoMinimoBloqueio = new Vector3(14f, 2f, 14f);

    private void Start()
    {
        if (congelarRigidbodiesDaCena)
            CongelarRigidbodiesExistentes();

        if (bloquearAreaInicialDosCarros)
            CriarBloqueioPosicionamentoDosCarros();
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

    private void CriarBloqueioPosicionamentoDosCarros()
    {
        if (GameObject.Find("AreaBloqueadaCarros") != null) return;

        PolicialScript[] policiais = FindObjectsOfType<PolicialScript>(true);
        FugitivoScript[] fugitivos = FindObjectsOfType<FugitivoScript>(true);
        if (policiais.Length == 0 || fugitivos.Length == 0) return;

        Transform tPolicial = policiais[0].transform;
        Transform tFugitivo = fugitivos[0].transform;

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

        Vector3 size = bounds.size;
        size.x = Mathf.Max(tamanhoMinimoBloqueio.x, size.x + margemBloqueioCarros);
        size.y = Mathf.Max(tamanhoMinimoBloqueio.y, size.y + 1f);
        size.z = Mathf.Max(tamanhoMinimoBloqueio.z, size.z + margemBloqueioCarros);

        GameObject area = new GameObject("AreaBloqueadaCarros");
        area.transform.position = bounds.center + new Vector3(0f, size.y * 0.5f, 0f);

        BoxCollider box = area.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = size;
        box.center = Vector3.zero;

        area.AddComponent<BloqueioPosicionamentoArea>();
    }
}
