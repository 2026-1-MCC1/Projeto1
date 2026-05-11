using UnityEngine;
using System.Collections.Generic;

public class ColisaoSom : MonoBehaviour
{
    public AudioClip somBatida;
    [Range(0f, 1f)] public float volumeBatida = 1f;

    [Header("Ajuste de Disparo")]
    public float velocidadeMinimaImpacto = 2.3f;
    public float cooldownSom = 0.12f;
    public float cooldownMesmoAlvo = 0.7f;
    public bool ignorarChaoERua = true;

    private float ultimoSomTempo = -999f;
    private readonly Dictionary<int, float> ultimoSomPorAlvo = new Dictionary<int, float>();
    private static AudioClip clipGlobalCache;

    private void Awake()
    {
        // Guarda um clip de referência para objetos que não receberam clip no inspector.
        if (somBatida != null && clipGlobalCache == null)
            clipGlobalCache = somBatida;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        if (!PodeTocar(collision.gameObject, collision.relativeVelocity.magnitude)) return;
        TocarSom();
    }

    public void TentarTocarPorContato(GameObject outro, float velocidade)
    {
        // Método público para outros scripts pedirem som de batida.
        if (!PodeTocar(outro, velocidade)) return;
        TocarSom();
    }

    private bool PodeTocar(GameObject outro, float velocidade)
    {
        if (outro == null) return false;
        // Cooldown global para não tocar dezenas de sons no mesmo instante.
        if (Time.time - ultimoSomTempo < cooldownSom) return false;
        if (velocidade < velocidadeMinimaImpacto) return false;
        if (outro.transform == transform || outro.transform.IsChildOf(transform) || transform.IsChildOf(outro.transform)) return false;

        if (outro.GetComponent<ColisaoSom>() != null) return false;
        if (outro.GetComponentInParent<ColisaoSom>() != null) return false;

        int idAlvo = outro.GetInstanceID();
        if (ultimoSomPorAlvo.TryGetValue(idAlvo, out float ultimoPorAlvo))
        {
            // Cooldown por alvo para evitar spam com o mesmo objeto.
            if (Time.time - ultimoPorAlvo < cooldownMesmoAlvo) return false;
        }

        if (ignorarChaoERua)
        {
            string nome = outro.name.ToLowerInvariant();
            if (EhSuperficieIgnorada(nome)) return false;
        }

        if (EhObjetoBloqueio(outro)) return false;

        ultimoSomPorAlvo[idAlvo] = Time.time;
        return true;
    }

    private bool EhSuperficieIgnorada(string nome)
    {
        // Filtro por nome para não tocar batida em superfícies grandes de cenário.
        if (string.IsNullOrEmpty(nome)) return false;
        if (nome.Contains("ground")) return true;
        if (nome.Contains("road")) return true;
        if (nome.Contains("pista")) return true;
        if (nome.Contains("floor")) return true;
        if (nome.Contains("lane")) return true;
        if (nome.Contains("tile")) return true;
        if (nome.Contains("sidewalk")) return true;
        if (nome.Contains("asfalto")) return true;
        if (nome.Contains("rua")) return true;
        return false;
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
        TocarClipNoSistema(clip, transform.position, volumeBatida);
    }

    private static void TocarClipNoSistema(AudioClip clip, Vector3 posicao, float volumeBatida)
    {
        if (clip == null) return;
        float volume = Mathf.Clamp01(volumeBatida);

        if (AudiosScript.instancia != null)
        {
            AudiosScript.instancia.TocarEfeito(clip, volume);
            return;
        }

        GameObject emissor = new GameObject("SfxBatidaFallback");
        emissor.transform.position = posicao;
        AudioSource source = emissor.AddComponent<AudioSource>();
        source.spatialBlend = 0f; // 2D, evita sumir por distancia
        source.playOnAwake = false;
        source.volume = 0.35f * volume * AudiosScript.ObterVolumeEfeitosGlobal();
        source.clip = clip;
        source.Play();
        Destroy(emissor, clip.length + 0.1f);
    }
}
