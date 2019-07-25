using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System;

namespace LCG
{
    public class ExcelRead : EditorWindow
    {
        [MenuItem("Tools/excel工具")]
        public static void Build()
        {
            Excel.parse();
        }

        public class Excel
        {
            private static string excelSuffix = ".csv";
            private static string readDir = null;
            private static string outDir = null;
            private static List<string> files = new List<string>();

            public static void parse()
            {
                getInfo(() =>
                    {
                        Debug.Log("开始!!!!");
                        ExcleToJson.ToJson(readDir, outDir, files);
                        Debug.Log("完成!!!!");
                    });
            }

            private static void getInfo(Action action)
            {
                string path = Application.dataPath + "/Editor/Excel.txt";
                if (!File.Exists(path))
                {
                    Debug.LogError("路径文件不存在！！" + path);
                }

                string t = Utils.readFile(path);
                Excel.parseInfo(t);

                if (null != action)
                {
                    action.Invoke();
                }
            }
            private static void parseInfo(string t)
            {
                t = t.Replace("\r", "");
                string[] sp = t.Split('\n');
                if (sp.Length < 2)
                {
                    Debug.LogError("配置文件错误！！");
                }
                foreach (var v in sp)
                {
                    string[] sp2 = v.Split('=');
                    if (sp2[0].ToLower() == "readdir")
                    {
                        readDir = sp2[1].Replace('\\', '/');
                    }
                    else if (sp2[0].ToLower() == "outdir")
                    {
                        outDir = sp2[1].Replace('\\', '/');
                    }
                }

                if (string.IsNullOrEmpty(readDir))
                {
                    Debug.LogError("请填写正确的配置文件读取路径！！");
                }
                if (string.IsNullOrEmpty(outDir))
                {
                    Debug.LogError("请填写正确的配置文件输出路径！！");
                }
                if (!Directory.Exists(readDir))
                {
                    Directory.CreateDirectory(readDir);
                }
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                Debug.Log("读取路径：" + readDir);
                Debug.Log("输出路径：" + outDir);

                files.Clear();
                List<string> paths = Utils.getAllFilesPathInDir(readDir);
                foreach (var v in paths)
                {
                    if (!v.EndsWith(excelSuffix))
                    {
                        continue;
                    }
                    files.Add(v.Replace('\\', '/'));
                }
            }

            #region excelToJson
            public class ExcleToJson
            {
                private static Dictionary<string, Excel> excels = new Dictionary<string, Excel>();
                private static List<string> shell = new List<string>();

                private static string readDir = null;
                private static string outDir = null;

                public static Excel ToJson(string p, bool isShell = false)
                {
                    Excel e;
                    if (excels.ContainsKey(p))
                    {
                        e = excels[p];
                    }
                    else
                    {
                        if (!File.Exists(p))
                        {
                            Debug.Log("文件不存在：" + p);
                            return null;
                        }
                        e = new Excel(p);
                        excels.Add(p, e);
                    }
                    if (isShell && !shell.Contains(e.fileOutPath))
                    {
                        shell.Add(e.fileOutPath);
                    }
                    return e;
                }

                public static void ToJson(string r, string o, List<string> fs)
                {
                    readDir = r;
                    outDir = o;
                    shell.Clear();
                    excels.Clear();
                    foreach (var v in fs)
                    {
                        ExcleToJson.ToJson(v);
                    }
                    foreach (var v in shell)
                    {
                        if (File.Exists(v))
                        {
                            File.Delete(v);
                        }
                    }
                }

                public class Excel
                {
                    private string filePath;
                    public string fileOutPath;

                    private StringBuilder txt = new StringBuilder();
                    private StringBuilder temp = new StringBuilder();
                    private List<string> names = new List<string>();
                    private List<string> types = new List<string>();
                    public Dictionary<int, List<string>> contents = new Dictionary<int, List<string>>();

                    private int row = 0;
                    private int line = 0;

                    public Excel(string fp)
                    {
                        filePath = fp;
                        fileOutPath = filePath.Replace(readDir, outDir);
                        fileOutPath = fileOutPath.Substring(0, fileOutPath.LastIndexOf("."));
                        this.parse();
                    }
                    private void parse()
                    {
                        Debug.Log("开始解析文件：" + filePath);

                        txt.Length = 0;
                        txt.AppendLine("{");
                        read();

                        for (row = 3; row < contents.Count; row++)
                        {
                            string s = contents[row][0];
                            if (string.IsNullOrEmpty(s))
                            {
                                continue;
                            }
                            txt.Append(string.Format("\"{0}\":", s));
                            txt.Append(parseRow());
                            if (row != contents.Count - 1)
                            {
                                txt.Append(",");
                            }
                            txt.AppendLine("");
                        }

                        txt.AppendLine("}");
                        fileOutPath = fileOutPath + ".json";
                        Utils.WriteFile(fileOutPath, txt.ToString());

                        Debug.Log("解析文件完成：" + fileOutPath);
                    }
                    private void read()
                    {
                        names.Clear();
                        types.Clear();
                        contents.Clear();

                        string s = Utils.readFile(filePath).Replace("\r", "").TrimEnd();
                        string[] sp = s.Split('\n');
                        if (sp.Length < 3)
                        {
                            Debug.LogError("无表头！！");
                        }
                        for (int i = 0; i < sp.Length; i++)
                        {
                            if (!contents.ContainsKey(i))
                            {
                                contents.Add(i, new List<string>());
                            }
                            foreach (var v in sp[i].Split(','))
                            {
                                contents[i].Add(v);
                                if (i == 1)
                                {
                                    names.Add(v);
                                }
                                else if (i == 2)
                                {
                                    types.Add(v);
                                }
                            }
                        }
                    }
                    private string parseRow(int? r = null)
                    {
                        temp.Length = 0;
                        row = r == null ? row : r.Value;

                        for (line = 0; line < types.Count; line++)
                        {
                            if (line == 0)
                            {
                                temp.Append("{");
                            }

                            temp.Append(string.Format("\"{0}\":", names[line]));
                            string s = contents[row][line];
                            Excel e = null;
                            string[] sp;

                            switch (types[line].ToLower())
                            {
                                case "string":
                                    temp.Append(string.Format("\"{0}\"", s));
                                    break;
                                case "number":
                                    temp.Append(s);
                                    break;
                                case "boolean":
                                    temp.Append(s.ToLower());
                                    break;
                                case "arraystring":
                                    sp = s.Split('\\');
                                    temp.Append("[");
                                    for (int b = 0; b < sp.Length; b++)
                                    {
                                        if (b == sp.Length - 1)
                                        {
                                            temp.Append(string.Format("\"{0}\"", sp[b]));
                                        }
                                        else
                                        {
                                            temp.Append(string.Format("\"{0}\",", sp[b]));
                                        }
                                    }
                                    temp.Append("]");
                                    break;
                                case "arraynumber":
                                    sp = s.Split('\\');
                                    temp.Append("[");
                                    for (int b = 0; b < sp.Length; b++)
                                    {
                                        if (b == sp.Length - 1)
                                        {
                                            temp.Append(sp[b]);
                                        }
                                        else
                                        {
                                            temp.Append(string.Format("{0},", sp[b]));
                                        }
                                    }
                                    temp.Append("]");
                                    break;
                                case "arrayboolean":
                                    sp = s.Split('\\');
                                    temp.Append("[");
                                    for (int b = 0; b < sp.Length; b++)
                                    {
                                        if (b == sp.Length - 1)
                                        {
                                            temp.Append(sp[b]);
                                        }
                                        else
                                        {
                                            temp.Append(string.Format("{0},", sp[b].ToLower()));
                                        }
                                    }
                                    temp.Append("]");
                                    break;
                                case "shell":
                                    e = ExcleToJson.ToJson(string.Format("{0}/{1}{2}", filePath.Substring(0, filePath.LastIndexOf('/')), names[line], excelSuffix), true);
                                    if (null == e) break;
                                    for (int i = 3; i < e.contents.Count; i++)
                                    {
                                        if (e.contents[i][0] == s)
                                        {
                                            temp.Append(e.parseRow(i));
                                        }
                                    }
                                    break;
                                case "arrayshell":
                                    sp = s.Split('\\');
                                    temp.Append("[");
                                    e = null;
                                    for (int b = 0; b < sp.Length; b++)
                                    {
                                        e = ExcleToJson.ToJson(string.Format("{0}/{1}{2}", filePath.Substring(0, filePath.LastIndexOf('/')), names[line], excelSuffix), true);
                                        if (null == e) break;
                                        for (int i = 3; i < e.contents.Count; i++)
                                        {
                                            if (e.contents[i][0] == sp[b])
                                            {
                                                if (b == sp.Length - 1)
                                                {
                                                    temp.Append(e.parseRow(i));
                                                }
                                                else
                                                {
                                                    temp.Append(string.Format("{0},", e.parseRow(i)));
                                                }
                                            }
                                        }
                                    }
                                    temp.Append("]");
                                    break;
                                default: break;
                            }
                            if (line != types.Count - 1)
                            {
                                temp.Append(",");
                            }
                            else
                            {
                                temp.Append("}");
                            }
                        }
                        return temp.ToString();
                    }
                }
            }
            #endregion


            #region excelToXML
            public class ExcleToXML
            {
            }
            #endregion

            #region 工具
            public class Utils
            {
                public static string readFile(string path)
                {
                    if (!File.Exists(path))
                    {
                        Debug.Log("[warning] 文件不存在: " + path);
                        return string.Empty;
                    }

                    string text = string.Empty;
                    try
                    {
                        using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding(936), true))
                        {
                            text = sr.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(path + "[error] 读取文件错误: " + e);
                        return string.Empty;
                    }

                    return text;
                }
                public static bool WriteFile(string path, string content)
                {
                    string dir = path.Substring(0, path.LastIndexOf('/'));
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(path, false))
                        {
                            sw.Write(content);
                        }
                    }
                    catch (Exception e)
                    {
                        //路径与名称未找到文件则直接返回空
                        Debug.LogWarning(path + "[error] 写入文件错误: " + e);
                        return false;
                    }

                    return true;
                }
                public static List<string> getAllFilesPathInDir(string path)
                {
                    List<string> filePathList = new List<string>();
                    if (!Directory.Exists(path))
                    {
                        return filePathList;
                    }
                    filePathList.AddRange(Directory.GetFiles(path));

                    string[] subDirs = Directory.GetDirectories(path);
                    foreach (string subPath in subDirs)
                    {
                        filePathList.AddRange(getAllFilesPathInDir(subPath));
                    }

                    return filePathList;
                }
            }
            #endregion
        }
    }

}