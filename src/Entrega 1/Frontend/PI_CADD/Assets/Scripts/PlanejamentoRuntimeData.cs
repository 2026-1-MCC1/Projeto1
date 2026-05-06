using System.Collections.Generic;
using UnityEngine;

public static class PlanejamentoRuntimeData
{
    public struct ItemPlanejado
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    private static readonly List<ItemPlanejado> itensPlanejados = new List<ItemPlanejado>();

    public static IReadOnlyList<ItemPlanejado> ItensPlanejados => itensPlanejados;
    public static bool TemItensPlanejados => itensPlanejados.Count > 0;

    public static void LimparPlano()
    {
        itensPlanejados.Clear();
    }

    public static void RegistrarItem(GameObject prefab, Transform transformDoItem)
    {
        if (prefab == null || transformDoItem == null) return;

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
