using UnityEngine;

public class PlatformController : MonoBehaviour
{
    // --- INSTANCES ---
    private GameProgressController gameProgressController;
    private PlayerController playerController;

    // --- POSITIONS ---
    private Vector3 originalPosition;

    // --- BOOL VARIABLES ---
    private bool isScoreUpdated = false;


    // --- UNITY METHODS ---    
    void Start()
    {
        gameProgressController = GameProgressController.Instance;
        playerController = PlayerController.Instance;

        originalPosition = transform.position;

        // If player is standing on the platform at the start of the game, act like the score is already updated.
        if (playerController.transform.position.y > transform.position.y)
        {
            isScoreUpdated = true;
        }
    }

    void Update()
    {        
        // If platform is respawned, act like the score wasn't updated.
        if (originalPosition.x != transform.position.x)
        {
            originalPosition = transform.position;
            isScoreUpdated = false;
        }

        // Update score when player gets above the platform.
        if (playerController.transform.position.y > transform.position.y && !isScoreUpdated)
        {
            gameProgressController.UpdateScore();
            isScoreUpdated = true;
        }
    }
}
