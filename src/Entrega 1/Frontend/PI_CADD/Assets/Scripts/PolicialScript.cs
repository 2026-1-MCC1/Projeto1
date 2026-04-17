using UnityEngine;
using UnityEngine.AI;

public class PolicialScript : MonoBehaviour
{
    public Transform seguirFugitivo;
    NavMeshAgent policial;
    void Start()
    {

    }

    void Update()
    {
        policial = GetComponent<NavMeshAgent>();
        policial.SetDestination(seguirFugitivo.position);
    }
}
