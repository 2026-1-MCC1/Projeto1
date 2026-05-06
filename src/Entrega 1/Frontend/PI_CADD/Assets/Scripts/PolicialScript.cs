using UnityEngine;
using UnityEngine.AI;

public class PolicialScript : MonoBehaviour
{
    [Header("Navegação")]
    public Transform seguirFugitivo;               // Referência ao transform do fugitivo

    [Header("Ponto de Bloqueio")]
    public Transform pontoBloqueio;                // Arrasta o objeto de bloqueio aqui no Inspector
    public float distanciaBloqueio = 3f;           // Distância para o policial parar no ponto

    private NavMeshAgent policial;
    private bool ativo = true;
    private bool bloqueado = false;                // Controla se o policial foi bloqueado

    void Start()
    {
        policial = GetComponent<NavMeshAgent>();

        if (policial == null)
        {
            Debug.LogError("PolicialScript: NavMeshAgent não encontrado no objeto do policial.");
            enabled = false;
            return;
        }

        if (seguirFugitivo == null)
        {
            Debug.LogError("PolicialScript: seguirFugitivo não foi atribuído no Inspector.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!ativo || policial == null || seguirFugitivo == null) return;

        VerificarBloqueio();                       // Verifica se passou pelo ponto de bloqueio

        if (!bloqueado)                            // Só persegue se não estiver bloqueado
            policial.SetDestination(seguirFugitivo.position);
    }

    void VerificarBloqueio()
    {
        if (pontoBloqueio == null) return;

        float distancia = Vector3.Distance(transform.position, pontoBloqueio.position);

        if (distancia <= distanciaBloqueio)        // Se chegou perto do ponto de bloqueio
        {
            bloqueado = true;
            policial.isStopped = true;             // Para o policial
            Debug.Log("Policial bloqueado — fugitivo pode escapar!");
        }
    }

    public void FugitivoEscapou()
    {
        ativo = false;
        if (policial != null) policial.isStopped = true;
    }

    public void FugitivoPego()
    {
        ativo = false;
        if (policial != null) policial.isStopped = true;
    }
}
