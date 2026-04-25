using UnityEngine;
using UnityEngine.AI;

public class FugitivoScript : MonoBehaviour
{
    [Header("Navegação")]
    public Transform pontoFuga;                    // Destino final de fuga
    public float distanciaParaEscapar = 1.5f;      // Distância para considerar que escapou

    [Header("Captura")]
    public float distanciaParaSerPego = 3f;        // Distância do policial para ser capturado
    public Transform policial;                     // Referência ao policial — arrasta no Inspector

    private NavMeshAgent fugitivo;
    private bool foiPego = false;
    private bool escapou = false;

    void Start()
    {
        fugitivo = GetComponent<NavMeshAgent>();
        fugitivo.SetDestination(pontoFuga.position);
    }

    void Update()
    {
        if (foiPego || escapou) return;

        VerificarCaptura();
        VerificarFuga();
    }

    void VerificarCaptura()
    {
        if (policial == null) return;

        float distancia = Vector3.Distance(transform.position, policial.position);

        if (distancia <= distanciaParaSerPego)
        {
            foiPego = true;
            fugitivo.isStopped = true;
            StartCoroutine(RodarFugitivo());

            PolicialScript ps = policial.GetComponent<PolicialScript>();
            if (ps != null) ps.FugitivoPego();
        }
    }

    void VerificarFuga()
    {
        float distanciaAoPonto = Vector3.Distance(transform.position, pontoFuga.position);

        if (distanciaAoPonto <= distanciaParaEscapar)
        {
            escapou = true;
            Debug.Log("Fugitivo escapou!");

            PolicialScript ps = policial.GetComponent<PolicialScript>();
            if (ps != null) ps.FugitivoEscapou();
        }
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
}