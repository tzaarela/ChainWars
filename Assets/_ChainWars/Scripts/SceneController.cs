using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
	public class SceneController
	{
        private static SceneController instance = null;
        public static SceneController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SceneController();
                }
                return instance;
            }
        }
        private SceneController()
        {
        }

        public void ChangeScene(SceneType scene)
		{
            Debug.Log("Loading sceneIndex " + (int)scene);
            SceneManager.LoadScene((int)scene, LoadSceneMode.Single);
        }
    }
}
