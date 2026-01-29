using UnityEngine;
using StarterAssets; // Required for the FirstPersonController
using UnityEngine.InputSystem; // Required for the new Input System

public class MiniGameTrigger : MonoBehaviour
{
    [Header("Mini Game Settings")]
    public GameObject miniGameRoot;       // The parent object "MiniGame_Stacking"
    public Camera miniGameCamera;         // The specific "PuzzleCamera"

    [Header("Starter Assets Player")]
    public GameObject playerRoot;          // The "PlayerCapsule" object
    public FirstPersonController controllerScript; // The script that moves the player
    public StarterAssetsInputs inputScript; // The script that locks/unlocks cursor
    public PlayerInput playerInputComponent; // The main Input System component
    public GameObject playerCameraRoot;    // The "MainCamera" inside the player
    public GameObject playerMesh;          // The "Geometry" or body mesh to hide

    private bool isPlayerInZone = false;
    private bool isGameActive = false;

    void Update()
    {
        // Check for "E" key to Interact
        // We check 'wasPressedThisFrame' to ensure it only fires once per click
        if (isPlayerInZone && !isGameActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartMiniGame();
        }

        // Check for "Escape" to Quit
        if (isGameActive && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndMiniGame();
        }
    }

    void StartMiniGame()
    {
        isGameActive = true;

        // 1. Disable Scripts
        controllerScript.enabled = false;
        playerInputComponent.enabled = false;

        // --- NEW: Disable the Player's Hitbox (Collider) ---
        // This stops the player from blocking the mouse clicks
        CharacterController playerHitbox = playerRoot.GetComponent<CharacterController>();
        if (playerHitbox != null) playerHitbox.enabled = false;
        // ---------------------------------------------------

        // 2. Cursor Logic
        inputScript.cursorLocked = false;
        inputScript.cursorInputForLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Cameras & Visuals
        playerCameraRoot.SetActive(false);
        miniGameRoot.SetActive(true);
        miniGameCamera.gameObject.SetActive(true);

        if (playerMesh != null) playerMesh.SetActive(false);
    }

    void EndMiniGame()
    {
        isGameActive = false;

        // 1. Reset Cameras & Visuals
        miniGameRoot.SetActive(false);
        miniGameCamera.gameObject.SetActive(false);
        playerCameraRoot.SetActive(true);

        if (playerMesh != null) playerMesh.SetActive(true);

        // --- NEW: Re-enable the Hitbox ---
        CharacterController playerHitbox = playerRoot.GetComponent<CharacterController>();
        if (playerHitbox != null) playerHitbox.enabled = true;
        // ---------------------------------

        // 2. Reset Inputs
        playerInputComponent.enabled = true;
        controllerScript.enabled = true;

        inputScript.cursorLocked = true;
        inputScript.cursorInputForLook = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ensure your PlayerCapsule has the tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            Debug.Log("Press E to Start Stacking!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}