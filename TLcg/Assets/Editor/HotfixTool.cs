using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using XLua;

namespace LCG
{
    public static class HotfixTool
    {
        [Hotfix(HotfixFlag.IgnoreProperty)]
        public static List<Type> by_field = new List<Type>()
         {
             typeof(Main),
         };

        [Hotfix(HotfixFlag.IgnoreProperty)]
        public static List<Type> by_property
        {
            get
            {
                List<Type> hotfixList = new List<Type>();

                Type[] assemblyClass = Assembly.Load("Assembly-CSharp").GetTypes();
                foreach (var value in assemblyClass)
                {
                    if (value.Namespace == "LCG")
                    {
                        hotfixList.Add(value);
                    }
                }

                // Type[] coreclass = Assembly.Load("Assembly-Core").GetTypes();
                // foreach (var value in coreclass)
                // {
                //     if (value.Namespace == "LCG")
                //     {
                //         hotfixList.Add(value);
                //     }
                // }

                return hotfixList;
            }
        }
    }
}