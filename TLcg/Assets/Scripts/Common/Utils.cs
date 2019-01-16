using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LCG
{
    public static class Utils
    {
        public static bool IsNull(UnityEngine.Object target)
        {
            if (null == target)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
