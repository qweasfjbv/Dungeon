using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerEx : MonoBehaviour
{

    [SerializeField] private SliderMove crystalFill;
    [SerializeField] private SliderMove bloodFill;

    static GameManagerEx s_instance;
    public static GameManagerEx Instance { get { return s_instance; } }

    public readonly float MaxCrystal = 10;
    public readonly float MaxBlood = 10;
    private float currentCrystal;
    private float currentBlood;

    private float progressTimer = 0f;
    private float restoreSpeed = 1f;

    public void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManagerEx>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<GameManagerEx>();

        }
        else
        {
            Destroy(this.gameObject);
            return;
        }



    }

    private void Awake()
    {
        Init();


        // 게임 시작시로 바꿔야함
        GameStart();
    }

    private void SetBloodFill(float dest)
    {
        currentBlood = dest;

        bloodFill.SetTargetV((float)dest / MaxBlood);

    }

    private void SetCrysatlFill(float dest)
    {

        currentCrystal = dest;

        crystalFill.SetTargetV((float)dest / MaxCrystal);

    }
    public void GameStart()
    {

        SetCrysatlFill(MaxCrystal);
        SetBloodFill(MaxBlood);

        StartCoroutine(CrystalRestore());
    }

    public bool UseCrystal(float num)
    {   if (currentCrystal >= num)
        {
            SetCrysatlFill(currentCrystal - num);
            return true;
        }

        return false;
    }

    public void RestoreCrystal(float num)
    {
        float destNum = num + currentCrystal;
        if (destNum >= MaxCrystal) destNum = MaxCrystal;
        SetCrysatlFill(destNum); 

    }

    public void RestoreBlood(float num)
    {
        float destNum = num + currentBlood;
        if (destNum >= MaxBlood) destNum = MaxBlood;
        SetBloodFill(destNum);
    }

    public bool UseBlood(float num)
    {
        if (currentBlood >= num)
        {
            SetBloodFill(currentBlood - num);
            return true;
        }

        return false;
    }

    private IEnumerator CrystalRestore()
    {
        progressTimer = 0f;

        while (true)
        {
            RestoreCrystal(Time.deltaTime * restoreSpeed);

            yield return new WaitForSeconds(Time.deltaTime);

        }
    }

}
