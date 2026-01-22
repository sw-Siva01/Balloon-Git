using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class HowToPlayHandler : MonoBehaviour
{
    [Header("Dynamic Size Related Components")]
    [SerializeField] private RectTransform PopupImg;
    [SerializeField] private readonly Vector2 PortrailSize = new(1040, 1430);
    [SerializeField] private readonly Vector2 LandscapeSize = new(1650, 1430);
    private Vector2Int LastRes = Vector2Int.zero;

    [Header("TEXT COMPONENTS")]
    [Header("============================")]
    [SerializeField] private Button CloseBtn;
    [SerializeField] private ScrollRect ScrollRect;

    private void Awake()
    {
        CloseBtn.onClick.AddListener(() => { OnClickClose(); });
    }

    private void Update()
    {
        if (PopupImg != null && (Screen.width != LastRes.x || Screen.height != LastRes.y))
        {
            UpdatePopupSize();
            LastRes = new Vector2Int(Screen.width, Screen.height);
        }
    }

    private void UpdatePopupSize()
    {
        float currentAR = CurrentAspectRatio;
        currentAR = currentAR <= 0.5f ? 0.5f : currentAR >= 1f ? 1f : currentAR;
        PopupImg.sizeDelta = Vector2.Lerp(PortrailSize, LandscapeSize, Mathf.InverseLerp(0.5f, 1f, currentAR));
    }

    private float CurrentAspectRatio => (float)Screen.width / Screen.height;

    public void ShowHowToPlay()
    {
        ScrollRect.verticalNormalizedPosition = 1f;
        this.gameObject.SetActive(true);
    }

    private void OnClickClose()
    {
        UI_Controller.instance.PlayButtonSound();
        this.gameObject.SetActive(false);
    }
}