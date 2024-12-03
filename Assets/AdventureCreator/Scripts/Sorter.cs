using AC;
using UnityEngine;
using UnityEngine.Rendering;

public class Sorter : MonoBehaviour
{
    [SerializeField] private Marker _marker;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var sg = collision.GetComponentInChildren<SortingGroup>();
        var pos = collision.transform.position;

        if (pos.y < _marker.transform.position.y)
            sg.sortingOrder = 1;
        else
            sg.sortingOrder = 0;

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var sg = collision.GetComponentInChildren<SortingGroup>();

        sg.sortingOrder = 0;
    }
}
