using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
    // --- INSTANCES ---        
    private CameraController cameraController;

    [Header("BACKGROUND MUSIC")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;  
    private AudioSource audioSourceCamera;        


    // --- UNITY METHODS ---
    protected override void Awake()
    {
        base.Awake();        
    }

    void OnEnable()
    {        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    // --- MY METHODS ---
    // Play mainMenu/gameplay music whenever a new scene is loaded.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cameraController = CameraController.Instance;        
        audioSourceCamera = cameraController.GetComponent<AudioSource>();

        if (scene.buildIndex == 0)
        {
            PlayBackgroundMusic(mainMenuMusic);
        }
        else
        {
            PlayBackgroundMusic(gameplayMusic);
        }
    }

    // Play background music.
    private void PlayBackgroundMusic(AudioClip audioClip)
    {                
        audioSourceCamera.clip = audioClip;
        audioSourceCamera.Play();
    }
}
