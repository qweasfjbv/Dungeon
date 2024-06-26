using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }

    [SerializeField]
    public AudioSource bgmPlayer;
    [SerializeField]
    public AudioSource buttonPlayer;
    [SerializeField]
    public AudioSource dialogPlayer;
    [SerializeField]
    public AudioSource enemyPlayer;
    [SerializeField]
    public AudioSource cardPlayer;
    [SerializeField]
    public AudioSource effectPlayer;
    [SerializeField]
    public AudioSource npcPlayer;


    [SerializeField]
    public AudioClip[] audioClips;
    [SerializeField]
    public AudioClip[] bgmClips;
    [SerializeField]
    public AudioClip[] buttonClips;
    [SerializeField]
    private AudioClip[] dialogClips;
    [SerializeField]
    private AudioClip[] effectClips;
    [SerializeField]
    private AudioClip[] npcClips; 



    private bool isFadeIn = false;
    private bool isFadeOut = false;
    private Coroutine soundFadeIn;
    private Coroutine soundFadeOut;




    private float sfxMaxVolume = 1f;
    private float bgmMaxVolume = 1f;


    private void Init()
    {
        if (instance == null)
        {
            GameObject go = GameObject.Find("@Sound");
            if (go == null)
            {
                go = new GameObject { name = "@Sound" };
                go.AddComponent<SoundManager>();
            }

            DontDestroyOnLoad(go);
            instance = go.GetComponent<SoundManager>();

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
        ChangeBGM(Define.BgmType.Main);
    }

    public void SetBGMVolume(float volume)
    {
        bgmPlayer.volume = volume;
        bgmMaxVolume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        buttonPlayer.volume = volume;
        dialogPlayer.volume = volume;
        enemyPlayer.volume = volume;
        cardPlayer.volume = volume;

        sfxMaxVolume = volume;
    }

    public void PlaySfxSound(Define.SFXSoundType type)
    {
        int idx;
        switch (type)
        {
            case Define.SFXSoundType.Paper:
                idx = 0;
                break;
            case Define.SFXSoundType.Coin:
                idx = 1;
                break;
            case Define.SFXSoundType.Throw:
                idx = 2;
                break;
            case Define.SFXSoundType.Fragile:
                idx = 3;
                break;
            case Define.SFXSoundType.Place:
                idx = 4;
                break;

            default:
                idx = -1;
                break;
        }

        if (idx == -1) return;

        cardPlayer.clip = audioClips[idx];
        cardPlayer.Play();
    }
    public bool IsSfxPlaying()
    {
        return cardPlayer.isPlaying;
    }

    public void PlayButtonSound(Define.ButtonSoundType type)
    {

        int idx;
        switch (type)
        {
            case Define.ButtonSoundType.ClickButton:
                idx = 0;
                break;
            case Define.ButtonSoundType.ShowButton:
                idx = 1;
                break;

            default:
                idx = -1;
                break;
        }

        if (idx == -1) return;

        buttonPlayer.clip = buttonClips[idx];
        buttonPlayer.Play();
    }

    public void ChangeBGM(Define.BgmType type)
    {
        int idx = -1;
        switch (type)
        {
            case Define.BgmType.Main:
                idx = 0;
                break;
            case Define.BgmType.Game:
                idx = 1;
                break;
            case Define.BgmType.NPC1:
                idx = 2;
                break;
            default:
                idx = -1;
                break;
        }
        if (bgmPlayer.clip == bgmClips[idx]) return;

        bgmPlayer.clip = bgmClips[idx];
        StartCoroutine(BGMFadeInCoroutine(bgmPlayer, 3.0f));
    }

    public void PlayWriteSound(Define.DialogSoundType type)
    {
        int idx = 0;
        switch (type)
        {
            case Define.DialogSoundType.LongWrite:
                idx = 0;
                break;
            case Define.DialogSoundType.MediumWrite:
                idx = 1; 
                break;
            case Define.DialogSoundType.ShortWrite:
                idx = 2;
                break;
            case Define.DialogSoundType.SelectChange:
                idx = 3;
                break;
            case Define.DialogSoundType.SelectChoose:
                idx = 4;
                break;
        }

        dialogPlayer.clip = dialogClips[idx];
        dialogPlayer.Play();
    }

    public bool IsWritePlaying()
    {
        return dialogPlayer.isPlaying;
    }


    public void PlayEffectSound(Define.EffectSoundType type)
    {

        int idx;
        switch (type)
        {
            case Define.EffectSoundType.Hit:
                idx = 0;
                break;
            case Define.EffectSoundType.Fire:
                idx = 1;
                break;
            case Define.EffectSoundType.Coin:
                idx = 2;
                break;

            default:
                idx = -1;
                break;
        }

        if (idx == -1) return;

        effectPlayer.clip = effectClips[idx];
        effectPlayer.Play();
    }


    public void PlayNPCSound(Define.NpcSoundType type)
    {
        int idx = 0;
        switch (type)
        {
            case Define.NpcSoundType.Goblin: idx = 0; break;
            case Define.NpcSoundType.Witch: idx = 1; break;
            case Define.NpcSoundType.Subordinate: idx = 2; break;
            case Define.NpcSoundType.Boss: idx = 3; break;
            case Define.NpcSoundType.Merchant: idx = 4;break;
        }

        npcPlayer.clip = npcClips[idx];
        npcPlayer.Play();
    }



    private IEnumerator SoundFadeInCoroutine(AudioSource player, float fadeTime)
    {
        isFadeIn = true;
        player.volume = 0f;
        player.Play();
        while (player.volume < sfxMaxVolume)
        {
            player.volume += sfxMaxVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        isFadeIn = true;
    }

    private IEnumerator BGMFadeInCoroutine(AudioSource player, float fadeTime)
    {
        player.volume = 0f;
        player.Play();
        while (player.volume < bgmMaxVolume)
        {
            player.volume += bgmMaxVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

    }

    private IEnumerator SoundFadeOutCoroutine(AudioSource player, float fadeTime)
    {
        isFadeOut = true;
        player.volume = sfxMaxVolume;

        while (player.volume > 0)
        {
            player.volume -= sfxMaxVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        player.Stop();

        isFadeOut = false;
    }

    

}