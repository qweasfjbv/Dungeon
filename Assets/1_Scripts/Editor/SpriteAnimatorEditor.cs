using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;

// NE, NW, SE, SW
// Attack 
// Die (No Dir)
// Dmg
// Idle
// Walk
public enum AnimDir { SE = 0, SW, NE, NW}
public enum AnimName { Attack = 0, DieSoul, Dmg, Idle, Walk}


public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}

public class SpriteAnimatorEditor : EditorWindow
{
    private string characterName = "NewAnimator";
    private string storePath;

    private Texture2D[] spriteSheets = new Texture2D[5];
    private AnimationClip[,] animationClips = new AnimationClip[5, 4];

    private int columns = 4;
    private int rows = 4;
    private int widthPx = 32;
    private int heightPx = 32;
    private AnimatorController animatorController;
    private AnimatorOverrideController overrideController;
    private AnimatorController overriddenConroller;

    private AnimationClipOverrides clipOverrides;

    [MenuItem("MyEditor/Sprite Animator Editor")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAnimatorEditor>("Sprite Animator Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animator Settings", EditorStyles.boldLabel);
        GUILayout.Space(20);

        characterName = EditorGUILayout.TextField("Character Name", characterName);
        overriddenConroller = (AnimatorController)EditorGUILayout.ObjectField("Overridden Controller", overriddenConroller, typeof(AnimatorController), false);


        GUILayout.Space(20);

        if (GUILayout.Button("Set Store path"))
        {
            storePath = EditorUtility.OpenFolderPanel("Store path", "", "");
            storePath = storePath.Substring(storePath.IndexOf('A'));
        }
        GUILayout.Label("Store Path : " + storePath);
        GUILayout.Space(20);

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

        for (int i = 0; i < Enum.GetValues(typeof(AnimName)).Length; i++)
        {
            SliceSpriteSheet(spriteSheets[i], i);
        }

        
        overrideController = new AnimatorOverrideController(overriddenConroller);
        clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);


        overrideController.GetOverrides(clipOverrides);
        for (int i = 0; i < Enum.GetValues(typeof(AnimName)).Length; i++)
        {
            if (i == 1)
            {
                clipOverrides["Human_DieSoul"] = animationClips[1, 0];
            }
            else
            {
                for (int j = 0; j < 4; j++)
                {

                    clipOverrides["Human_" + Enum.GetName(typeof(AnimName), i) + "_" + Enum.GetName(typeof(AnimDir), j)] = animationClips[i, j];
                    Debug.Log("Human_" + Enum.GetName(typeof(AnimName), i) + "_" + Enum.GetName(typeof(AnimDir), j));
                }
            }
        }

        overrideController.ApplyOverrides(clipOverrides);

        AssetDatabase.CreateAsset(overrideController, storePath + "/" + characterName + "_Animator.overrideController");
        AssetDatabase.SaveAssets();


        Debug.Log("Animator and Override Controller created successfully.");
    }

    private void SliceSpriteSheet(Texture2D texture, int sheetIdx)
    {
        List<Sprite> sprites = new List<Sprite>();

        int columns = texture.height / widthPx;
        int rows = texture.width / widthPx;

        for (int y = 0; y < columns; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                Rect rect = new Rect(x * widthPx, y * heightPx, widthPx, heightPx);
                Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.4f));
                sprites.Add(sprite);
            }

            // row마다 각 방향 clip을 하나씩 만듭니다.
            CreateAnimationClip(sprites, sheetIdx, y);
            sprites.Clear();
        }

        return;
    }

    private void CreateAnimationClip(List<Sprite> sprites, int sheetIdx, int dir)
    { 
        dir = (dir + 2) % 4;
        AnimationClip clip = new AnimationClip();
        string clipName = "";
        clipName = characterName + "_" + Enum.GetName(typeof(AnimName), sheetIdx) + (sheetIdx != 1 ? Enum.GetName(typeof(AnimDir), dir) : "");

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

        
        animationClips[sheetIdx, (sheetIdx == (int)AnimName.DieSoul) ? 0 : dir] = clip;

        // 에셋으로 생성 후 저장
        AssetDatabase.CreateAsset(clip, storePath + "/" + clipName + ".anim");
        AssetDatabase.SaveAssets();

        return;
    }

 
}