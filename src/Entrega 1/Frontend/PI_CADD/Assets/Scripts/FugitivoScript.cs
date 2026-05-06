using UnityEngine;
using UnityEngine.AI;

public class FugitivoScript : MonoBehaviour
{
    [Header("Navegacao")]
    public Transform pontoFuga;                    // Destino final de fuga
    public float distanciaParaEscapar = 1.5f;      // Distância para considerar que escapou

    [Header("Captura")]
    public float distanciaParaSerPego = 3f;        // Distância do policial para ser capturado
    public Transform policial;                     // Referência ao policial — arrasta no Inspector

    [Header("Pontuacao")]
    public int penalidadeColisao = 3;
    public float velocidadeMinimaImpacto = 2.5f;
    public float cooldownPenalidade = 0.25f;
    [Header("Detector de Colisao do Cenario")]
    public float raioDetectorCenario = 1.3f;
    public float alturaDetectorCenario = 0.8f;
    [Header("Animacao Visual")]
    public bool garantirAnimacaoRodas = true;

    private NavMeshAgent fugitivo;
    private Rigidbody rb;
    private bool foiPego = false;
    private bool escapou = false;
    private float ultimoImpactoTempo = -999f;
    private SphereCollider detectorCenario;

    void Start()
    {
        fugitivo = GetComponent<NavMeshAgent>();

        if (fugitivo == null)
        {
            Debug.LogError("FugitivoScript: NavMeshAgent não encontrado no objeto do fugitivo.");
            enabled = false;
            return;
        }

        if (pontoFuga == null)
        {
            Debug.LogError("FugitivoScript: pontoFuga não foi atribuído no Inspector.");
            enabled = false;
            return;
        }

        if (!fugitivo.isOnNavMesh)
        {
            Debug.LogError("FugitivoScript: fugitivo está fora da NavMesh.");
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Evita conflito entre física e NavMeshAgent (causa comum de deslizamento).
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        CriarDetectorCenarioSeNecessario();

        if (garantirAnimacaoRodas)
            GarantirAnimacaoVisualRodas();

        fugitivo.isStopped = false;
        fugitivo.SetDestination(pontoFuga.position);
    }

    void Update()
    {
        if (foiPego || escapou) return;

        VerificarCapturaPorDistancia();
        VerificarFuga();
    }

    void VerificarCapturaPorDistancia()
    {
        if (policial == null) return;

        float distancia = Vector3.Distance(transform.position, policial.position);
        if (distancia <= distanciaParaSerPego)
            CapturarFugitivo();
    }

    private void CapturarFugitivo()
    {
        if (foiPego || escapou) return;

        foiPego = true;

        if (fugitivo != null)
        {
            fugitivo.isStopped = true;
            fugitivo.ResetPath();
        }

        StartCoroutine(RodarFugitivo());

        if (policial != null)
        {
            PolicialScript ps = policial.GetComponent<PolicialScript>();
            if (ps != null) ps.FugitivoPego();
        }
    }

    void VerificarFuga()
    {
        if (pontoFuga == null) return;

        float distanciaAoPonto = Vector3.Distance(transform.position, pontoFuga.position);

        if (distanciaAoPonto <= distanciaParaEscapar)
        {
            escapou = true;
            Debug.Log("Fugitivo escapou!");

            if (policial != null)
            {
                PolicialScript ps = policial.GetComponent<PolicialScript>();
                if (ps != null) ps.FugitivoEscapou();
            }

            GameplayPartidaController partida = GameplayPartidaController.Instancia;
            if (partida != null)
                partida.RegistrarFuga();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (foiPego || escapou) return;
        if (collision == null) return;

        PolicialScript policialScript = collision.gameObject.GetComponent<PolicialScript>();
        if (policialScript != null)
        {
            CapturarFugitivo();
            return;
        }

        if (!PodePenalizar(collision)) return;

        ColisaoSom.TocarSomBatidaGlobal(transform.position);

        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida != null)
            partida.DescontarPontos(penalidadeColisao);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (foiPego || escapou) return;
        if (other == null) return;

        if (other.GetComponent<PolicialScript>() != null)
        {
            CapturarFugitivo();
            return;
        }

        if (!PodePenalizar(other)) return;

        ColisaoSom.TocarSomBatidaGlobal(transform.position);

        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida != null)
            partida.DescontarPontos(penalidadeColisao);
    }

    private bool PodePenalizar(Collision collision)
    {
        if (collision == null) return false;
        if (Time.time - ultimoImpactoTempo < cooldownPenalidade) return false;
        float velocidadeImpacto = Mathf.Max(collision.relativeVelocity.magnitude, fugitivo != null ? fugitivo.velocity.magnitude : 0f);
        if (velocidadeImpacto < velocidadeMinimaImpacto) return false;

        GameObject outro = collision.gameObject;
        if (outro == null) return false;

        if (outro.GetComponent<PolicialScript>() != null) return false;
        if (outro.GetComponent<FugitivoScript>() != null) return false;

        string nome = outro.name.ToLowerInvariant();
        if (nome.Contains("ground") || nome.Contains("road")) return false;

        ultimoImpactoTempo = Time.time;
        return true;
    }

    private bool PodePenalizar(Collider outroCollider)
    {
        if (outroCollider == null) return false;
        if (Time.time - ultimoImpactoTempo < cooldownPenalidade) return false;
        if (fugitivo != null && fugitivo.velocity.magnitude < velocidadeMinimaImpacto) return false;

        GameObject outro = outroCollider.gameObject;
        if (outro == null) return false;
        if (outro == gameObject) return false;

        if (outro.GetComponent<PolicialScript>() != null) return false;
        if (outro.GetComponent<FugitivoScript>() != null) return false;

        string nome = outro.name.ToLowerInvariant();
        if (nome.Contains("ground") || nome.Contains("road")) return false;

        if (outroCollider.isTrigger) return false;

        ultimoImpactoTempo = Time.time;
        return true;
    }

    private void CriarDetectorCenarioSeNecessario()
    {
        Transform existente = transform.Find("DetectorColisaoCenario");
        if (existente != null)
        {
            detectorCenario = existente.GetComponent<SphereCollider>();
            if (detectorCenario != null) detectorCenario.isTrigger = true;
            return;
        }

        GameObject detector = new GameObject("DetectorColisaoCenario");
        detector.transform.SetParent(transform);
        detector.transform.localRotation = Quaternion.identity;
        detector.transform.localPosition = new Vector3(0f, alturaDetectorCenario, 0f);
        detector.transform.localScale = Vector3.one;

        detectorCenario = detector.AddComponent<SphereCollider>();
        detectorCenario.isTrigger = true;
        detectorCenario.radius = Mathf.Max(0.5f, raioDetectorCenario);
    }

    System.Collections.IEnumerator RodarFugitivo()
    {
        float tempo = 0f;
        float duracaoRotacao = 3f;

        while (tempo < duracaoRotacao)
        {
            transform.Rotate(0, 200f * Time.deltaTime, 0);
            tempo += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Fugitivo foi pego!");
    }

    private void GarantirAnimacaoVisualRodas()
    {
        AnimacaoVisualRodasCarro animacao = GetComponent<AnimacaoVisualRodasCarro>();
        if (animacao == null)
            animacao = gameObject.AddComponent<AnimacaoVisualRodasCarro>();

        animacao.AutoDetectarRodas();
    }
}
