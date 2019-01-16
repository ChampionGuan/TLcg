namespace LCG
{
    public class PlayerPrefs : Singleton<PlayerPrefs>
    {
        /// <summary>
        /// 保存数据(string)
        /// </summary>
        /// <param name="dataType">Data type.</param>
        public void SaveString(string dataType, string str)
        {
            UnityEngine.PlayerPrefs.SetString(dataType, str);
        }

        /// <summary>
        /// 获取数据(string)
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="dataType">Data type.</param>
        public string GetString(string dataType)
        {
            return UnityEngine.PlayerPrefs.GetString(dataType, string.Empty);
        }

        /// <summary>
        /// 保存数据(float)
        /// </summary>
        public void SaveFloat(string dataType, float value)
        {
            UnityEngine.PlayerPrefs.SetFloat(dataType, value);
        }

        /// <summary>
        /// 获取数据(float)
        /// </summary>
        public float GetFloat(string dataType)
        {
            return UnityEngine.PlayerPrefs.GetFloat(dataType, 1);
        }

        /// <summary>
        /// 保存数据(int)
        /// </summary>
        public void SaveInt(string dataType, int value)
        {
            UnityEngine.PlayerPrefs.SetInt(dataType, value);
        }

        /// <summary>
        /// 获取数据(int)
        /// </summary>
        public float GetInt(string dataType)
        {
            return UnityEngine.PlayerPrefs.GetInt(dataType, 1);
        }
        /// <summary>
        /// 是否含有key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        /// <param name="key"></param>
        public void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }
    }
}