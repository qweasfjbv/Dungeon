using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// NE, NW, SE, SW
// Attack 
// Die (No Dir)
// Dmg
// Idle
// Walk
public enum AnimDir { NE = 0, NW, SE, SW}
public enum AnimName { Attack = 0, Die, Dmg, Idle, Walk}


public class SpriteAnimatorEditor : EditorWindow
{
    private string animatorName = "NewAnimator";
    private string storePath;

    private Texture2D[] spriteSheets = new Texture2D[5];
    private AnimationClip[,] animationClips = new AnimationClip[5, 4];

    private int columns = 4;
    private int rows = 4;
    private int widthPx = 32;
    private int heightPx = 32;
    private AnimatorController animatorController;
    private RuntimeAnimatorController overrideController;



    [MenuItem("Window/My Sprite Animator Editor")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAnimatorEditor>("Sprite Animator Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animator Settings", EditorStyles.boldLabel);
        GUILayout.Space(20);

        if (GUILayout.Button("Set Store path"))
        {
            storePath = EditorUtility.OpenFolderPanel("Store path", "", "");
        }
        GUILayout.Label("Store Path : " + storePath);
        GUILayout.Space(20);

        animatorName = EditorGUILayout.TextField("Character Name", animatorName);
        spriteSheets[0] = (Texture2D)EditorGUILayout.ObjectField("Attack Sprite", spriteSheets[0], typeof(Texture2D), false);
        spriteSheets[1] = (Texture2D)EditorGUILayout.ObjectField("Die Sprite", spriteSheets[1], typeof(Texture2D), false);
        spriteSheets[2] = (Texture2D)EditorGUILayout.ObjectField("Dmg Sprite", spriteSheets[2], typeof(Texture2D), false);
        spriteSheets[3] = (Texture2D)EditorGUILayout.ObjectField("Idle Sprite", spriteSheets[3], typeof(Texture2D), false);
        spriteSheets[4] = (Texture2D)EditorGUILayout.ObjectField("Walk Sprite", spriteSheets[4], typeof(Texture2D), false);

        if (GUILayout.Button("Create Animator"))
        {
            CreateAnimator();
        }

    }

    private void CreateAnimator()
    {
        // 예외처리. 시트는 전부 지정되어있어야 합니다.
        for (int i = 0; i < spriteSheets.Length; i++)
        {
            if (spriteSheets[i] == null)
            {
                Debug.LogError("Sprite Sheet is null.");
                return;
            }
        }

        List<Sprite> sprites = SliceSpriteSheet(spriteSheets[0]);

        // 32 x 32 크기로 Slice
        AnimationClip clip = CreateAnimationClip(sprites, animatorName + "_Clip");


    }

    /*
    private void CreateAnimator()
    {
        if (spriteSheet == null)
        {
            Debug.LogError("Sprite Sheet is null.");
            return;
        }

        // Slice the sprite sheet
        List<Sprite> sprites = SliceSpriteSheet(spriteSheet, columns, rows);

        // Create Animation Clips
        AnimationClip clip = CreateAnimationClip(sprites, animatorName + "_Clip");

        // Create Animator Controller
        animatorController = AnimatorController.CreateAnimatorControllerAtPath("Assets/" + animatorName + ".controller");
        AnimatorControllerLayer layer = animatorController.layers[0];
        AnimatorState state = layer.stateMachine.AddState(animatorName);
        state.motion = clip;

        // Create Override Controller
        overrideController = new AnimatorOverrideController();
        //overrideController.runtimeAnimatorController = animatorController;

        // Save the created assets
        AssetDatabase.CreateAsset(clip, "Assets/" + animatorName + "_Clip.anim");
        AssetDatabase.CreateAsset(overrideController, "Assets/" + animatorName + "_Override.overrideController");
        AssetDatabase.SaveAssets();

        Debug.Log("Animator and Override Controller created successfully.");
    }
    */

    private List<Sprite> SliceSpriteSheet(Texture2D texture)
    {
        List<Sprite> sprites = new List<Sprite>();

        int columns = texture.height / widthPx;
        int rows = texture.width / widthPx;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Rect rect = new Rect(x * widthPx, y * heightPx, widthPx, heightPx);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.4f));
                sprites.Add(sprite);
            }

        }

        return sprites;
    }

    private AnimationClip CreateAnimationClip(List<Sprite> sprites, string clipName)
    {
        AnimationClip clip = new AnimationClip();



        return clip;
    }

    /*
    private AnimationClip CreateAnimationClip(List<Sprite> sprites, string clipName)
    {
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe();
            keyFrames[i].time = i / 10f;
            keyFrames[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        return clip;
    }
    */
}