using UnityEngine;
using UnityEngine.AI;

public class FugitivoScript : MonoBehaviour
{
    public Transform pontoFuga;
    public float distanciaParaEscapar = 1.5f;
    NavMeshAgent fugitivo;

    void Start()
    {
        fugitivo = GetComponent<NavMeshAgent>();   
        fugitivo.SetDestination(pontoFuga.position);     
    }

    void Update()
    {
        
    }
}
