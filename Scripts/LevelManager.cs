using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{   
    // --- INSTANCES ---            
    private GameProgressController gameProgressController;
    private CameraController cameraController;
    private PlayerController playerController; 

    // --- CONST VARIABLES - TAGS ---
    public const string k_PlatformTag = "Platform";

    [Header("GLOBAL TIME SCALING")]    
    [SerializeField] private float minTimeScale = 1.2f;
    [SerializeField] private float maxTimeScale = 2.2f;
    [SerializeField] private float timeScaleIncrement = 0.05f;
    [SerializeField] private float currentTimeScale;

    [Header("SPRITES")]
    [SerializeField] private Sprite[] backgroundSprites;
    [SerializeField] private Sprite[] platformSprites;
    public Sprite[] BackgroundSprites => backgroundSprites;
    public Sprite[] PlatformSprites => platformSprites;

    [Header("SECTIONS")]    
    [SerializeField] private List<GameObject> sections;
    [SerializeField] private GameObject sectionPrefab;
    public Vector3 TopSectionPosition { get; private set; } 
    public float SectionHeight { get; private set; }       
    public int SpawnedSectionsCount { get; private set; }        
    public const int k_MaxSectionsInTheme = 3;
    public const int k_MaxSectionsOnScreen = 2;

    [Header("SCROLL SETTINGS")]
    public float Speed { get; private set; }
    [SerializeField] private float speedMultiplier = 1.5f;
    private float originalSpeed;    
    private bool isBoosted = false;    


    // --- UNITY METHODS ---
    void Awake()
    {
        currentTimeScale = minTimeScale; 
        Time.timeScale = currentTimeScale;
        
        Speed = 2f;
        originalSpeed = Speed;
        
        sections = new List<GameObject>(k_MaxSectionsOnScreen);
        SpawnedSectionsCount = 0;
        SpawnSections();        
    }
    
    void Start()
    {
        gameProgressController = GameProgressController.Instance;
        cameraController = CameraController.Instance;
        playerController = PlayerController.Instance;

        gameProgressController.gameObject.SetActive(true);
        gameProgressController.ResetGameProgress();
        gameProgressController.StartTimer();
    }
    
    void Update()
    {
        CalculateScrollSpeed();
    }

    void OnDestroy()
    {        
        Time.timeScale = minTimeScale;
    }
    

    // --- MY METHODS ---
    // Instantiate and vertically stack the initial game sections.
    private void SpawnSections()
    {
        Vector3 spawnPosition = Vector3.zero;                

        for (int i = 0; i < k_MaxSectionsOnScreen; i++)
        {            
            GameObject sectionInstance = Instantiate(sectionPrefab, spawnPosition, Quaternion.identity);
            sections.Add(sectionInstance);   
            
            SpriteRenderer bgSpriteRenderer = sectionInstance.transform.Find("Background").GetComponent<SpriteRenderer>();
            SectionHeight = bgSpriteRenderer.bounds.size.y; 
            TopSectionPosition = Vector3.up * SectionHeight;
            spawnPosition += TopSectionPosition;
        }                
    }

    // Increment the number of spawned sections.
    public void IncrementSpawnedSectionsCount()
    {
        SpawnedSectionsCount++;
        IncreaseGameSpeed();        
    }

    // Increase the game speed.
    private void IncreaseGameSpeed()
    {        
        if (currentTimeScale < maxTimeScale)
        {
            currentTimeScale = Mathf.Min(currentTimeScale + timeScaleIncrement, maxTimeScale);
            Time.timeScale = currentTimeScale;
        }
    }

    // Calculate scroll speed based on player's position relative to the center of the screen.
    private void CalculateScrollSpeed()
    {
        bool shouldBoost = playerController.transform.position.y > cameraController.transform.position.y;
        
        if (shouldBoost != isBoosted)
        {
            isBoosted = shouldBoost;

            if (isBoosted)
            {
                Speed = originalSpeed * speedMultiplier; 
            }
            else
            {
                Speed = originalSpeed;
            }
                        
            cameraController.CalculateCameraBounds();
        }
    }
}
