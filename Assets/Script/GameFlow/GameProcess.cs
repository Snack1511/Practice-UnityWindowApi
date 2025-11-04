using System;
using Script.Manager.StaticManager;
using UnityEngine;

namespace Script.GameFlow
{
    //GameProcess에 묶여 있는 UnityObject용 Component
    public class GameProcess : MonoBehaviour
    {
        private static GameProcess gameProcess;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            GameProcessManager.Update();
        }
    }
}
