using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
public class CursorController : MonoBehaviour
{
    bool isMobile = false;
    Texture2D handCursor;
    Texture2D defaultCursor;
    Texture2D inputCursor;
    Vector2 hotspot = new Vector2(0, 0);
    Vector2 handhotspot = new Vector2(5, 3);
    Texture2D CurrentCursor;
    float scaleFactor;
    private bool HighlightCursor = false;
    public static CursorManager CurrentCursorObj = null;
    public static UnityAction<bool, CursorManager> OnCursorStateChange;
#if UNITY_WEBGL && !UNITY_EDITOR
       [DllImport("__Internal")]
    private static extern void RegisterMouseEvents();
        [DllImport("__Internal")]
    private static extern int detectInputDevice();
#endif
    public void OnMouseLeaveCanvas()
    {
        Cursor.visible = false;
    }
    public void OnMouseEnterCanvas()
    {
        Cursor.visible = true;
    }
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
     RegisterMouseEvents();
     isMobile = detectInputDevice() == 0;
#endif
        OnCursorStateChange += (onCursorHL, cursorStateManager) =>
        {
            HighlightCursor = onCursorHL;
            CurrentCursorObj = cursorStateManager;
        };
        SetScaleFactor();
        LoadCursorsFormResources();
        if (handCursor == null || defaultCursor == null)
        {
            DebugHelper.LogError("Cursor textures not found. Please ensure they are named correctly and placed in the Resources folder.");
            return;
        }
        Cursor.visible = false;
        Invoke(nameof(SetDefaultCursor), 1f);
    }
    void SetDefaultCursor()
    {
        SetCursorToDefault();
        Cursor.visible = true;
    }
    void LateUpdate()
    {
        if (CurrentCursorObj != null && CurrentCursorObj.gameObject.activeInHierarchy && HighlightCursor)
        {
            if (CurrentCursorObj.isInputField && CurrentCursorObj.inputField != null)
            {
                if (CurrentCursorObj.inputField.interactable && CurrentCursor != inputCursor)
                {
                    SetCursorToInput();
                }
                else if (!CurrentCursorObj.inputField.interactable && CurrentCursor != defaultCursor)
                {
                    SetCursorToDefault();
                }
            }
            else if (CurrentCursorObj.button != null)
            {
                if (CurrentCursorObj.button.interactable && CurrentCursor != handCursor)
                {
                    SetCursorToHand();
                }
                else if (!CurrentCursorObj.button.interactable && CurrentCursor != defaultCursor)
                {
                    SetCursorToDefault();
                }
            }
            else if (CurrentCursorObj.toggle != null)
            {
                if (CurrentCursorObj.toggle.interactable && CurrentCursor != handCursor)
                {
                    SetCursorToHand();
                }
                else if (!CurrentCursorObj.toggle.interactable && CurrentCursor != defaultCursor)
                {
                    SetCursorToDefault();
                }
            }
            else
            {
                SetCursorToDefault();
            }
        }
        if (!HighlightCursor)
        {
            SetCursorToDefault();
        }
    }
    bool IsMobile()
    {
        return isMobile;
    }
    void SetScaleFactor()
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        float dpi = Screen.dpi;
        DebugHelper.Log($"Screen Resolution: {screenWidth}x{screenHeight}, DPI: {dpi}");
        scaleFactor = dpi > 0 ? dpi / 140 : 1;
        DebugHelper.Log($"Scale Factor: {scaleFactor}");
    }
    void LoadCursorsFormResources()
    {
        handCursor = Resources.Load<Texture2D>("hand_cursor");
        defaultCursor = Resources.Load<Texture2D>("default_cursor");
        inputCursor = Resources.Load<Texture2D>("input_cursor");
        handCursor = ResizeTexture(handCursor, scaleFactor);
        defaultCursor = ResizeTexture(defaultCursor, scaleFactor);
        inputCursor = ResizeTexture(inputCursor, scaleFactor);
        hotspot *= scaleFactor;
        handhotspot *= scaleFactor;
    }
    private Texture2D ResizeTexture(Texture2D original, float scaleFactor)
    {
        int newWidth = Mathf.CeilToInt(original.width * scaleFactor);
        int newHeight = Mathf.CeilToInt(original.height * scaleFactor);
        // Create a RenderTexture with the new dimensions
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        // Render the original texture onto the RenderTexture
        Graphics.Blit(original, rt);
        // Create a new Texture2D with the resized dimensions
        Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resizedTexture.Apply();
        // Clean up
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return resizedTexture;
    }
    public void SetCursorToDefault()
    {
        if (!IsMobile() && CurrentCursor != defaultCursor)
        {
            CurrentCursor = defaultCursor;
            Cursor.SetCursor(CurrentCursor, hotspot, CursorMode.ForceSoftware);
        }
    }
    public void SetCursorToHand()
    {
        if (!IsMobile() && CurrentCursor != handCursor)
        {
            CurrentCursor = handCursor;
            Cursor.SetCursor(CurrentCursor, handhotspot, CursorMode.ForceSoftware);
        }
    }
    public void SetCursorToInput()
    {
        if (!IsMobile() && CurrentCursor != inputCursor)
        {
            CurrentCursor = inputCursor;
            Cursor.SetCursor(CurrentCursor, hotspot, CursorMode.ForceSoftware);
        }
    }
    private bool IsValidCursorTexture(Texture2D texture)
    {
        Debug.Log($"{texture.name} - Readable: {texture.isReadable}, Format: {texture.format}, Mipmap Count: {texture.mipmapCount}");
        if (texture == null)
        {
            Debug.LogError("Cursor texture is null.");
            return false;
        }
        if (!texture.isReadable)
        {
            Debug.LogError($"Cursor texture '{texture.name}' is not readable. Enable 'Read/Write' in the texture import settings.");
            return false;
        }
        if (texture.format != TextureFormat.RGBA32 && texture.format != TextureFormat.ARGB32)
        {
            Debug.LogError($"Cursor texture '{texture.name}' must be in RGBA32 or ARGB32 format. Current format: {texture.format}");
            return false;
        }
        if (texture.mipmapCount > 1)
        {
            Debug.LogError($"Cursor texture '{texture.name}' has mip maps enabled. Disable 'Generate Mip Maps' in the texture import settings.");
            return false;
        }
        return true;
    }
}