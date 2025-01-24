using UnityEngine;

public class RemoveAllInside : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }

    public void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }

    public void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Animal")) Destroy(other.gameObject);
    }
}