using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnlargeOnMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   
    public float enlargePercent = 0;

    private float defaultWidth;
    private float defaultHeight;

    private RectTransform rectTransform;

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        defaultWidth = rectTransform.rect.width;
        defaultHeight = rectTransform.rect.height;
    }

    public void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultWidth*(1 + (enlargePercent/100.0f)));
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultHeight *(1 + (enlargePercent/100.0f)));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, defaultWidth);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, defaultHeight);
    }
}