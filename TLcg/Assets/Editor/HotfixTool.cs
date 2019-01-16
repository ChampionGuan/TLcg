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
        [Hotfix]
        public static List<Type> by_field = new List<Type>()
         {
             typeof(Main),
         };

        [Hotfix]
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

                //Type[] coreClass = Assembly.Load("Core").GetTypes();
                //foreach (var value in coreClass)
                //{
                //    if (value.Namespace == "LCG")
                //    {
                //        hotfixList.Add(value);
                //    }
                //}

                return hotfixList;
            }
        }
    }
}