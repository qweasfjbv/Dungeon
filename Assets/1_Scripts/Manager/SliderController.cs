using System.Collections;
using UnityEngine;

public class SliderController : MonoBehaviour
{

    [SerializeField] private SliderMove manaFill;
    [SerializeField] private SliderMove bloodFill;

    static SliderController s_instance;
    public static SliderController Instance { get { return s_instance; } }

    public readonly float MaxMana = 10;
    public readonly float MaxBlood = 10;
    private float currentMana;
    private float currentBlood;

    private float progressTimer = 0f;
    private float restoreSpeed = 1f;

    private void Awake()
    {
        s_instance = this;

        // 게임 시작시로 바꿔야함
        GameStart();
    }

    private void SetBloodFill(float dest)
    {
        currentBlood = dest;

        bloodFill.SetTargetV((float)dest / MaxBlood);

    }

    private void SetManaFill(float dest)
    {

        currentMana = dest;

        manaFill.SetTargetV((float)dest / MaxMana);

    }
    public void GameStart()
    {

        SetManaFill(MaxMana);
        SetBloodFill(MaxBlood);

        StartCoroutine(ManaRestoreCoroutine());
    }

    public bool UseMana(float num)
    {   if (currentMana >= num)
        {
            SetManaFill(currentMana - num);
            return true;
        }

        return false;
    }

    public void RestoreMana(float num)
    {
        float destNum = num + currentMana;
        if (destNum >= MaxMana) destNum = MaxMana;
        SetManaFill(destNum); 

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

    private IEnumerator ManaRestoreCoroutine()
    {
        progressTimer = 0f;

        while (true)
        {
            RestoreMana(Time.deltaTime * restoreSpeed);

            yield return new WaitForSeconds(Time.deltaTime);

        }
    }

}
