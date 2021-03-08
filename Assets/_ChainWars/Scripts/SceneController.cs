using Assets._ChainWars.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneController {

    private class LoadingMonoBehaviour : MonoBehaviour { }

    
    private static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;

    public static void Load(SceneType scene) {
        // Set the loader callback action to load the target scene
        onLoaderCallback = () => {
            Debug.Log("Loading lobby scene");
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
        };

        // Load the loading scene
        SceneManager.LoadScene(SceneType.LoadingScene.ToString());
    }

    private static IEnumerator LoadSceneAsync(SceneType scene) {
        yield return null;

        loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());

        while (!loadingAsyncOperation.isDone) {

            yield return null;
        }
    }

    public static float GetLoadingProgress() {
        if (loadingAsyncOperation != null) {
            return loadingAsyncOperation.progress;
        } else {
            return 1f;
        }
    }

    public static void LoaderCallback() {
        // Triggered after the first Update which lets the screen refresh
        // Execute the loader callback action which will load the target scene
        if (onLoaderCallback != null) {
            Debug.Log("loaderCallback");
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }
}
