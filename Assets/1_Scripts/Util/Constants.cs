
public static class Constants
{ 

    // Tag Names
    public readonly static string TAG_ENEMY         = "Human";
    public readonly static string TAG_MONSTER       = "Monster";
    public readonly static string TAG_DYING         = "Dying";

    // Layers (int)
    public readonly static int LAYER_ENEMY          = (1 << 8);

    // Enemy/Monster Animation Names
    public readonly static string ANIM_PARAM_ATK    = "Attack";
    public readonly static string ANIM_PARAM_WALK   = "Walk";
    public readonly static string ANIM_PARAM_IDLE   = "Idle";
    public readonly static string ANIM_PARAM_DIE    = "Die";
    public readonly static string ANIM_PARAM_DMG    = "Damage";

    // Animation Name for IsName func
    public readonly static string DIE_ANIM_NAME     = "Die Blend Tree";
    public readonly static string ATK_ANIM_NAME     = "Attack Blend Tree";
    public readonly static string THUNDER_ANIM_NAME = "ThunderFall";
    public readonly static string ICECRIS_ANIM_NAME = "IceCrystal";

    // Behavior Tree NodeData Names
    public readonly static string NDATA_ATK         = "atkFlag";
    public readonly static string NDATA_TARGET      = "Target";
    public readonly static string NDATA_PATH        = "pathFlag";
    public readonly static string NDATA_TRACK       = "isTracked";

}
