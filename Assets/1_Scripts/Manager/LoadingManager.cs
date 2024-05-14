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
        // 게임 씬 비동기 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;  // 자동으로 씬 전환되는 것을 방지

        // 맵 생성 로직 시작
        StartCoroutine(GenerateMap());

        // 로딩이 완료될 때까지 기다림
        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상태 UI 업데이트 (예: 로딩 바)
            UpdateLoadingUI(asyncLoad.progress);

            // 로딩이 거의 완료되었을 때
            if (asyncLoad.progress >= 0.9f)
            {
                // 맵 생성이 완료되었는지 확인
                if (mapIsReady)
                {
                    asyncLoad.allowSceneActivation = true;  // 씬 전환 허용
                }
            }
            yield return null;
        }
    }

    IEnumerator GenerateMap()
    {
        // 맵 생성 로직 구현
        // 예: 맵 데이터를 조금씩 생성하고 Yield 반환
        for (int i = 0; i < 100; i++)
        {
            // 여기에서 맵 조각 생성
            yield return null;  // 다음 프레임까지 대기
        }
        mapIsReady = true;  // 맵 생성 완료 플래그 설정
    }

    void UpdateLoadingUI(float progress)
    {
        // 로딩 UI 업데이트 로직
        Debug.Log("Loading progress: " + progress * 100 + "%");
    }

    private bool mapIsReady = false;  // 맵 생성 완료 여부
}