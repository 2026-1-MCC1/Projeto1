using UnityEngine;

public class BloqueioPosicionamentoArea : MonoBehaviour
{
    [SerializeField] private Color corGizmo = new Color(1f, 0.25f, 0.2f, 0.35f);

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = corGizmo;
        Matrix4x4 original = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.matrix = original;
    }
}
