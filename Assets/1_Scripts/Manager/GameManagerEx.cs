using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerEx : MonoBehaviour
{

    [SerializeField] private Slider crystalSlider;
    [SerializeField] private Slider bloodSlider;

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


    public void GameStart()
    {
        crystalSlider.maxValue = MaxCrystal;
        bloodSlider.maxValue = MaxBlood;

        currentCrystal = MaxCrystal;
        currentBlood = MaxBlood;

        crystalSlider.value = currentCrystal;
        bloodSlider.value = currentBlood;

        StartCoroutine(CrystalRestore());
    }

    public bool UseCrystal(int num)
    {   if (currentCrystal >= num)
        {
            currentCrystal -= num;
            crystalSlider.value = currentCrystal;
            return true;
        }

        return false;
    }

    public void RestoreCrystal(int num)
    {
        int destNum = num + currentCrystal;
        if (destNum >= MaxCrystal) destNum = MaxCrystal;

        currentCrystal = destNum;
        crystalSlider.value = currentCrystal;

    }

    public void RestoreBlood(int num)
    {
        int destNum = num + currentBlood;
        if (destNum >= MaxBlood) destNum = MaxBlood;

        currentBlood = destNum;
        bloodSlider.value = currentBlood;
    }

    public bool UseBlood(int num)
    {
        if (currentBlood >= num)
        {
            currentBlood -= num;
            bloodSlider.value = currentBlood;
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
