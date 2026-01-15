using System.Collections;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DoorController : MonoBehaviour
{
    [SerializeField] Transform Door;
    [SerializeField] float Width;
    [SerializeField] int OpenTime;
    [SerializeField] LayerField Layer;

    Vector3 Closed;
    Vector3 Open;
    Vector3 targetPosition;
    Vector3 Current;

    float doortimer;
       // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Closed = Door.transform.localPosition;
        Open = new Vector3(Closed.x + Width, Closed.y, Closed.z);

    }

    // Update is called once per frame
    void Update()
    {
        if (Current != targetPosition)
        {
            Current = Door.transform.localPosition =
                        Vector3.MoveTowards(Door.transform.localPosition,
                        targetPosition,
                        OpenTime * Time.deltaTime);
        }
        
    }

    void OpenDoor()
    {
        targetPosition = Open;
    }

    void CloseDoor()
    {
        targetPosition = Closed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) { return; }

        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            OpenDoor();
            Debug.Log("Door Opening.");
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) { return; }
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            CloseDoor();
            Debug.Log("Door Closing.");
        }
    }
}
