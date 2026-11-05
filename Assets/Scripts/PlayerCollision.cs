
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCollision : MonoBehaviour
{
    [Header("References")]
    public GameObject zombie;

    private int hitCount = 0;
    private bool gameOverTriggered = false;

    private CharacterController controller;

    private HashSet<GameObject> hitObstacles =
        new HashSet<GameObject>();

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private GameObject FindObstacleRoot(GameObject touchedObject)
    {
        Transform current = touchedObject.transform;

        while (current != null)
        {
            if (current.CompareTag("Obstacle"))
                return current.gameObject;

            string objectName = current.name.ToLower();

            if (objectName.Contains("barricade") ||
                objectName.Contains("fallenlog") ||
                objectName.Contains("fallen log") ||
                objectName.Contains("log") ||
                objectName.Contains("rock") ||
                objectName.Contains("treeroot") ||
                objectName.Contains("tree roots"))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        return null;
    }

    private void HandleObstacleHit(GameObject touchedObject)
    {
        if (gameOverTriggered || touchedObject == null)
            return;

        if (controller == null || !controller.isGrounded)
            return;

        GameObject obstacleRoot = FindObstacleRoot(touchedObject);

        if (obstacleRoot == null)
            return;

        if (!hitObstacles.Add(obstacleRoot))
            return;

        hitCount++;

        Debug.Log("REAL OBSTACLE HIT: " + hitCount);

        if (hitCount == 1)
        {
            Debug.Log("FIRST HIT! Zombie chase activated.");

            if (zombie != null)
            {
                zombie.SetActive(true);

                ZombieFollow follow =
                    zombie.GetComponent<ZombieFollow>();

                if (follow == null)
                    follow = zombie.GetComponentInChildren<ZombieFollow>();

                if (follow != null)
                    follow.CatchPlayer();
                else
                    Debug.LogWarning("ZombieFollow not found!");
            }

            return;
        }

        if (hitCount >= 2)
        {
            gameOverTriggered = true;

            Debug.Log("SECOND HIT! GAME OVER!");

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            else
                Debug.LogError("GameManager.Instance is NULL!");
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        HandleObstacleHit(hit.gameObject);
    }
}
