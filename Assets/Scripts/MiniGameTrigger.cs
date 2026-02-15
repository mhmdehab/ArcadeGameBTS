using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

public class MiniGameTrigger : MonoBehaviour
{
    [Header("Mini Game Settings")]
    public GameObject miniGameRoot;
    public Camera miniGameCamera;

    [Header("Starter Assets Player")]
    public GameObject playerRoot;
    public FirstPersonController controllerScript;
    public StarterAssetsInputs inputScript;
    public PlayerInput playerInputComponent;
    public GameObject playerCameraRoot;
    public GameObject playerMesh;      

    private bool isPlayerInZone = false;
    private bool isGameActive = false;

    void Update()
    {
        if (isPlayerInZone && !isGameActive && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartMiniGame();
        }

        if (isGameActive)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.xKey.wasPressedThisFrame)
            {
                EndMiniGame();
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetRun();
                }
            }
        }
    }

    void StartMiniGame()
    {
        isGameActive = true;

        controllerScript.enabled = false;
        playerInputComponent.enabled = false;

        CharacterController playerHitbox = playerRoot.GetComponent<CharacterController>();
        if (playerHitbox != null) playerHitbox.enabled = false;

        inputScript.cursorLocked = false;
        inputScript.cursorInputForLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerCameraRoot.SetActive(false);
        miniGameRoot.SetActive(true);
        miniGameCamera.gameObject.SetActive(true);

        if (playerMesh != null) playerMesh.SetActive(false);
    }

    void EndMiniGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
        }

        isGameActive = false;

        miniGameRoot.SetActive(false);
        miniGameCamera.gameObject.SetActive(false);
        playerCameraRoot.SetActive(true);

        if (playerMesh != null) playerMesh.SetActive(true);

        CharacterController playerHitbox = playerRoot.GetComponent<CharacterController>();
        if (playerHitbox != null) playerHitbox.enabled = true;

        playerInputComponent.enabled = true;
        controllerScript.enabled = true;

        inputScript.cursorLocked = true;
        inputScript.cursorInputForLook = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
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