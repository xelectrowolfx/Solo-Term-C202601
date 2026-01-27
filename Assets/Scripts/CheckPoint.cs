using System.Collections;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.instance.playerSpawnPos.transform.position != transform.position)
        {

            GameManager.instance.playerSpawnPos.transform.position = transform.position;
             StartCoroutine(feedback());
        
        }
    }

    IEnumerator feedback()
    {
        GameManager.instance.checkpointPopup.SetActive(true);
        yield return new WaitForSeconds(1);
        GameManager.instance.checkpointPopup.SetActive(false);
    }
}
