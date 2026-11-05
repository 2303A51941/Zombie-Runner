using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class UVInventory : MonoBehaviour
{
    public static UVInventory Instance;

    [Header("UI")]
    public TMP_Text countText;

    [Header("UV Power")]
    public float boostDuration = 5f;
    public float speedMultiplier = 1.8f;

    [Header("References")]
    public UVLightSpawner uvSpawner;
    public AudioSource nitroSpeedAudio;

    [Header("Inventory")]
    public int uvCount = 0;

    private PlayerMovement playerMovement;

    void Awake()
    {
        Instance = this;

        playerMovement = GetComponent<PlayerMovement>();

        UpdateCountText();

        Debug.Log("UVInventory: PlayerMovement connected!");
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseUV();
        }
    }

    public void AddUV()
    {
        uvCount++;
        UpdateCountText();

        Debug.Log("UV collected! Total: " + uvCount);
    }

    void UseUV()
    {
        if (uvCount <= 0)
            return;

        if (GameManager.Instance == null ||
            !GameManager.Instance.gameStarted ||
            GameManager.Instance.gameOver)
            return;

        if (playerMovement == null)
        {
            Debug.LogError("UVInventory: PlayerMovement is missing!");
            return;
        }

        uvCount--;
        UpdateCountText();

        // Activate speed boost.
        playerMovement.ActivateUVBoost(
            speedMultiplier,
            boostDuration
        );

        // Play Nitro Speed sound.
        if (nitroSpeedAudio != null)
        {
            nitroSpeedAudio.Play();
        }

        if (uvSpawner != null)
        {
            uvSpawner.RequestRespawnAfterUse();
        }

        Debug.Log("UV USED! Speed boost activated.");
    }

    void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = uvCount.ToString();
        }
    }
}