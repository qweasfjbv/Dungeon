using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnNextEventButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.RemoveListener(Managers.Game.NextEvent);
        GetComponent<Button>().onClick.AddListener(Managers.Game.NextEvent);

        Managers.Game.OnEventStart -= OnEventStart;
        Managers.Game.OnEventStart += OnEventStart;

        Managers.Game.onEventEnd -= OnEventEnd;
        Managers.Game.onEventEnd += OnEventEnd;
    }


    private void OnEventStart()
    {
        GetComponent<Button>().enabled = false;
    }

    private void OnEventEnd()
    {
        GetComponent<Button>().enabled = true;
    }

}
