using UnityEngine;

public class ColisaoSom : MonoBehaviour
{
    public AudioClip somBatida;

    [Header("Ajuste de Disparo")]
    public float velocidadeMinimaImpacto = 2.3f;
    public float cooldownSom = 0.12f;
    public bool ignorarChaoERua = true;

    private Rigidbody rb;
    private float ultimoSomTempo = -999f;
    private Vector3 ultimaPosicao;
    private float velocidadePorDeslocamento;
    private static AudioClip clipGlobalCache;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ultimaPosicao = transform.position;
        if (somBatida != null && clipGlobalCache == null)
            clipGlobalCache = somBatida;
    }

    private void Update()
    {
        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        Vector3 deslocamento = transform.position - ultimaPosicao;
        velocidadePorDeslocamento = deslocamento.magnitude / dt;
        ultimaPosicao = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        if (!PodeTocar(collision.gameObject, collision.relativeVelocity.magnitude)) return;
        TocarSom();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        float velocidadeRb = rb != null ? rb.linearVelocity.magnitude : 0f;
        float velocidade = Mathf.Max(velocidadeRb, velocidadePorDeslocamento);
        if (!PodeTocar(other.gameObject, velocidade)) return;

        TocarSom();
    }

    private bool PodeTocar(GameObject outro, float velocidade)
    {
        if (outro == null) return false;
        if (Time.time - ultimoSomTempo < cooldownSom) return false;
        if (velocidade < velocidadeMinimaImpacto) return false;

        if (outro.GetComponent<ColisaoSom>() != null) return false;
        if (outro.GetComponentInParent<ColisaoSom>() != null) return false;

        if (ignorarChaoERua)
        {
            string nome = outro.name.ToLowerInvariant();
            if (nome.Contains("ground") || nome.Contains("road") || nome.Contains("pista") || nome.Contains("floor")) return false;
        }

        if (EhObjetoBloqueio(outro)) return false;

        return true;
    }

    private bool EhObjetoBloqueio(GameObject obj)
    {
        if (obj == null) return false;
        if (obj.GetComponentInParent<BloqueioPosicionamentoArea>() != null) return true;

        string nome = obj.name.ToLowerInvariant();
        if (nome.Contains("pontobloqueio")) return true;
        if (nome.Contains("areabloqueadacarros")) return true;
        if (nome.Contains("bloqueio")) return true;

        return false;
    }

    private void TocarSom()
    {
        ultimoSomTempo = Time.time;
        AudioClip clip = somBatida != null ? somBatida : clipGlobalCache;
        if (clip == null) return;
        TocarClipNoSistema(clip, transform.position);
    }

    public static void TocarSomBatidaGlobal(Vector3 posicao)
    {
        AudioClip clip = clipGlobalCache;
        if (clip == null)
        {
            ColisaoSom[] sons = FindObjectsByType<ColisaoSom>(FindObjectsSortMode.None);
            for (int i = 0; i < sons.Length; i++)
            {
                if (sons[i] == null || sons[i].somBatida == null) continue;
                clip = sons[i].somBatida;
                clipGlobalCache = clip;
                break;
            }
        }

        if (clip == null) return;
        TocarClipNoSistema(clip, posicao);
    }

    private static void TocarClipNoSistema(AudioClip clip, Vector3 posicao)
    {
        if (clip == null) return;
        if (AudiosScript.instancia != null)
        {
            AudiosScript.instancia.TocarEfeito(clip);
            if (AudiosScript.instancia.volumeEfeitos > 0.01f) return;
        }

        GameObject emissor = new GameObject("SfxBatidaFallback");
        emissor.transform.position = posicao;
        AudioSource source = emissor.AddComponent<AudioSource>();
        source.spatialBlend = 0f; // 2D, evita sumir por distancia
        source.playOnAwake = false;
        source.volume = 0.35f;
        source.clip = clip;
        source.Play();
        Destroy(emissor, clip.length + 0.1f);
    }
}
