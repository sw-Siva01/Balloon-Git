using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class HOWTOPLAY : MonoBehaviour
{
    [TextArea(10, 100)]
    [SerializeField] String prerequisites;
    public TMP_Text textElement;
    public float padding = 10f;
    float bulletIndent = 1.6f;
    float leftMargin = 1f;
    public bool isInEditMode;

    void Start()
    {
        FormetText();
    }

    void FormetText()
    {
        textElement.text = Format(prerequisites);
        if (textElement != null)
        {
            float textHeight = textElement.preferredHeight;
            RectTransform rectTransform = textElement.rectTransform;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, textHeight + padding);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInEditMode) return;
        FormetText();
    }

    public string Format(string rawText)
    {
        const char bold = '~';
        const char bullet = '•';
        const char semiBold = '^';
        const char bulletDetail = '#';

        bool isInBullet = false;
        bool isInBold = false;
        bool isInSemiBold = false;
        bool isinbulletDetail = false;

        string _formatedText = $"<margin-left={leftMargin}em>";
        string bulletIndentLeftTag = $"<indent={bulletIndent}em>", bulletindentRightTag = "</indent>";
        //string boldStyleLeft = "<size=40><b><alpha=#FF>", boldStylRight = "</b></size><alpha=#70>";
        string semiBoldStyleLeft = "<size=32><b><alpha=#FF>", semiBoldStyleRight = "</b></size><alpha=#70>";
        string boldStyleLeft = "<size=40><b><alpha=#FF><color=#FB9718>", boldStylRight = "</b></color></size>";
        foreach (char c in rawText)
        {

            if (c == (char)10)
            {
                if (isInBold)
                {
                    _formatedText += boldStylRight;
                    isInBold = false;
                }
                if (isInBullet)
                {
                    _formatedText += bulletindentRightTag;
                    isInBullet = false;
                }
                if (isInSemiBold)
                {
                    _formatedText += semiBoldStyleRight;
                    isInSemiBold = false;
                }
                if (isinbulletDetail)
                {
                    _formatedText += bulletindentRightTag;
                    isinbulletDetail = false;
                }

                _formatedText += c;
            }
            else
            {

                switch (c)
                {
                    case bold:
                        _formatedText += boldStyleLeft;
                        isInBold = true;
                        break;

                    case semiBold:
                        _formatedText += semiBoldStyleLeft;
                        isInSemiBold = true;
                        break;

                    case bullet:
                        _formatedText += "  " + bullet + bulletIndentLeftTag;
                        isInBullet = true;
                        break;

                    case bulletDetail:
                        _formatedText += "  " + bulletIndentLeftTag;
                        isinbulletDetail = true;
                        break;
                    default:
                        _formatedText += c;
                        break;
                }
            }
        }
        if (isInBullet) _formatedText += bulletindentRightTag;
        _formatedText += "</margin>";
        return _formatedText;
    }
}
