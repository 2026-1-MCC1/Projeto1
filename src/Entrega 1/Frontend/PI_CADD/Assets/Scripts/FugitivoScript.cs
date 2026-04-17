using UnityEngine;
using UnityEngine.AI;

public class FugitivoScript : MonoBehaviour
{
    // Cria uma variavel public que recebe tipo Transform e recebe o valor de transform do object pontoFuga.
    public Transform pontoFuga;
    public float distanciaParaEscapar = 1.5f;
    NavMeshAgent fugitivo;

    void Start()
    {
        // Coloca o pontoFuga.position como destino do fugitivo.
        fugitivo = GetComponent<NavMeshAgent>();   
        fugitivo.SetDestination(pontoFuga.position);     
    }

    void Update()
    {

    }
}
