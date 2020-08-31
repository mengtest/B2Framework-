﻿using UnityEngine;

namespace B2Framework
{
    public partial class GameManager : MonoSingleton<GameManager>, IManager
    {
        private float _pauseSpeed;
        protected override void Awake()
        {
            base.Awake();
            Initialize();
            Application.lowMemory += OnLowMemory;
        }
        void Start()
        {
            // 从这里启动游戏
            ScenesManager.Instance.LoadSceneAsync(Scenes.Updater.ToString());
        }
        // void OnGUI()
        // {
        //     if (GUI.Button(new Rect(100, 100, 100, 50), ""))
        //     {
        //         var size = string.Empty;
        //         var go = false;
        //         // GC
        //         GameUtility.Sampling(() =>
        //         {
        //             // for (var i = 0; i < 100; i++)

        //         });

        //         // 耗时
        //         Utility.Watch(() =>
        //         {
        //             // for (var i = 0; i < 100; i++)

        //         });
        //         Log.Debug(size);
        //         Log.Debug(go);
        //     }
        // }
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void Pause()
        {
            if (isGamePaused) return;

            _pauseSpeed = gameSpeed;
            gameSpeed = 0f;
        }
        /// <summary>
        /// 恢复暂停
        /// </summary>
        public void Resume()
        {
            if (!isGamePaused) return;

            gameSpeed = _pauseSpeed;
        }
        /// <summary>
        /// 恢复游戏正常速度
        /// </summary>
        public void NormalSpeed()
        {
            if (m_GameSpeed != 1f) gameSpeed = 1f;
        }
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        void Update()
        {
            // TODO:更新游戏逻辑
        }
        /// <summary>
        /// 重启游戏
        /// </summary>
        public void Restart()
        {
            Dispose();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        /// <param name="restart"></param>
        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        protected override void OnDestroy()
        {
            DisposeManagers();
            SceneMgr?.Dispose();
            LuaMgr?.Dispose();
            NetMgr?.Dispose();

            m_instance = null;
            base.OnDestroy();
        }
        /// <summary>
        /// 游戏退出时调用
        /// </summary>
        protected override void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
            base.OnApplicationQuit();
        }
        /// <summary>
        /// 低内存时的处理逻辑
        /// </summary>
        private void OnLowMemory()
        {
            Log.Warning("Low memory reported...");
            // TODO:释放对象池
            // TODO:释放加载的资源
            AssetsManger.UnloadUnusedAssets();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}