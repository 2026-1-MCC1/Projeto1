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
    }

    void Update()
    {
        if (!ativo) return;

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
        policial.isStopped = true;
    }

    public void FugitivoPego()
    {
        ativo = false;
        policial.isStopped = true;
    }
}