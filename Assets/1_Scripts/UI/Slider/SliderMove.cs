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

        sliderFill.fillAmount = currentV;
    }

}
