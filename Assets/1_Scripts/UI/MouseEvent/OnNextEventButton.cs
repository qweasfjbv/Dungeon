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

        Managers.Game.onEventStart -= OnEventStart;
        Managers.Game.onEventStart += OnEventStart;

        Managers.Game.onEventEnd -= OnEventEnd;
        Managers.Game.onEventEnd += OnEventEnd;
    }


    private void OnEventStart()
    {
        Debug.Log("ENABLED:FALSE");
        GetComponent<Button>().enabled = false;
    }

    private void OnEventEnd()
    {
        Debug.Log("ENABLED:TRUE");
        GetComponent<Button>().enabled = true;
    }

}
