using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LCG
{
    public class ABTest : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log(ABHelper.AppTempCachePath);
            ABCheck.Instance.Initialize(() =>
            {
                // 搭建本地http服，使用hfs.exe测试
                ABCheck.Instance.CheckHotter("0.0.1.3", null, "http://192.168.1.110:100/ab_TAccumulation/", Complete);
            });
        }
        void Complete(ABHelper.VersionArgs args)
        {
            if (args.state == ABHelper.EVersionState.DownloadConfirm)
            {
                args.callBack("");
            }
            else if (args.state == ABHelper.EVersionState.ClientVersionId)
            {
                Debug.Log("客户端版本号：" + args.sValue);
            }
            else if (args.state == ABHelper.EVersionState.ServerVersionId)
            {
                Debug.Log("服务器所需版本号：" + args.sValue);
            }
            else if (args.state == ABHelper.EVersionState.HotfixComplete)
            {
                Debug.Log("资源初始化完成！！！");
            }
            else if (args.state == ABHelper.EVersionState.UnknowError)
            {
                Debug.Log("更新异常！" + args.sValue);
            }
            else if (args.state == ABHelper.EVersionState.AutofixProgress)
            {
                Debug.Log("修复进度！" + args.fValue);
            }
            else if (args.state == ABHelper.EVersionState.CheckFileProgress)
            {
                Debug.Log("检测进度！" + args.fValue);
            }
            else if (args.state == ABHelper.EVersionState.AutofixDeleteFile)
            {
                Debug.Log("删除进度！" + args.fValue);
            }
            else if (args.state == ABHelper.EVersionState.AutofixComplete)
            {
                Debug.Log("修复完成！" + args.sValue);
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                for (int i = 0; i < 2; i++)
                {
                    UnityEngine.Object obj = ABManager.Load("Prefabs/Monster/Cha_Archer_L1", "Cha_Archer_L1", typeof(GameObject));
                    Debug.Log(obj);
                    GameObject.Instantiate(obj);
                }
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                for (int i = 0; i < 2; i++)
                {
                    ABManager.AsyncLoad("Prefabs/Monster/Cha_Archer_L1", "Cha_Archer_L1", typeof(GameObject),
                    (obj) =>
                    {
                        Debug.Log(obj);
                        GameObject.Instantiate(obj);
                    }
                    );
                }
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                ABManager.LoadScene("HotterTest");
                UnityEngine.SceneManagement.SceneManager.LoadScene("HotterTest");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ABManager.UnloadAll();
            }


            // 自动检测
            if (Input.GetKeyDown(KeyCode.P))
            {
                ABAutofix.Instance.Repair(true, Complete);
            }
        }
    }
}