using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlockPanelController : MonoBehaviour
{

    [SerializeField] private GameObject blockPanel;
    [SerializeField] private ButtonPivotUI buttonPivot;

    private bool isBlocking = false;


    private void Awake()
    {
        blockPanel.SetActive(false);
    }



    public void OnBlock()
    {
        if (!isBlocking)
        {
            isBlocking = true;
            
            blockPanel.SetActive(true);

            buttonPivot.HideButton();
            GetComponent<SliderController>().Hide();

        }
    }

    public void OffBlock()
    {
        if (isBlocking)
        {
            isBlocking = false;

            buttonPivot.ShowButton();
            GetComponent<SliderController>().Show();
            
            blockPanel.SetActive(false);    
        }
    }


}
