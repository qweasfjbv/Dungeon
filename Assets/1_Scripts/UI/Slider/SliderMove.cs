using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderMove : MonoBehaviour
{
    [SerializeField] private Image sliderFill;

    private float maxV;

    private float targetV;
    private float currentV;

    private float targetScale = 1f;
    private float currentScale = 1f;

    public void OnSelected()
    {
        targetScale = 1.2f;
    }
    public void OnUnselected()
    {
        targetScale = 0.8f;
    }
    public void OnNothingSelected()
    {
        targetScale = 1.0f;
    }

    public void SetTargetV(float targetV)
    {
        this.targetV = targetV;
    }

    public void SetCurrentV(float currentV)
    {
        this.currentV = currentV;
    }


    private void Update()
    {
        this.currentV = UtilFunctions.ColorAlphaLerp(currentV, targetV, 3f, 0.1f);
        this.currentScale = UtilFunctions.ColorAlphaLerp(currentScale, targetScale, 3f, 0.1f);

        sliderFill.fillAmount = currentV;
        GetComponent<RectTransform>().localScale = new Vector3(currentScale, currentScale, 1f);

    }
}
