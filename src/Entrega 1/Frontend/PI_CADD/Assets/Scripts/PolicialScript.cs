using UnityEngine;
using UnityEngine.AI;

public class PolicialScript : MonoBehaviour
{
    // Cria uma variavel public que recebe tipo Transform e pega o valor de transform do fugitivo
    public Transform seguirFugitivo;
    NavMeshAgent policial;
    void Start()
    {

    }

    void Update()
    {
        // Coloca como SetDestination a posição do fugitivo
        policial = GetComponent<NavMeshAgent>();
        policial.SetDestination(seguirFugitivo.position);
    }
}
