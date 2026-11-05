
using UnityEngine;

public class ZombieFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float followSpeed = 10f;
    public float normalDistanceBehind = 8f;
    public float catchUpDistance = 2f;
    public float rotationSpeed = 8f;

    [Header("Zombie Audio")]
    public AudioSource footstepsAudio;
    public AudioSource growlAudio;

    private bool catchPlayer = false;

    void Update()
    {
        if (GameManager.Instance != null &&
            (!GameManager.Instance.gameStarted ||
             GameManager.Instance.gameOver))
        {
            StopZombieSounds();
            return;
        }

        if (player == null)
            return;

        // Keep the zombie directly behind the player.
        float distance = catchPlayer
            ? catchUpDistance
            : normalDistanceBehind;

        Vector3 targetPosition =
            player.position - player.forward * distance;

        // Keep the zombie at its existing height.
        targetPosition.y = transform.position.y;

        // Smoothly follow the player.
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Face toward the player.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    public void CatchPlayer()
    {
        if (catchPlayer)
            return;

        catchPlayer = true;

        // Start continuous running footsteps.
        if (footstepsAudio != null)
            footstepsAudio.Play();

        // Play the zombie growl once when the chase begins.
        if (growlAudio != null)
            growlAudio.Play();

        Debug.Log("Zombie chase started with audio!");
    }

    private void StopZombieSounds()
    {
        if (footstepsAudio != null && footstepsAudio.isPlaying)
            footstepsAudio.Stop();

        if (growlAudio != null && growlAudio.isPlaying)
            growlAudio.Stop();
    }
}
