using System.Collections.Generic;
using UnityEngine;

public class SectionController : MonoBehaviour
{
    // --- INSTANCES ---
    private LevelManager levelManager; 
    private GameProgressController gameProgressController;
    private CameraController cameraController;

    [Header("PLATFORMS")]
    [SerializeField] private List<GameObject> platforms;
    [SerializeField] private GameObject platformPrefab;        
    [SerializeField] private GameObject startPlatformPrefab;    
    private GameObject startPlatformInstance;
    private const int k_MaxPlatforms = 10;    
        
    // --- BACKGROUND ---
    private SpriteRenderer bgSpriteRenderer;        
    private int themeID = 0;


    // --- UNITY METHODS ---
    void Awake()
    {
        bgSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();                        
        platforms = new List<GameObject>(k_MaxPlatforms);
    }
    
    void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        gameProgressController = GameProgressController.Instance;
        cameraController = CameraController.Instance;

        InstantiatePlatforms();
        SpawnPlatforms();
        levelManager.IncrementSpawnedSectionsCount();
    }
    
    void Update()
    {
        if (!gameProgressController.IsTimerActive)
        {
            ScrollDown();

            if (transform.position.y <= -levelManager.SectionHeight)
            {
                MoveToTop();            
            }
        }        
    }
    

    // --- MY METHODS --- 
    // Check if theme is changed.
    private bool IsThemeChanged()
    {
        if (levelManager.SpawnedSectionsCount % LevelManager.k_MaxSectionsInTheme == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Instantiate platforms.
    private void InstantiatePlatforms()
    {                       
        for (int i = 0; i < k_MaxPlatforms; i++)
        {
            GameObject platformInstance = Instantiate(platformPrefab, Vector3.zero, Quaternion.identity);
            platformInstance.transform.SetParent(gameObject.transform);                        
            platformInstance.SetActive(false); 
            platforms.Add(platformInstance);            
        }

        startPlatformInstance = Instantiate(startPlatformPrefab, Vector3.zero, Quaternion.identity);
        startPlatformInstance.transform.SetParent(gameObject.transform);                        
        startPlatformInstance.SetActive(false);         
    }

    // Spawn platforms with the fixed vertical gap and random horizontal position.
    private void SpawnPlatforms()
    {
        Sprite platformSprite = levelManager.PlatformSprites[themeID];

        // Calculate platform position.
        float platformWidth = platformPrefab.GetComponent<BoxCollider2D>().size.x;
        float platformHeight = platformPrefab.GetComponent<BoxCollider2D>().size.y;
        
        float leftBound = cameraController.LeftBound + platformWidth;
        float rightBound = cameraController.RightBound - platformWidth;
        float bottomBound = cameraController.BottomBound + platformHeight / 3;        

        float xPosition;
        float yPosition = transform.position.y + bottomBound;
        float zPosition = 0f;
        float yGap = levelManager.SectionHeight / k_MaxPlatforms;
        float startPlatformXBound = 0.00001f;
        
        if (IsThemeChanged())
        {                        
            SpriteRenderer[] spriteRenderers = startPlatformInstance.GetComponentsInChildren<SpriteRenderer>();
            
            foreach (SpriteRenderer item in spriteRenderers)
            {
                item.sprite = platformSprite;
            }

            xPosition = Random.Range(-startPlatformXBound, startPlatformXBound);
            startPlatformInstance.transform.position = new Vector3(xPosition, yPosition, zPosition);
            startPlatformInstance.SetActive(true);
        }
        else
        {
            startPlatformInstance.SetActive(false);
        }
        
        for (int i = 0; i < k_MaxPlatforms; i++)
        {                        
            if (i == 0 && IsThemeChanged())
            {
                platforms[i].SetActive(false);
            }
            else
            {                                
                SpriteRenderer platformSpriteRenderer = platforms[i].GetComponent<SpriteRenderer>();
                platformSpriteRenderer.sprite = platformSprite;
                
                xPosition = Random.Range(leftBound, rightBound);
                platforms[i].transform.position = new Vector3(xPosition, yPosition, zPosition);
                platforms[i].SetActive(true);
            }                        
            
            yPosition += yGap;                        
        }
        
        RescalePlatforms();
    }  

    // Shrink platform width to increase game difficulty within the current theme.
    private void RescalePlatforms()
    {
        float originalScaleX = platformPrefab.transform.localScale.x;
        float scaleX;
        float scaleMultiplier = 0.3f;

        if (IsThemeChanged())
        {
            scaleX = originalScaleX;
        }
        else
        {
            scaleX = Mathf.Max(originalScaleX - (levelManager.SpawnedSectionsCount % LevelManager.k_MaxSectionsInTheme * scaleMultiplier), 1);
        }

        foreach (GameObject item in platforms)
        {
            if (item.transform.childCount == 0)
            {
                item.transform.localScale = new Vector3(scaleX, item.transform.localScale.y, item.transform.localScale.z);
            }            
        }        
    }

    // Move the section downward based on the game's current scroll speed.
    private void ScrollDown()
    {
        transform.position += levelManager.Speed * Time.deltaTime * Vector3.down;
    }

    // Move current section to the top of the highest section, update theme, and respawn platforms.
    private void MoveToTop()
    {
        transform.position = levelManager.TopSectionPosition * (LevelManager.k_MaxSectionsOnScreen - 1);
        levelManager.IncrementSpawnedSectionsCount();
        themeID = GetThemeID();
        bgSpriteRenderer.sprite = levelManager.BackgroundSprites[themeID];       
        SpawnPlatforms();      
    }

    // Calculate the current themeID based on the number of spawned sections.
    private int GetThemeID()
    {
        int minThemeID = 0;
        int maxThemeID = levelManager.BackgroundSprites.Length - 1;
        
        return Mathf.Clamp(levelManager.SpawnedSectionsCount / LevelManager.k_MaxSectionsInTheme, minThemeID, maxThemeID);
    }
}
