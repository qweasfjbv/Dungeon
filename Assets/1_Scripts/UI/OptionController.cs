using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionController : MonoBehaviour
{

    [SerializeField] private OptionUI optionUI;


    private void Start()
    {
        Managers.Input.escAction -= OnKeyboard;
        Managers.Input.escAction += OnKeyboard;
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionUI.Toggle()) // ON
            {
                SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ClickButton);
                Managers.Input.EscBlock = true;
                GetComponent<BlockPanelController>().OnBlock();
            }
            else // OFF
            {
                SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ShowButton);
                Managers.Input.EscBlock = false;
                GetComponent<BlockPanelController>().OffBlock();
            }
        }
    }

}
