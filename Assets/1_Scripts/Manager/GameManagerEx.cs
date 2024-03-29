using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerEx : MonoBehaviour
{

    [SerializeField] private Image crystalFill;
    [SerializeField] private Image bloodFill;

    static GameManagerEx s_instance;
    public static GameManagerEx Instance { get { return s_instance; } }

    public readonly int MaxCrystal = 10;
    public readonly int MaxBlood = 10;
    private int currentCrystal;
    private int currentBlood;

    private float progressTimer = 0f;

    private float restoreTime = 1f;

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

    private void SetBloodFill(int dest)
    {
        currentBlood = dest;

        bloodFill.fillAmount = (float)dest / MaxBlood;

    }

    private void SetCrysatlFill(int dest)
    {

        currentCrystal = dest;

        crystalFill.fillAmount = (float)dest / MaxCrystal;

    }
    public void GameStart()
    {

        SetCrysatlFill(MaxCrystal);
        SetBloodFill(MaxBlood);

        StartCoroutine(CrystalRestore());
    }

    public bool UseCrystal(int num)
    {   if (currentCrystal >= num)
        {
            SetCrysatlFill(currentCrystal - num);
            return true;
        }

        return false;
    }

    public void RestoreCrystal(int num)
    {
        int destNum = num + currentCrystal;
        if (destNum >= MaxCrystal) destNum = MaxCrystal;
        SetCrysatlFill(destNum); 

    }

    public void RestoreBlood(int num)
    {
        int destNum = num + currentBlood;
        if (destNum >= MaxBlood) destNum = MaxBlood;
        SetBloodFill(destNum);
    }

    public bool UseBlood(int num)
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
            progressTimer += Time.deltaTime;

            if (progressTimer > restoreTime)
            {
                RestoreCrystal(1);
                progressTimer -= restoreTime;
            }

            yield return new WaitForSeconds(Time.deltaTime);

        }
    }

}
