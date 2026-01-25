using UnityEngine;

public class FootSteps : MonoBehaviour
{
    [SerializeField] GameObject Parent;

    private void OnTriggerEnter(Collider other)
    {
        IFootstep Instagator = Parent.GetComponent<IFootstep>();
        if (other.CompareTag("Ground") && Instagator != null){
            Instagator.FootStepEvent(transform.position);
        }

    }
}
