using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureListItem : MonoBehaviour
{
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public Image image;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }
}
