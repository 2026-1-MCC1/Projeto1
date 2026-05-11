using UnityEngine;
using UnityEngine.AI;

public class FugitivoScript : MonoBehaviour
{
    // Valor base de penalidade aplicado quando o fugitivo bate em objetos relevantes.
    private const int PenalidadeColisaoFugitivo = 3;

    [Header("Navegacao")]
    public Transform pontoFuga;
    public float distanciaParaEscapar = 1.5f;

    [Header("Captura")]
    public float distanciaParaSerPego = 3f;
    public Transform policial;

    [Header("Pontuacao")]
    public int penalidadeColisao = 3;
    public float velocidadeMinimaImpacto = 2.5f;
    public float cooldownPenalidade = 0.25f;
    [Header("Som de Batida")]
    public bool garantirSomBatida = true;

    [Header("Detector de Colisao do Cenario")]
    public float raioDetectorCenario = 1.3f;
    public float alturaDetectorCenario = 0.8f;

    private NavMeshAgent fugitivo;
    private bool foiPego = false;
    private bool escapou = false;
    private float ultimoImpactoTempo = -999f;

    private void Start()
    {
        // Mantém penalidade padronizada independentemente do valor editado por acidente.
        penalidadeColisao = PenalidadeColisaoFugitivo;

        fugitivo = GetComponent<NavMeshAgent>();

        if (fugitivo == null)
        {
            Debug.LogError("FugitivoScript: NavMeshAgent nao encontrado no objeto do fugitivo.");
            enabled = false;
            return;
        }

        if (pontoFuga == null)
        {
            Debug.LogError("FugitivoScript: pontoFuga nao foi atribuido no Inspector.");
            enabled = false;
            return;
        }

        if (!fugitivo.isOnNavMesh)
        {
            Debug.LogError("FugitivoScript: fugitivo esta fora da NavMesh.");
            enabled = false;
            return;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Evita conflito entre fisica e NavMeshAgent.
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        if (garantirSomBatida)
            GarantirComponenteSomBatida();

        CriarDetectorCenarioSeNecessario();

        fugitivo.isStopped = false;
        // Define destino de fuga logo no início da cena.
        fugitivo.SetDestination(pontoFuga.position);
    }

    private void Update()
    {
        if (foiPego || escapou) return;

        // Mantem regras de vitoria/derrota avaliadas em todo frame.
        VerificarCapturaPorDistancia();
        VerificarFuga();
    }

    private void VerificarCapturaPorDistancia()
    {
        if (policial == null) return;

        // Se o policial estiver perto o suficiente, considera captura.
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

        // Rotacao visual curta para feedback de captura.
        StartCoroutine(RodarFugitivo());

        if (policial != null)
        {
            // Notifica o script do policial para encerrar a partida como vitória.
            PolicialScript ps = policial.GetComponent<PolicialScript>();
            if (ps != null) ps.FugitivoPego();
        }
    }

    private void VerificarFuga()
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
                // Notifica derrota do policial.
                partida.RegistrarFuga();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessarColisao(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProcessarColisao(collision);
    }

    private void ProcessarColisao(Collision collision)
    {
        if (foiPego || escapou) return;
        if (collision == null) return;

        PolicialScript policialScript = collision.collider != null
            ? collision.collider.GetComponentInParent<PolicialScript>()
            : null;
        if (policialScript != null)
        {
            CapturarFugitivo();
            return;
        }

        Rigidbody hitRb = collision.collider.attachedRigidbody;
        if (hitRb != null && !hitRb.isKinematic)
        {
            // Dá um empurrão visual no objeto batido para reforçar feedback de colisão.
            Vector3 dir = collision.contacts[0].point - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f) dir.Normalize();
            else dir = transform.forward;

            float forca = (fugitivo != null && fugitivo.velocity.magnitude > 1f) ? 15f : 6f;
            hitRb.AddForce((dir + Vector3.up * 0.4f) * forca, ForceMode.Impulse);
        }

        if (!PodePenalizar(collision)) return;

        ColisaoSom somBatida = GetComponent<ColisaoSom>();
        if (somBatida != null)
        {
            float velocidadeAtual = fugitivo != null ? fugitivo.velocity.magnitude : 0f;
            somBatida.TentarTocarPorContato(collision.gameObject, velocidadeAtual);
        }

        AplicarPenalidadeColisao();
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessarTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ProcessarTrigger(other);
    }

    private void ProcessarTrigger(Collider other)
    {
        if (foiPego || escapou) return;
        if (other == null) return;

        if (other.GetComponentInParent<PolicialScript>() != null)
        {
            CapturarFugitivo();
            return;
        }

        if (other.isTrigger) return;
        if (other.transform == transform || other.transform.IsChildOf(transform)) return;

        ColisaoSom somBatida = GetComponent<ColisaoSom>();
        if (somBatida == null) return;

        float velocidadeAtual = fugitivo != null ? fugitivo.velocity.magnitude : 0f;
        somBatida.TentarTocarPorContato(other.gameObject, velocidadeAtual);

        if (!PodePenalizar(other.gameObject, velocidadeAtual)) return;
        AplicarPenalidadeColisao();
    }

    private bool PodePenalizar(Collision collision)
    {
        if (collision == null) return false;
        float velocidadeImpacto = Mathf.Max(collision.relativeVelocity.magnitude, fugitivo != null ? fugitivo.velocity.magnitude : 0f);
        GameObject outro = collision.gameObject;
        return PodePenalizar(outro, velocidadeImpacto);
    }

    private bool PodePenalizar(GameObject outro, float velocidadeImpacto)
    {
        // Filtro de tempo e velocidade para evitar punição exagerada por contatos fracos.
        if (Time.time - ultimoImpactoTempo < cooldownPenalidade) return false;
        if (velocidadeImpacto < velocidadeMinimaImpacto) return false;
        if (outro == null) return false;

        if (outro.GetComponentInParent<PolicialScript>() != null) return false;
        if (outro.GetComponentInParent<FugitivoScript>() != null) return false;
        if (outro.GetComponentInParent<BloqueioPosicionamentoArea>() != null) return false;

        string nome = outro.name.ToLowerInvariant();
        // Ignora chão/rua/bloqueios no desconto de pontos.
        if (nome.Contains("ground") || nome.Contains("road") || nome.Contains("pista") || nome.Contains("lane") || nome.Contains("tile") || nome.Contains("bloqueio")) return false;

        ultimoImpactoTempo = Time.time;
        return true;
    }

    private void AplicarPenalidadeColisao()
    {
        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida == null)
            partida = FindFirstObjectByType<GameplayPartidaController>();
        if (partida != null)
            partida.DescontarPontos(penalidadeColisao);
    }

    private void CriarDetectorCenarioSeNecessario()
    {
        // Detector trigger auxiliar para contatos suaves com cenário.
        Transform existente = transform.Find("DetectorColisaoCenario");
        if (existente != null)
        {
            SphereCollider detectorExistente = existente.GetComponent<SphereCollider>();
            if (detectorExistente != null) detectorExistente.isTrigger = true;
            return;
        }

        GameObject detector = new GameObject("DetectorColisaoCenario");
        detector.transform.SetParent(transform);
        detector.transform.localRotation = Quaternion.identity;
        detector.transform.localPosition = new Vector3(0f, alturaDetectorCenario, 0f);
        detector.transform.localScale = Vector3.one;

        SphereCollider novoDetector = detector.AddComponent<SphereCollider>();
        novoDetector.isTrigger = true;
        novoDetector.radius = Mathf.Max(0.5f, raioDetectorCenario);
    }

    private System.Collections.IEnumerator RodarFugitivo()
    {
        // Animação simples de giro para mostrar que foi capturado.
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

    private void GarantirComponenteSomBatida()
    {
        // Se o fugitivo não tiver ColisaoSom, cria e tenta copiar config de outro objeto.
        ColisaoSom meuSom = GetComponent<ColisaoSom>();
        if (meuSom != null) return;

        meuSom = gameObject.AddComponent<ColisaoSom>();
        ColisaoSom[] referencias = FindObjectsByType<ColisaoSom>(FindObjectsSortMode.None);
        for (int i = 0; i < referencias.Length; i++)
        {
            ColisaoSom referencia = referencias[i];
            if (referencia == null || referencia == meuSom || referencia.somBatida == null) continue;

            meuSom.somBatida = referencia.somBatida;
            meuSom.volumeBatida = referencia.volumeBatida;
            meuSom.velocidadeMinimaImpacto = referencia.velocidadeMinimaImpacto;
            meuSom.cooldownSom = referencia.cooldownSom;
            meuSom.cooldownMesmoAlvo = referencia.cooldownMesmoAlvo;
            meuSom.ignorarChaoERua = referencia.ignorarChaoERua;
            break;
        }
    }

}
