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

    [Header("Pontuacao")]
    public int penalidadeColisao = 5;
    public float velocidadeMinimaImpacto = 2.5f;
    public float cooldownPenalidade = 0.25f;

    [Header("Tracker do Fugitivo")]
    public bool usarTracker = true;
    public Transform alvoFugitivo;
    public GameObject prefabSetaTracker;
    public float distanciaMinimaTracker = 45f;
    public float distanciaInicialTracker = 10f;
    public float offsetAlturaTracker = 0.12f;

    [Header("Sirene")]
    public AudioClip sirene;
    [Range(0f, 1f)] public float volumeSirene = 0.22f;

    [Header("Limites de Mapa")]
    public bool limitarMovimentoAoAsfalto = true;
    public LayerMask camadasDeChaoPermitidas = 1 << 3;
    public float alturaRaycastChao = 2f;
    public float distanciaRaycastChao = 8f;
    public float antecipacaoLimite = 1.3f;
    public float recuoAoEncostarNoLimite = 0.25f;

    private Rigidbody rb;
    private AudioSource sourceSirene;
    private Transform setaTrackerInstancia;
    private Collider colliderPolicial;
    private Collider[] collidersPontoBloqueio;
    private float proximaBuscaPontoBloqueioTempo = -1f;
    private bool ativo = true;
    private bool movimentoTravadoPorBloqueio = false;
    private float ultimoImpactoTempo = -999f;
    private float velocidadeAtual = 0f;
    private float volumeSireneAplicado = -1f;
    private Vector3 ultimaPosicaoValidaNoMapa;
    private bool temUltimaPosicaoValidaNoMapa = false;

    private void Start()
    {
        // O policial e dirigido por fisica do Rigidbody.
        // Se houver NavMeshAgent no prefab, ele e desligado para evitar conflitos.
        NavMeshAgent agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PolicialScript: Rigidbody nao encontrado no objeto do policial.");
            enabled = false;
            return;
        }

        colliderPolicial = GetComponent<Collider>();
        if (colliderPolicial == null)
        {
            Debug.LogError("PolicialScript: Collider nao encontrado no objeto do policial.");
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        GarantirComponenteSomBatida();

        ConfigurarETocarSirene();
        AtualizarReferenciaPontoBloqueio(true);
        RegistrarPosicaoValidaNoMapa();

        // O tracker depende do fugitivo e da seta de indicador ja existentes na cena.
        InicializarTrackerSeNecessario();
    }

    private void FixedUpdate()
    {
        // Toda física de movimento roda em FixedUpdate para ficar estável.
        if (!ativo || rb == null) return;
        if (EstaDentroDoPontoBloqueio())
        {
            AcionarBloqueioMovimento();
            return;
        }

        if (movimentoTravadoPorBloqueio)
        {
            // Mantém totalmente parado se entrou em área de bloqueio.
            velocidadeAtual = 0f;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        if (limitarMovimentoAoAsfalto)
            AplicarCorrecaoSeSaiuDoMapa();

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

        if (limitarMovimentoAoAsfalto && Mathf.Abs(velocidadeAlvo) > 0.001f && !MovimentoProjetadoPermitido(velocidadeAlvo))
            velocidadeAlvo = 0f;

        float taxaVelocidade = Mathf.Abs(vertical) > 0.001f
            ? aceleracaoMovimento
            : desaceleracaoSemInput;

        velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, velocidadeAlvo, taxaVelocidade * Time.fixedDeltaTime);

        // Movimento no plano horizontal; eixo Y continua sob responsabilidade da fisica.
        Vector3 novaVelocidadePlano = transform.forward * velocidadeAtual;
        rb.linearVelocity = new Vector3(novaVelocidadePlano.x, rb.linearVelocity.y, novaVelocidadePlano.z);

        if (limitarMovimentoAoAsfalto)
            RegistrarPosicaoValidaNoMapa();

        if (Mathf.Abs(horizontal) > 0.001f)
        {
            // Quanto mais rápido o carro, mais "forte" a curva.
            float intensidadeCurva = Mathf.Clamp01(Mathf.Abs(velocidadeAtual) / Mathf.Max(1f, velocidadeMovimento));
            float direcaoCurva = velocidadeAtual >= 0f ? 1f : -1f;
            float yaw = horizontal * direcaoCurva * velocidadeRotacao * 100f * intensidadeCurva * Time.fixedDeltaTime;
            Quaternion delta = Quaternion.Euler(0f, yaw, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }
    }

    private void Update()
    {
        AtualizarVolumeSireneEmTempoReal();
        AtualizarTracker();
    }

    public void FugitivoEscapou()
    {
        // Fim de partida por derrota: para movimento, sirene e tracker.
        ativo = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        PararSirene();
        OcultarTracker();
    }

    public void FugitivoPego()
    {
        // Fim de partida por vitória: para movimento, sirene e tracker.
        ativo = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;
        PararSirene();
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
        ProcessarColisao(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProcessarColisao(collision);
    }

    private void ProcessarColisao(Collision collision)
    {
        if (!ativo || collision == null) return;

        FugitivoScript fugitivo = collision.collider != null
            ? collision.collider.GetComponentInParent<FugitivoScript>()
            : null;
        if (fugitivo != null)
        {
            FugitivoPego();
            return;
        }

        if (!PodePenalizar(collision)) return;

        // Colisão válida desconta pontos do jogador.
        GameplayPartidaController partida = GameplayPartidaController.Instancia;
        if (partida == null)
            partida = FindFirstObjectByType<GameplayPartidaController>();
        if (partida != null)
            partida.DescontarPontos(penalidadeColisao);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ativo || other == null) return;
        if (movimentoTravadoPorBloqueio) return;
        if (!EhPontoBloqueio(other)) return;
        AcionarBloqueioMovimento();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!ativo || other == null) return;
        if (movimentoTravadoPorBloqueio) return;
        if (!EhPontoBloqueio(other)) return;
        AcionarBloqueioMovimento();
    }

    private bool PodePenalizar(Collision collision)
    {
        if (collision == null) return false;
        if (Time.time - ultimoImpactoTempo < cooldownPenalidade) return false;
        if (collision.relativeVelocity.magnitude < velocidadeMinimaImpacto) return false;

        GameObject outro = collision.gameObject;
        if (outro == null) return false;

        if (outro.GetComponentInParent<PolicialScript>() != null) return false;
        if (outro.GetComponentInParent<FugitivoScript>() != null) return false;
        if (outro.GetComponentInParent<BloqueioPosicionamentoArea>() != null) return false;

        string nome = outro.name.ToLowerInvariant();
        // Ignora superfícies de cenário e bloqueios no sistema de penalidade.
        if (nome.Contains("ground") || nome.Contains("road") || nome.Contains("pista") || nome.Contains("lane") || nome.Contains("tile") || nome.Contains("bloqueio")) return false;

        ultimoImpactoTempo = Time.time;
        return true;
    }

    private bool EhPontoBloqueio(Collider other)
    {
        if (other == null) return false;

        if (collidersPontoBloqueio != null)
        {
            for (int i = 0; i < collidersPontoBloqueio.Length; i++)
            {
                Collider c = collidersPontoBloqueio[i];
                if (c == null) continue;
                if (other == c) return true;
                if (other.transform.IsChildOf(c.transform) || c.transform.IsChildOf(other.transform)) return true;
            }
        }

        Transform t = other.transform;
        if (t == null) return false;

        string nome = t.name.ToLowerInvariant();
        if (nome.Contains("pontobloqueio")) return true;

        if (t.root != null)
        {
            string nomeRaiz = t.root.name.ToLowerInvariant();
            if (nomeRaiz.Contains("pontobloqueio")) return true;
        }

        return false;
    }

    private bool EstaDentroDoPontoBloqueio()
    {
        if (colliderPolicial == null) return false;

        if ((collidersPontoBloqueio == null || collidersPontoBloqueio.Length == 0) && Time.time >= proximaBuscaPontoBloqueioTempo)
            AtualizarReferenciaPontoBloqueio(false);

        if (collidersPontoBloqueio == null || collidersPontoBloqueio.Length == 0) return false;

        for (int i = 0; i < collidersPontoBloqueio.Length; i++)
        {
            Collider bloqueio = collidersPontoBloqueio[i];
            if (bloqueio == null || !bloqueio.enabled) continue;

            Vector3 direcao;
            float distancia;
            bool sobrepoe = Physics.ComputePenetration(
                colliderPolicial, colliderPolicial.transform.position, colliderPolicial.transform.rotation,
                bloqueio, bloqueio.transform.position, bloqueio.transform.rotation,
                out direcao, out distancia);

            // Se sobrepõe ao volume do bloqueio, considera invasão da área proibida.
            if (sobrepoe) return true;
        }

        return false;
    }

    private void AtualizarReferenciaPontoBloqueio(bool imediato)
    {
        if (!imediato && Time.time < proximaBuscaPontoBloqueioTempo) return;

        GameObject pontoBloqueio = GameObject.Find("PontoBloqueio");
        collidersPontoBloqueio = pontoBloqueio != null
            ? pontoBloqueio.GetComponentsInChildren<Collider>(true)
            : null;

        proximaBuscaPontoBloqueioTempo = Time.time + 0.5f;
    }

    private void AcionarBloqueioMovimento()
    {
        // Trava movimento uma única vez para evitar processamento repetido.
        if (movimentoTravadoPorBloqueio) return;

        movimentoTravadoPorBloqueio = true;
        velocidadeAtual = 0f;
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }

    private void AplicarCorrecaoSeSaiuDoMapa()
    {
        if (rb == null) return;
        if (PontoPertenceAoMapaDirigivel(rb.position, out _)) return;
        if (!temUltimaPosicaoValidaNoMapa) return;

        Vector3 direcaoSaida = (rb.position - ultimaPosicaoValidaNoMapa);
        direcaoSaida.y = 0f;
        if (direcaoSaida.sqrMagnitude < 0.0001f)
            direcaoSaida = transform.forward;
        direcaoSaida.Normalize();

        Vector3 destino = ultimaPosicaoValidaNoMapa - direcaoSaida * Mathf.Max(0f, recuoAoEncostarNoLimite);
        destino.y = rb.position.y;

        rb.position = destino;
        velocidadeAtual = 0f;
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    private bool MovimentoProjetadoPermitido(float velocidadeDesejada)
    {
        Vector3 direcao = transform.forward * Mathf.Sign(velocidadeDesejada);
        float distancia = Mathf.Max(0.65f, Mathf.Abs(velocidadeDesejada) * Time.fixedDeltaTime * Mathf.Max(1f, antecipacaoLimite));
        Vector3 pontoProjetado = rb.position + direcao * distancia;
        return PontoPertenceAoMapaDirigivel(pontoProjetado, out _);
    }

    private void RegistrarPosicaoValidaNoMapa()
    {
        if (!limitarMovimentoAoAsfalto) return;
        if (!PontoPertenceAoMapaDirigivel(rb.position, out Vector3 ponto)) return;

        ultimaPosicaoValidaNoMapa = ponto;
        temUltimaPosicaoValidaNoMapa = true;
    }

    private bool PontoPertenceAoMapaDirigivel(Vector3 ponto, out Vector3 pontoNoChao)
    {
        pontoNoChao = ponto;

        Vector3 origem = ponto + Vector3.up * Mathf.Max(0.5f, alturaRaycastChao);
        float distancia = Mathf.Max(2f, distanciaRaycastChao);
        if (!Physics.Raycast(origem, Vector3.down, out RaycastHit hit, distancia, ~0, QueryTriggerInteraction.Ignore))
            return false;

        pontoNoChao = hit.point;
        if (hit.collider == null) return false;

        if (hit.collider.GetComponentInParent<BloqueioPosicionamentoArea>() != null)
            return false;

        int mask = camadasDeChaoPermitidas.value;
        if (mask != 0 && (mask & (1 << hit.collider.gameObject.layer)) != 0)
            return true;

        string nome = hit.collider.name.ToLowerInvariant();
        if (nome.Contains("ground") || nome.Contains("grass") || nome.Contains("grama") || nome.Contains("terrain"))
            return false;

        if (nome.Contains("road") || nome.Contains("rua") || nome.Contains("pista") || nome.Contains("lane") || nome.Contains("asphalt") || nome.Contains("concrete"))
            return true;

        return false;
    }

    private void InicializarTrackerSeNecessario()
    {
        if (!usarTracker) return;

        if (alvoFugitivo == null)
        {
            FugitivoScript fugitivo = FindFirstObjectByType<FugitivoScript>();
            if (fugitivo != null) alvoFugitivo = fugitivo.transform;
        }

        GameObject obj = GameObject.Find("SetaTracker");
        if (obj != null)
            setaTrackerInstancia = obj.transform;

        if (setaTrackerInstancia == null && prefabSetaTracker != null)
        {
            GameObject instancia = Instantiate(prefabSetaTracker);
            instancia.name = "SetaTracker";
            setaTrackerInstancia = instancia.transform;
        }

        OcultarTracker();
    }

    private void AtualizarTracker()
    {
        if (!usarTracker || !ativo || alvoFugitivo == null || setaTrackerInstancia == null)
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
            // Se o fugitivo está perto, o tracker não é necessário.
            OcultarTracker();
            return;
        }

        Vector3 direcao = delta / Mathf.Max(0.001f, distancia);
        Vector3 pos = origem + (direcao * Mathf.Max(0f, distanciaInicialTracker));
        pos.y = CalcularAlturaNoChao(pos) + offsetAlturaTracker;

        setaTrackerInstancia.position = pos;
        setaTrackerInstancia.rotation = Quaternion.LookRotation(direcao, Vector3.up);

        if (!setaTrackerInstancia.gameObject.activeSelf)
            setaTrackerInstancia.gameObject.SetActive(true);
    }

    private void GarantirComponenteSomBatida()
    {
        // Adiciona ColisaoSom se faltar, copiando parâmetros de outro objeto da cena.
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
            meuSom.volumeBatida = referenciaSom.volumeBatida;
            meuSom.velocidadeMinimaImpacto = referenciaSom.velocidadeMinimaImpacto;
            meuSom.cooldownSom = referenciaSom.cooldownSom;
            meuSom.cooldownMesmoAlvo = referenciaSom.cooldownMesmoAlvo;
            meuSom.ignorarChaoERua = referenciaSom.ignorarChaoERua;
        }
    }

    private void ConfigurarETocarSirene()
    {
        if (sirene == null)
        {
            PararSirene();
            return;
        }

        Transform noSirene = transform.Find("SireneAudio");
        if (noSirene == null)
        {
            GameObject go = new GameObject("SireneAudio");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            noSirene = go.transform;
        }

        sourceSirene = noSirene.GetComponent<AudioSource>();
        if (sourceSirene == null)
            sourceSirene = noSirene.gameObject.AddComponent<AudioSource>();

        sourceSirene.playOnAwake = false;
        sourceSirene.loop = true;
        sourceSirene.spatialBlend = 0f;
        sourceSirene.clip = sirene;

        float volumeAtual = Mathf.Clamp01(volumeSirene) * ObterVolumeEfeitosGlobal();
        sourceSirene.volume = volumeAtual;
        volumeSireneAplicado = volumeAtual;

        if (volumeAtual > 0.0001f && !sourceSirene.isPlaying)
            sourceSirene.Play();

        SincronizarOutrasFontesSirene(volumeAtual);
    }

    private void PararSirene()
    {
        if (sourceSirene != null && sourceSirene.isPlaying)
            sourceSirene.Stop();
    }

    private void OnDestroy()
    {
        PararSirene();
    }

    private void AtualizarVolumeSireneEmTempoReal()
    {
        if (sourceSirene == null || sourceSirene.clip == null) return;

        float volumeAtual = Mathf.Clamp01(volumeSirene) * ObterVolumeEfeitosGlobal();
        if (Mathf.Approximately(volumeSireneAplicado, volumeAtual)) return;

        sourceSirene.volume = volumeAtual;
        volumeSireneAplicado = volumeAtual;

        if (volumeAtual <= 0.0001f)
        {
            if (sourceSirene.isPlaying) sourceSirene.Pause();
        }
        else
        {
            if (!sourceSirene.isPlaying) sourceSirene.UnPause();
            if (!sourceSirene.isPlaying) sourceSirene.Play();
        }

        SincronizarOutrasFontesSirene(volumeAtual);
    }

    private void SincronizarOutrasFontesSirene(float volumeAtual)
    {
        // Mantém outras fontes "de sirene" com o mesmo volume do principal.
        AudioSource[] fontes = GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < fontes.Length; i++)
        {
            AudioSource fonte = fontes[i];
            if (fonte == null || fonte == sourceSirene) continue;

            bool pareceSirenePorNome = fonte.gameObject.name.ToLowerInvariant().Contains("sirene")
                || fonte.gameObject.name.ToLowerInvariant().Contains("siren");
            bool usaMesmoClip = sirene != null && fonte.clip == sirene;
            if (!pareceSirenePorNome && !usaMesmoClip) continue;

            fonte.volume = volumeAtual;

            if (volumeAtual <= 0.0001f)
            {
                if (fonte.isPlaying) fonte.Pause();
            }
            else
            {
                if (fonte.loop && !fonte.isPlaying) fonte.UnPause();
            }
        }
    }

    private float ObterVolumeEfeitosGlobal()
    {
        return AudiosScript.ObterVolumeEfeitosGlobal();
    }

    private void OcultarTracker()
    {
        if (setaTrackerInstancia != null && setaTrackerInstancia.gameObject.activeSelf)
            setaTrackerInstancia.gameObject.SetActive(false);
    }

    private float CalcularAlturaNoChao(Vector3 pontoNoMundo)
    {
        // Raycast para colocar tracker exatamente sobre o chão.
        Vector3 origemRay = pontoNoMundo + Vector3.up * 50f;
        if (Physics.Raycast(origemRay, Vector3.down, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return transform.position.y;
    }
}
