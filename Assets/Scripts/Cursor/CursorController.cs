using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class CursorController : MonoBehaviour
{
    static bool isMobile = false;
    Texture2D handCursor;
    Texture2D defaultCursor;
    Texture2D inputCursor;
    Vector2 hotspot = new Vector2(0, 0);
    Vector2 handhotspot = new Vector2(5, 3);
    Texture2D CurrentCursor;
    float scaleFactor;

    string currentWebCursor;

    private bool HighlightCursor = false;
    public static CursorManager CurrentCursorObj = null;
    public static UnityAction<bool, CursorManager> OnCursorStateChange;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterMouseEvents();
    [DllImport("__Internal")]
    private static extern int detectInputDevice();
    [DllImport("__Internal")]
    private static extern void SetWebCursor(string cursorType);
    // [DllImport("__Internal")]
    // private static extern void SetWebCursorVisible(bool visible);
#endif

    public void OnMouseLeaveCanvas()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
       // SetWebCursorVisible(false);
#else
        Cursor.visible = false;
#endif
    }

    public void OnMouseEnterCanvas()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
       // SetWebCursorVisible(true);
        SetWebCursor(currentWebCursor == "hand" ? "pointer" : currentWebCursor == "input" ? "text" : "default");
#else
        Cursor.visible = true;
#endif
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

#if UNITY_WEBGL && !UNITY_EDITOR
       // SetWebCursorVisible(false);
#else
        Cursor.visible = false;
#endif
        Invoke(nameof(SetDefaultCursor), 1f);
    }

    void SetDefaultCursor()
    {
        SetCursorToDefault();
#if UNITY_WEBGL && !UNITY_EDITOR
       // SetWebCursorVisible(true);
#else
        Cursor.visible = true;
#endif
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
        if (!HighlightCursor && CurrentCursor != defaultCursor)
        {
            SetCursorToDefault();
        }
    }

    public static bool IsMobile()
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

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(original, rt);

        Texture2D resizedTexture = new Texture2D(newWidth, newHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resizedTexture;
    }

    public void SetCursorToDefault()
    {
        if (IsMobile()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        if (currentWebCursor == "default") return;
        currentWebCursor = "default";
        CurrentCursor = defaultCursor;
        SetWebCursor("default");
#else
        if (CurrentCursor != defaultCursor)
        {
            CurrentCursor = defaultCursor;
            Cursor.SetCursor(CurrentCursor, hotspot, CursorMode.ForceSoftware);
        }
#endif
    }

    public void SetCursorToHand()
    {
        if (IsMobile()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        if (currentWebCursor == "hand") return;
        currentWebCursor = "hand";
        CurrentCursor = handCursor;
        SetWebCursor("pointer");
#else
        if (CurrentCursor != handCursor)
        {
            CurrentCursor = handCursor;
            Cursor.SetCursor(CurrentCursor, handhotspot, CursorMode.ForceSoftware);
        }
#endif
    }

    public void SetCursorToInput()
    {
        if (IsMobile()) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        if (currentWebCursor == "input") return;
        currentWebCursor = "input";
        CurrentCursor = inputCursor;
        SetWebCursor("text");
#else
        if (CurrentCursor != inputCursor)
        {
            CurrentCursor = inputCursor;
            Cursor.SetCursor(CurrentCursor, hotspot, CursorMode.ForceSoftware);
        }
#endif
    }
}