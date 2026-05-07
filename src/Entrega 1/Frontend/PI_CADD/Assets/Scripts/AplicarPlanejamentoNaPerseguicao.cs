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

    [Header("Gameplay")]
    [SerializeField] private bool ocultarHotbarNaPerseguicao = true;
    [SerializeField] private bool desativarArrasteNaPerseguicao = true;
    [SerializeField] private bool desativarBloqueiosDePlanejamentoNaPerseguicao = true;

    private void Awake()
    {
        bool temPlano = PlanejamentoRuntimeData.TemItensPlanejados;

        if (temPlano)
        {
            foreach (PlanejamentoRuntimeData.ItemPlanejado item in PlanejamentoRuntimeData.ItensPlanejados)
            {
                if (item.prefab == null) continue;

                GameObject instancia = Instantiate(item.prefab, item.position, item.rotation, parentDosItens);
                instancia.transform.localScale = item.scale;

                Rigidbody rb = instancia.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                Collider col = instancia.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                if (!adicionarNavMeshObstacle) continue;

                NavMeshObstacle obstacle = instancia.GetComponent<NavMeshObstacle>();
                if (obstacle == null)
                    obstacle = instancia.AddComponent<NavMeshObstacle>();

                obstacle.carving = obstacleCarve;
                obstacle.carveOnlyStationary = true;
            }
        }

        if (desativarArrasteNaPerseguicao)
        {
            ArrastarItemScript[] itensArrastaveis = FindObjectsByType<ArrastarItemScript>(FindObjectsSortMode.None);
            foreach (ArrastarItemScript itemArrastavel in itensArrastaveis)
                itemArrastavel.enabled = false;
        }

        if (ocultarHotbarNaPerseguicao)
        {
            GameObject hotbar = GameObject.Find("Hotbar");
            if (hotbar != null) hotbar.SetActive(false);
        }

        if (desativarBloqueiosDePlanejamentoNaPerseguicao)
            DesativarBloqueiosDePlanejamento();

        if (limparPlanoAposInstanciar)
            PlanejamentoRuntimeData.LimparPlano();
    }

    private void DesativarBloqueiosDePlanejamento()
    {
        BloqueioPosicionamentoArea[] bloqueios = FindObjectsByType<BloqueioPosicionamentoArea>(FindObjectsSortMode.None);
        for (int i = 0; i < bloqueios.Length; i++)
        {
            if (bloqueios[i] == null) continue;
            Collider[] cols = bloqueios[i].GetComponentsInChildren<Collider>(true);
            for (int c = 0; c < cols.Length; c++)
            {
                if (cols[c] != null)
                    cols[c].enabled = false;
            }
        }

        // Mantem o PontoBloqueio ativo na perseguicao.
        // Ele e usado como area de derrota/trava para o policial.
    }
}
