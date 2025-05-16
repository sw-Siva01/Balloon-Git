using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
public class CursorManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isInputField;
    public TMP_InputField inputField;
    public Button button;
    public Toggle toggle;
    void Start()
    {
        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();
        if (isInputField)
        {
            inputField = GetComponent<TMP_InputField>();
        }
    }
    private void OnDisable()
    {
        // if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null &&
        // !EventSystem.current.currentSelectedGameObject.GetComponent<CursorManager>())
        // {
        if (CursorController.CurrentCursorObj == this)
            CursorController.OnCursorStateChange?.Invoke(false, null);
    }
    public async void OnPointerEnter(PointerEventData eventData)
    {
        await UniTask.DelayFrame(1);
        CursorController.OnCursorStateChange?.Invoke(true, this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CursorController.OnCursorStateChange?.Invoke(false, null);
    }
}