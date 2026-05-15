using System.Collections.Generic;
using UnityEngine;

public static class PlanejamentoRuntimeData
{
    // Estrutura simples que guarda "como o item estava" no planejamento.
    public struct ItemPlanejado
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    // Lista compartilhada em memória entre as cenas.
    private static readonly List<ItemPlanejado> itensPlanejados = new List<ItemPlanejado>();

    public static IReadOnlyList<ItemPlanejado> ItensPlanejados => itensPlanejados;
    public static bool TemItensPlanejados => itensPlanejados.Count > 0;

    public static void LimparPlano()
    {
        // Remove todos os itens planejados.
        itensPlanejados.Clear();
    }

    public static void RegistrarItem(GameObject prefab, Transform transformDoItem)
    {
        if (prefab == null || transformDoItem == null) return;

        // Copia os dados do item solto na cena para reconstruir depois.
        ItemPlanejado item = new ItemPlanejado
        {
            prefab = prefab,
            position = transformDoItem.position,
            rotation = transformDoItem.rotation,
            scale = transformDoItem.localScale
        };

        itensPlanejados.Add(item);
    }
}
