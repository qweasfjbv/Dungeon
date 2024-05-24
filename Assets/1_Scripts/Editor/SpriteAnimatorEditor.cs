using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using UnityEditorInternal;
using System.Data.SqlClient;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using System.Linq;
using NUnit.Framework.Constraints;

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
    private AnimatorOverrideController overrideController;
    private UnityEditor.Animations.AnimatorController overriddenConroller;

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
        overriddenConroller = (UnityEditor.Animations.AnimatorController)EditorGUILayout.ObjectField("Overridden Controller", overriddenConroller, typeof(UnityEditor.Animations.AnimatorController), false);


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
        // ����ó��. ��Ʈ�� ���� �����Ǿ��־�� �մϴ�.
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
                }
            }
        }

        overrideController.ApplyOverrides(clipOverrides);

        AssetDatabase.CreateAsset(overrideController, storePath + "/" + characterName + "_Animator.overrideController");
        AssetDatabase.SaveAssets();


        Debug.Log("Animator and Override Controller created successfully.");
    }

    // �� �ִϸ��̼� ��������Ʈ �� �� �� ȣ��˴ϴ�.
    // �� row�� �ִϸ��̼� Ŭ������ ����ϴ�.
    private void SliceSpriteSheet(Texture2D texture, int sheetIdx)
    {

        List<SpriteMetaData> mData = new List<SpriteMetaData>();
        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
            texture, Vector2.zero, new Vector2(widthPx, heightPx), Vector2.zero);


        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        ti.isReadable = true;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = widthPx;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;


        for (int i = 0; i < rects.Length; i++)
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.rect = rects[i];
            smd.pivot = new Vector2(0.5f, 0.4f);
            smd.alignment = (int)SpriteAlignment.Center;
            smd.name = texture.name + "_" + i;
            mData.Add(smd);
        }



        ti.spritesheet = mData.ToArray();

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);


        List<Sprite> sprites = new List<Sprite>();
        // column ����. �ִϸ��̼� �� ���� �� �̹��� ������ ���ϴ�.
        int animFrameCount = texture.width / widthPx;

        int dir = 0;
        int cnt = 0;
        for (int i = 0; i < assets.Length; i++)
        {
            if ((cnt + 1)% animFrameCount == 0)
            {
                CreateAnimationClip(sprites, sheetIdx, dir++);
                sprites.Clear();
            }

            if (assets[i] is Sprite)
            {
                Debug.Log(assets[i].name);
                sprites.Add(assets[i] as Sprite);
                cnt++;
            }
        }

        return;


    }

    private void CreateAnimationClip(List<Sprite> sprites, int sheetIdx, int dir)
    {
        dir = (dir + 2) % 4;
        AnimationClip clip = new AnimationClip();
        string clipName = "";
        clipName = characterName + "_" + Enum.GetName(typeof(AnimName), sheetIdx) + "_" + (sheetIdx != 1 ? Enum.GetName(typeof(AnimDir), dir) : "");

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

        // �������� ���� �� ����
        AssetDatabase.CreateAsset(clip, storePath + "/" + clipName + ".anim");
        AssetDatabase.SaveAssets();

        return;
    }

 
}