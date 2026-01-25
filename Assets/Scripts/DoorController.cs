using System;
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
    [SerializeField] AudioSource Source;
    [SerializeField] float Delay = 1;

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
    //private void OnValidate()
    //{
    //    if(Closed == Vector3.zero)
    //    {
    //        Closed = new Vector3(Door.transform.localPosition.x, Door.transform.localPosition.y, Door.transform.localPosition.z);
    //        Debug.Log(Closed);
    //    }
        
    //    if (Start_Open){
    //        if (Open == Vector3.zero)
    //        {
    //            Open = Door.transform.localPosition = new Vector3(Closed.x + Width, Closed.y, Closed.z);
    //            Debug.Log(Open);
    //        }
    //    }
    //    else
    //    {
    //        Door.transform.localPosition = Closed;
    //    }
    //}

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
    IEnumerator OpenDelay()
    {
        yield return new WaitForSeconds(Delay);
        OpenDoor();
    }
    IEnumerator CloseDelay()
    {
        yield return new WaitForSeconds(Delay);
        CloseDoor();
    }
    void playSound()
    {
        if (Source != null)
        {
            Source.Play();
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
            playSound();
           StartCoroutine(OpenDelay());
           
            
            //Debug.Log("Door Opening.");
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) { return; }
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            playSound();
            StartCoroutine(CloseDelay());
            
           
            //Debug.Log("Door Closing.");
        }
    }
}
