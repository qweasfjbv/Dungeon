using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        StartCoroutine(LoadGameSceneAsync());
    }

    IEnumerator LoadGameSceneAsync()
    {
        // ���� �� �񵿱� �ε� ����
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;  // �ڵ����� �� ��ȯ�Ǵ� ���� ����

        // �� ���� ���� ����
        StartCoroutine(GenerateMap());

        // �ε��� �Ϸ�� ������ ��ٸ�
        while (!asyncLoad.isDone)
        {
            // �ε� ���� ���� UI ������Ʈ (��: �ε� ��)
            UpdateLoadingUI(asyncLoad.progress);

            // �ε��� ���� �Ϸ�Ǿ��� ��
            if (asyncLoad.progress >= 0.9f)
            {
                // �� ������ �Ϸ�Ǿ����� Ȯ��
                if (mapIsReady)
                {
                    asyncLoad.allowSceneActivation = true;  // �� ��ȯ ���
                }
            }
            yield return null;
        }
    }

    IEnumerator GenerateMap()
    {
        // �� ���� ���� ����
        // ��: �� �����͸� ���ݾ� �����ϰ� Yield ��ȯ
        for (int i = 0; i < 100; i++)
        {
            // ���⿡�� �� ���� ����
            yield return null;  // ���� �����ӱ��� ���
        }
        mapIsReady = true;  // �� ���� �Ϸ� �÷��� ����
    }

    void UpdateLoadingUI(float progress)
    {
        // �ε� UI ������Ʈ ����
        Debug.Log("Loading progress: " + progress * 100 + "%");
    }

    private bool mapIsReady = false;  // �� ���� �Ϸ� ����
}