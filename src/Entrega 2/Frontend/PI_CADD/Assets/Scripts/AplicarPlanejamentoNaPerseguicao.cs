using UnityEngine;

public class AplicarPlanejamentoNaPerseguicao : MonoBehaviour
{
    [Header("Instanciação")]
    [SerializeField] private Transform parentDosItens;
    [SerializeField] private bool limparPlanoAposInstanciar = true;

    [Header("Gameplay")]
    [SerializeField] private bool ocultarHotbarNaPerseguicao = true;
    [SerializeField] private bool desativarArrasteNaPerseguicao = true;
    [SerializeField] private bool desativarBloqueiosDePlanejamentoNaPerseguicao = true;

    private void Awake()
    {
        // Verifica se o jogador colocou itens na fase de planejamento.
        bool temPlano = PlanejamentoRuntimeData.TemItensPlanejados;

        if (temPlano)
        {
            // Recria cada item planejado na cena de perseguição.
            foreach (PlanejamentoRuntimeData.ItemPlanejado item in PlanejamentoRuntimeData.ItensPlanejados)
            {
                if (item.prefab == null) continue;

                GameObject instancia = Instantiate(item.prefab, item.position, item.rotation, parentDosItens);
                instancia.transform.localScale = item.scale;

                // O prefab já instanciou com seus componentes (Rigidbody, Colliders, NavMeshObstacle)
                // preservando exatamente as configurações feitas pela interface da Unity.
            }
        }

        if (desativarArrasteNaPerseguicao)
        {
            // Na perseguição não queremos criar itens novos arrastando ícones.
            ArrastarItemScript[] itensArrastaveis = FindObjectsByType<ArrastarItemScript>(FindObjectsSortMode.None);
            foreach (ArrastarItemScript itemArrastavel in itensArrastaveis)
                itemArrastavel.enabled = false;
        }

        if (ocultarHotbarNaPerseguicao)
        {
            // Esconde a barra de itens para limpar a interface.
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
        // Desliga os colliders usados só para impedir posicionamento no planejamento.
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
