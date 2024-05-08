
public static class Constants
{

    // Layers (int)
    public const int LAYER_ENEMY          = (1 << 8);

    // Tag Names
    public const string TAG_ENEMY         = "Human";
    public const string TAG_MONSTER       = "Monster";
    public const string TAG_DYING         = "Dying";

    // Enemy/Monster Animation Names
    public const string ANIM_PARAM_ATK    = "Attack";
    public const string ANIM_PARAM_WALK   = "Walk";
    public const string ANIM_PARAM_IDLE   = "Idle";
    public const string ANIM_PARAM_DIE    = "Die";
    public const string ANIM_PARAM_DMG    = "Damage";

    // Animation Name for IsName func
    public const string DIE_ANIM_NAME     = "Die Blend Tree";
    public const string ATK_ANIM_NAME     = "Attack Blend Tree";
    public const string THUNDER_ANIM_NAME = "ThunderFall";
    public const string ICECRIS_ANIM_NAME = "IceCrystal";

    // Behavior Tree NodeData Names
    public const string NDATA_ATK         = "atkFlag";
    public const string NDATA_TARGET      = "Target";
    public const string NDATA_PATH        = "pathFlag";
    public const string NDATA_TRACK       = "isTracked";

    // Animation Time
    public const float DMG_ANIM_TIME      = 0.4f;

}
