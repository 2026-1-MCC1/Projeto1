using UnityEngine;
using UnityEngine.AI;

public class AplicarPlanejamentoNaPerseguicao : MonoBehaviour
{
    [Header("Instanciação")]
    [SerializeField] private Transform parentDosItens;
    [SerializeField] private bool limparPlanoAposInstanciar = true;

    [Header("NavMesh Obstacle")]
    [SerializeField] private bool adicionarNavMeshObstacle = true;
    [SerializeField] private bool obstacleCarve = true;

    private void Awake()
    {
        if (!PlanejamentoRuntimeData.TemItensPlanejados) return;

        foreach (PlanejamentoRuntimeData.ItemPlanejado item in PlanejamentoRuntimeData.ItensPlanejados)
        {
            if (item.prefab == null) continue;

            GameObject instancia = Instantiate(item.prefab, item.position, item.rotation, parentDosItens);
            instancia.transform.localScale = item.scale;

            if (!adicionarNavMeshObstacle) continue;

            NavMeshObstacle obstacle = instancia.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
                obstacle = instancia.AddComponent<NavMeshObstacle>();

            obstacle.carving = obstacleCarve;
        }

        if (limparPlanoAposInstanciar)
            PlanejamentoRuntimeData.LimparPlano();
    }
}
