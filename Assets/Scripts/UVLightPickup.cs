using UnityEngine;

public class UVLightPickup : MonoBehaviour
{
    [Header("Pickup Sound")]
    public AudioClip pickupClip;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null ||
            !GameManager.Instance.gameStarted ||
            GameManager.Instance.gameOver)
            return;

        if (UVInventory.Instance == null)
        {
            Debug.LogError("UVInventory is missing!");
            return;
        }

        UVInventory.Instance.AddUV();

        // Play the clip independently of this pickup object.
        if (pickupClip != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupClip,
                other.transform.position,
                0.8f
            );
        }

        Debug.Log("UV LIGHT COLLECTED!");
        Destroy(gameObject);
    }
}