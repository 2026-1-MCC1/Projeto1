using UnityEngine;
using UnityEngine.AI;

public class PolicialScript : MonoBehaviour
{
    [Header("Controle do Jogador")]
    public float velocidadeMovimento = 12f;
    public float velocidadeRe = 7f;
    public float aceleracaoMovimento = 24f;
    public float desaceleracaoSemInput = 30f;
    public float velocidadeRotacao = 7f;
    [Range(0.2f, 1f)] public float sensibilidadeInput = 0.6f;
    [Range(0f, 0.3f)] public float zonaMortaInput = 0.08f;

    [Header("Estabilidade")]
    public bool travarMovimentoNoPlano = true;

    [Header("Pontuacao")]
    public int penalidadeColisao = 5;
    public float velocidadeMinimaImpacto = 2.5f;
    public float cooldownPenalidade = 0.25f;

    [Header("Tracker do Fugitivo")]
    public bool usarTrackerPontilhado = true;
    public Transform alvoFugitivo;
    public Transform setaTrackerUnica;
    public float distanciaMinimaTracker = 45f;
    public float distanciaInicialTracker = 10f;
    public float espacamentoTracker = 4f;
    public int maxPontosTracker = 25;
    public float tamanhoPontoTracker = 1.2f;
    public float offsetAlturaTracker = 0.12f;
    public Color corTracker = new Color(1f, 0.95f, 0.2f, 0.85f);

    [Header("Compatibilidade de Bloqueio")]
    public bool ignorarColisaoComBloqueioPlanejamento = true;

    [Header("Som de Batida")]
    public bool garantirSomBatida = true;
    public float velocidadeMinimaSomBatidaFallback = 3f;
    public float cooldownSomBatidaFallback = 0.2f;

    private NavMeshAgent policialAgent;
    private Rigidbody rb;
    private ColisaoSom somColisaoLocal;
    private bool ativo = true;
    private float ultimoImpactoTempo = -999f;
    private float ultimoSomFallbackTempo = -999f;
    private float velocidadeAtual = 0f;

    private void Start()
    {
        policialAgent = GetComponent<NavMeshAgent>();
        if (policialAgent != null)
        {
            policialAgent.isStopped = true;
            policialAgent.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PolicialScript: Rigidbody nao encontrado no objeto do policial.");
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (garantirSomBatida)
            GarantirComponenteSomBatida();
        somColisaoLocal = GetComponent<ColisaoSom>();

        if (ignorarColisaoComBloqueioPlanejamento)
            IgnorarColisoesComBloqueios();

        InicializarTrackerSeNecessario();
    }

    private void FixedUpdate()
    {
        if (!ativo || rb == null) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontal) < zonaMortaInput) horizontal = 0f;
        if (Mathf.Abs(vertical) < zonaMortaInput) vertical = 0f;

        horizontal *= sensibilidadeInput;
        vertical *= sensibilidadeInput;

        float velocidadeAlvo = 0f;
        if (vertical > 0f)
            velocidadeAlvo = vertical * velocidadeMovimento;
        else if (vertical < 0f)
            velocidadeAlvo = vertical * velocidadeRe;

        float taxaVelocidade = Mathf.Abs(vertical) > 0.001f
            ? aceleracaoMovimento
            : desaceleracaoSemInput;

        velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, velocidadeAlvo, taxaVelocidade * Time.fixedDeltaTime);

        Vector3 novaVelocidadePlano = transform.forward * velocidadeAtual;
        rb.linearVelocity = new Vector3(novaVelocidadePlano.x, rb.linearVelocity.y, novaVelocidadePlano.z);

        if (Mathf.Abs(horizontal) > 0.001f)
        {
            float intensidadeCurva = Mathf.Clamp01(Mathf.Abs(velocidadeAtual) / Mathf.Max(1f, velocidadeMovimento));
            float direcaoCurva = velocidadeAtual >= 0f ? 1f : -1f;
            float yaw = horizontal * direcaoCurva * velocidadeRotacao * 100f * intensidadeCurva * Time.fixedDeltaTime;
            Quaternion delta = Quaternion.Euler(0f, yaw, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }
    }

    private void Update()
    {
        AtualizarTracker();
    }

    public void FugitivoEscapou()
    {
        ativo = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        OcultarTracker();
    }

    public void FugitivoPego()
    {
        ativo = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        OcultarTracker();

        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida == null)
            partida = FindFirstObjectByType<GameplayPartidaController>();

        if (partida != null)
            partida.RegistrarCaptura();
        else
            Debug.LogError("PolicialScript: GameplayPartidaController nao encontrado para exibir tela de captura.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!ativo || collision == null) return;

        // Evita som duplicado/spam quando ja existe ColisaoSom no objeto.
        if (somColisaoLocal == null && PodeTocarSomFallback(collision))
            ColisaoSom.TocarSomBatidaGlobal(transform.position);

        if (collision.gameObject.GetComponent<FugitivoScript>() != null)
        {
            FugitivoPego();
            return;
        }

        if (!PodePenalizar(collision)) return;

        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida != null)
            partida.DescontarPontos(penalidadeColisao);
    }

    private bool PodePenalizar(Collision collision)
    {
        if (collision == null) return false;
        if (Time.time - ultimoImpactoTempo < cooldownPenalidade) return false;
        if (collision.relativeVelocity.magnitude < velocidadeMinimaImpacto) return false;

        GameObject outro = collision.gameObject;
        if (outro == null) return false;

        if (outro.GetComponent<PolicialScript>() != null) return false;
        if (outro.GetComponent<FugitivoScript>() != null) return false;

        string nome = outro.name.ToLowerInvariant();
        if (nome.Contains("ground") || nome.Contains("road")) return false;

        ultimoImpactoTempo = Time.time;
        return true;
    }

    private bool PodeTocarSomFallback(Collision collision)
    {
        if (collision == null) return false;
        if (Time.time - ultimoSomFallbackTempo < cooldownSomBatidaFallback) return false;
        if (collision.relativeVelocity.magnitude < velocidadeMinimaSomBatidaFallback) return false;

        GameObject outro = collision.gameObject;
        if (outro == null) return false;

        if (outro.transform.IsChildOf(transform)) return false;

        string nome = outro.name.ToLowerInvariant();
        if (nome.Contains("ground") || nome.Contains("road") || nome.Contains("pista") || nome.Contains("floor")) return false;

        ultimoSomFallbackTempo = Time.time;
        return true;
    }

    private void InicializarTrackerSeNecessario()
    {
        if (!usarTrackerPontilhado) return;

        if (alvoFugitivo == null)
        {
            FugitivoScript fugitivo = FindFirstObjectByType<FugitivoScript>();
            if (fugitivo != null) alvoFugitivo = fugitivo.transform;
        }

        if (setaTrackerUnica == null)
        {
            GameObject obj = GameObject.Find("SetaTrackerUnica");
            if (obj != null) setaTrackerUnica = obj.transform;
        }

        OcultarTracker();
    }

    private void AtualizarTracker()
    {
        if (!usarTrackerPontilhado || !ativo || alvoFugitivo == null || setaTrackerUnica == null)
        {
            OcultarTracker();
            return;
        }

        Vector3 origem = transform.position;
        Vector3 alvo = alvoFugitivo.position;
        Vector3 delta = alvo - origem;
        delta.y = 0f;

        float distancia = delta.magnitude;
        if (distancia < distanciaMinimaTracker)
        {
            OcultarTracker();
            return;
        }

        Vector3 direcao = delta / Mathf.Max(0.001f, distancia);
        Vector3 pos = origem + (direcao * Mathf.Max(0f, distanciaInicialTracker));
        pos.y = CalcularAlturaNoChao(pos) + offsetAlturaTracker;

        setaTrackerUnica.position = pos;
        setaTrackerUnica.rotation = Quaternion.LookRotation(direcao, Vector3.up);

        if (!setaTrackerUnica.gameObject.activeSelf)
            setaTrackerUnica.gameObject.SetActive(true);
    }

    private void IgnorarColisoesComBloqueios()
    {
        Collider[] colsPolicial = GetComponentsInChildren<Collider>(true);
        if (colsPolicial == null || colsPolicial.Length == 0) return;

        Collider[] todos = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        for (int i = 0; i < todos.Length; i++)
        {
            Collider col = todos[i];
            if (col == null) continue;
            if (!EhColliderDeBloqueio(col)) continue;

            for (int p = 0; p < colsPolicial.Length; p++)
            {
                if (colsPolicial[p] == null) continue;
                Physics.IgnoreCollision(colsPolicial[p], col, true);
            }
        }
    }

    private bool EhColliderDeBloqueio(Collider col)
    {
        if (col.GetComponentInParent<BloqueioPosicionamentoArea>() != null) return true;

        string nome = col.gameObject.name.ToLowerInvariant();
        if (nome.Contains("pontobloqueio")) return true;
        if (nome.Contains("areabloqueadacarros")) return true;

        return false;
    }

    private void GarantirComponenteSomBatida()
    {
        ColisaoSom meuSom = GetComponent<ColisaoSom>();
        if (meuSom != null) return;

        meuSom = gameObject.AddComponent<ColisaoSom>();
        ColisaoSom[] referencias = FindObjectsByType<ColisaoSom>(FindObjectsSortMode.None);
        ColisaoSom referenciaSom = null;

        for (int i = 0; i < referencias.Length; i++)
        {
            if (referencias[i] == null) continue;
            if (referencias[i] == meuSom) continue;
            if (referencias[i].somBatida == null) continue;
            referenciaSom = referencias[i];
            break;
        }

        if (referenciaSom != null)
        {
            meuSom.somBatida = referenciaSom.somBatida;
            meuSom.velocidadeMinimaImpacto = referenciaSom.velocidadeMinimaImpacto;
            meuSom.cooldownSom = referenciaSom.cooldownSom;
            meuSom.ignorarChaoERua = referenciaSom.ignorarChaoERua;
        }
    }

    private void OcultarTracker()
    {
        if (setaTrackerUnica != null && setaTrackerUnica.gameObject.activeSelf)
            setaTrackerUnica.gameObject.SetActive(false);
    }

    private float CalcularAlturaNoChao(Vector3 pontoNoMundo)
    {
        Vector3 origemRay = pontoNoMundo + Vector3.up * 50f;
        if (Physics.Raycast(origemRay, Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return transform.position.y;
    }
}
