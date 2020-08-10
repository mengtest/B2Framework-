﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace shaco
{
    /// <summary>
    /// 该方法已弃用，请使用更高级的 UnityGameFramework.Runtime.DebuggerComponent 代替
    /// </summary>
    [System.Obsolete]
    public class OutLogS : MonoBehaviour
    {
        static List<string> mLines = new List<string>();
        static List<string> mWriteTxt = new List<string>();
        private string outpath;
        void Start()
        {
#if !DEBUGLY
            //Application.persistentDataPath Unity中只有这个路径是既可以读也可以写的。
            outpath = Application.persistentDataPath + "/outLog.txt";
            //每次启动客户端删除之前保存的Log
            if (System.IO.File.Exists(outpath))
            {
                File.Delete(outpath);
            }
            //在这里做一个Log的监听
#if UNITY_5_3_OR_NEWER
            Application.logMessageReceived += HandleLog;
#else
            Application.RegisterLogCallback(HandleLog);
#endif
#endif
        }

        void Update()
        {
            //因为写入文件的操作必须在主线程中完成，所以在Update中哦给你写入文件。
            if (mWriteTxt.Count > 0)
            {
                string[] temp = mWriteTxt.ToArray();
                foreach (string t in temp)
                {
                    using (StreamWriter writer = new StreamWriter(outpath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(t);
                    }
                    mWriteTxt.Remove(t);
                }
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            //mWriteTxt.Add(logString);
            if (type == LogType.Error || type == LogType.Exception)
            {
                Log(logString);
                Log(stackTrace);
            }
        }

        //这里我把错误的信息保存起来，用来输出在手机屏幕上
        static public void Log(params object[] objs)
        {
            string text = string.Empty;
            for (int i = 0; i < objs.Length; ++i)
            {
                if (i == 0)
                {
                    text += objs[i].ToString();
                }
                else
                {
                    text += ", " + objs[i].ToString();
                }
            }
            if (Application.isPlaying)
            {
                if (mLines.Count > 20)
                {
                    mLines.RemoveAt(0);
                }
                mLines.Add(text);

            }
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.fontSize = (Screen.width + Screen.height) / 2 / 40;

            for (int i = 0, imax = mLines.Count; i < imax; ++i)
            {
                GUILayout.Label(mLines[i], style);
            }
        }
    }
}