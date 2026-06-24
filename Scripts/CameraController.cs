using UnityEngine;

[RequireComponent(typeof(Camera))]

public class CameraController : Singleton<CameraController>
{
    // --- CAMERA ---
    public Camera MainCamera { get; private set; }

    // --- BOUNDS ---
    public float LeftBound { get; private set; }
    public float RightBound { get; private set; }
    public float TopBound { get; private set; }
    public float BottomBound { get; private set; }

    
    // --- UNITY METHODS ---
    protected override void Awake()
    {
        base.Awake();

        MainCamera = GetComponent<Camera>();
        CalculateCameraBounds();
    }
    

    // --- MY METHODS ---
    // Calculate positions of left/right/top/bottom camera bounds.
    public void CalculateCameraBounds()
    {                                     
        float verticalSize = MainCamera.orthographicSize; // Distance from the center to top/bottom of the screen.
        float horizontalSize = verticalSize * MainCamera.aspect; // Distance from the center to left/right of the screen.

        // Bounds in world coordinates relative to the center of the MainCamera.
        LeftBound = transform.position.x - horizontalSize;
        RightBound = transform.position.x + horizontalSize;
        TopBound = transform.position.y + verticalSize;
        BottomBound = transform.position.y - verticalSize;           
    }
}
