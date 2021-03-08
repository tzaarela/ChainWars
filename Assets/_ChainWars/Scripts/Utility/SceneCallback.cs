using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCallback : MonoBehaviour {

    private bool isFirstUpdate = true;

    private void Update() {
        if (isFirstUpdate) {
            isFirstUpdate = false;
            SceneController.LoaderCallback();
        }
    }

}
