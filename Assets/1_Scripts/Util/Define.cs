using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum GridType { 
        None = 0,
        MainRoom,
        HallWay
    }

    public enum CardType
    {
        Summon = 0,
        Magic,
        None
    }

    public enum ActorID { 
        Unknown = 0,
        GoldenGoblin,
        Witch,

    }

    public enum BuffType
    {
        Atk,
        Spd,
        Def
    }

    public enum SFXSoundType
    {
        Paper,
        Coin,
        Throw,
        Fragile,
        Place,
    }

    public enum BgmType
    {
        Main,
        Game,
        NPC1
    }

    public enum ButtonSoundType
    {
        ClickButton,
        ShowButton
    }
    
    public enum DialogSoundType
    {
        LongWrite,
        MediumWrite,
        ShortWrite,
        SelectChange,
        SelectChoose
    }

    public enum EffectSoundType
    {
        Hit,
        Fire,

    }

    public enum NpcSoundType
    {
        Goblin = 0,
        Witch
    }

    public enum AtkType
    {
        Slash = 0,
        Bow,
        Ice,
        Thunder,
        Blood
    }
}
