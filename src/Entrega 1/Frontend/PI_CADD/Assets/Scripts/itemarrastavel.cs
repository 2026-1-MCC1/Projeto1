using UnityEngine;
using UnityEngine.EventSystems;

public class ItemArrastavel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabDoItem; // objeto real (bola, muro...)

    private GameObject objetoArrastando;
    public Vector3 escalaPadrao = new Vector3(0.5f, 0.5f, 0.5f);
    public void OnBeginDrag(PointerEventData eventData)
    {
        // cria uma "cópia" enquanto arrasta
        objetoArrastando = Instantiate(prefabDoItem);
        objetoArrastando.transform.localScale = escalaPadrao;
        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 pos = hit.point;
            Collider col = objetoArrastando.GetComponent<Collider>();
            if (col != null)
            {
                pos.y = col.bounds.extents.y; // altura correta
            }
            else
            {
                pos.y = 0.5f; // fallback
            }

            objetoArrastando.transform.position = pos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // solta e mantém no mesmo lugar
        objetoArrastando = null;
    }
}