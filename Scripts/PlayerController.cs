using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : Singleton<PlayerController>
{    
    // --- INSTANCES ---
    private GameProgressController gameProgressController;
    private CameraController cameraController;

    // --- RIGIDBODY2D --- 
    private Rigidbody2D rb;

    // ---- BOXCOLLIDER2D ---
    private BoxCollider2D boxCollider2D;
    private float originalOffsetX;

    // --- POSITIONS ---
    private Vector3 originalPosition;

    [Header("SPRITES")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite[] jumpSprites;
    private SpriteRenderer spriteRenderer;

    [Header("MOVEMENT")]
    public InputActionReference moveInput;
    public InputActionReference jumpInput;    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpSpeed = 4f;
    [SerializeField] private float jumpForce = 10f;
    private float speed;
    private Vector2 moveDirection;
    
    // --- BOOL VARIABLES ---
    private bool isGrounded = true;
    private bool isDead = false;


    // --- UNITY METHODS ---
    protected override void Awake()
    {
        base.Awake();
        
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        
        originalPosition = transform.position;
        originalOffsetX = boxCollider2D.offset.x;
    }

    void OnEnable()
    {        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        gameProgressController = GameProgressController.Instance;
        cameraController = CameraController.Instance;

        StartCoroutine(ChangeSprite());
    }

    void Update()
    {
        if (!gameProgressController.IsTimerActive)
        {
            if (isGrounded)
            {
                speed = moveSpeed;
            }
            else
            {
                speed = jumpSpeed;
            }        
            
            Move();

            if (isGrounded && jumpInput.action.triggered) // Player jumps if it's on the ground and jump button is pressed.
            {
                Jump();
            }

            if (!isDead && transform.position.y < (cameraController.BottomBound - boxCollider2D.bounds.size.y / 2)) // Respawn player if it falls off the map.
            {
                StartCoroutine(RespawnPlayer());
            }
        }               
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collidedGameObject = collision.gameObject;
        ContactPoint2D contactPoint2D = collision.GetContact(0); // Get the first contact point of the collision.

        // --- COLLISION FROM ABOVE ---
        if (contactPoint2D.normal.y > 0.5f) // Player is grounded whenever it is standing on the object.
        {
            isGrounded = true;

            if (collidedGameObject.CompareTag(LevelManager.k_PlatformTag)
                && collidedGameObject.activeInHierarchy) // Stick player to the platform whenever it is standing on one.
            {
                transform.SetParent(collidedGameObject.transform);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        GameObject collidedGameObject = collision.gameObject;

        if (collidedGameObject.CompareTag(LevelManager.k_PlatformTag)
            && gameObject.activeInHierarchy && enabled)
        {
            transform.SetParent(null);
            isGrounded = false;
        }
    }
    

    // --- MY METHODS ---   
    // Set up player's movement whenever a new scene is loaded.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            FreezeMovement();
        }
        else
        {
            UnfreezeMovement();
        }
    }

    // Freeze player's movement.
    private void FreezeMovement()
    {
        transform.position = Vector3.up * 10f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        moveInput.action.Disable();
        jumpInput.action.Disable();
    }

    // Unfreeze player's movement.
    private void UnfreezeMovement()
    {
        transform.position = originalPosition;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        moveInput.action.Enable();
        jumpInput.action.Enable();
    }   

    // Change player's sprite.
    private IEnumerator ChangeSprite()
    {    
        int spriteID = 0;
        float delay;
        bool wasGrounded = isGrounded;
        bool isIdle;        

        while (true)
        {       
            if (Mathf.Approximately(rb.linearVelocityX, 0) && Mathf.Approximately(rb.linearVelocityY, 0))
            {
                isIdle = true;
            }
            else
            {
                isIdle = false;
            }

            if (isGrounded && isIdle)
            {
                spriteRenderer.sprite = idleSprite;
                spriteID = 0;
                wasGrounded = isGrounded;
                yield return null;
            }
            else
            {                
                if (isGrounded != wasGrounded) // Reset the animation if the player just landed or just jumped.
                {
                    spriteID = 0;
                    wasGrounded = isGrounded;
                }

                if (isGrounded)
                {
                    if (spriteID == walkSprites.Length)
                    {
                        spriteID = 0;
                    }
                    
                    spriteRenderer.sprite = walkSprites[spriteID];
                    delay = 0.15f;
                    spriteID++;
                }
                else
                {                    
                    spriteRenderer.sprite = jumpSprites[spriteID];
                    delay = 0.35f;

                    if (spriteID < (jumpSprites.Length - 1))
                    {
                        spriteID++;
                    }
                }
                                
                yield return new WaitForSeconds(delay);
            }
        }        
    } 

    // Handle player's walk movement.
    private void Move()
    {        
        moveDirection = moveInput.action.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(moveDirection.x * speed, rb.linearVelocityY);

        if (moveDirection.x > 0) // Player walks/looks right.
        {
            spriteRenderer.flipX = false;
            boxCollider2D.offset = new Vector2(originalOffsetX, boxCollider2D.offset.y);
        }        
        else if (moveDirection.x < 0) // Player walks/looks left.
        {
            spriteRenderer.flipX = true;
            boxCollider2D.offset = new Vector2(-originalOffsetX, boxCollider2D.offset.y);
        }                

        // Stop player from moving past the left/right border.
        float clampedX = Mathf.Clamp(transform.position.x, cameraController.LeftBound, cameraController.RightBound);
        transform.position = new Vector2(clampedX, transform.position.y);
    }

    // Handle player's jump movement.
    private void Jump()
    {        
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false;        
    }

    // Handle player's respawning.
    private IEnumerator RespawnPlayer()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);        
        
        isDead = true;
        yield return StartCoroutine(gameProgressController.DisplayGameOverScreen());
        isDead = false;            
    }    
}
