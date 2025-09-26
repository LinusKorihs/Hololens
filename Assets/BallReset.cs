using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class BallReset : MonoBehaviour //Old Script before PlaceScript
{
    NearInteractionGrabbable grabbable;
    ObjectManipulator manipulator;

    void Start()
    {
        if (this.GetComponent<BoxCollider>() == null || !this.GetComponent<BoxCollider>().isTrigger || this.GetComponent<BoxCollider>().enabled == false)
        {
            Destroy(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();

            NearInteractionGrabbable grabbable = other.GetComponent<NearInteractionGrabbable>();
            if (grabbable != null) grabbable.enabled = false;

            ObjectManipulator manipulator = other.GetComponent<ObjectManipulator>();
            if (manipulator != null)
            {
                manipulator.enabled = false;
            }

            other.transform.position = gameManager.ballSpawnPoint.position + new Vector3(0.05f, 0.05f, 0.3f);
            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            Debug.Log("Ball reset to spawn point.");

            grabbable.enabled = true;
            manipulator.enabled = true;
        }
    }
}