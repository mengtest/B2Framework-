﻿using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public static partial class Utility
	{
        /// <summary>
        /// 获取脚本文本中的名字空间 
        /// <param name="textScript">脚本文本</param>
        /// <return></return>
        /// </summary>
        static public string GetNamespace(ref string textScript)
        {
            textScript = FilterScriptNotes(textScript);
            var retValue = GetNamespaceBase(ref textScript);
            if (retValue.Length > 0 && retValue[retValue.Length - 1] == '.')
                retValue = retValue.Remove(retValue.Length - 1, 1);
            return retValue;
        }

        /// <summary>
        /// 获取脚本中类名，允许循环调用，会依次编译该脚本中的所有类名
        /// <param name="textScript">脚本文本</param>
        /// <param name="inheritType">需要查找继承自该类型的类，允许为空，则默认查找脚本中所有类</param>
        /// </summary>
        static public string GetClassName(ref string textScript, params System.Type[] inheritTypes)
        {
            var oldTextScript = textScript;
            textScript = FilterScriptNotes(textScript);
            string retValue = string.Empty;
            var textClassLine = GetScriptSelectString(ref textScript, "class", "{");

            //get class name
            retValue = textClassLine.ParseUntilSymbols(0, ":", "<", " ", "\n");
            if (string.IsNullOrEmpty(retValue))
                retValue = textClassLine;

            bool isheritType = false;
            if (!inheritTypes.IsNullOrEmpty())
            {
                for (int i = inheritTypes.Length - 1; i >= 0; --i)
                {
                    if (null == inheritTypes[i])
                        continue;

                    var inheritType = inheritTypes[i];
                    var inheritTypeFullName = inheritType.FullName;
                    var inheritTypeName = inheritType.Name;
                    
                    if (textClassLine.Contains(inheritTypeFullName))
                    {
                        isheritType = true;
                        break;
                    }
                    //当不是全type名字找到的时候，再查看对应类型是匹配上using
                    else if (textClassLine.Contains(inheritTypeName))
                    {
                        if (oldTextScript.Contains("using " + inheritType.Namespace))
                        {
                            isheritType = true;
                            break;
                        }
                    }
                }
            }
            else
                isheritType = true;
            
            //没有匹配的父类
            if (!isheritType)
            {
                retValue = string.Empty;
            }
            return retValue;
        }

        /// <summary>
        /// 获取脚本中所有类名全称
        /// <param name="textScript">脚本文本</param>
        /// <param name="inheritType">需要查找继承自该类型的类，允许为空，则默认查找脚本中所有类</param>
        /// </summary>
        static public string[] GetClassNames(string textScript, params System.Type[] inheritTypes)
        {
            var retValue = new System.Collections.Generic.List<string>();
            string className = string.Empty;
            do
            {
                className = shaco.Base.Utility.GetClassName(ref textScript, inheritTypes);
                if (!string.IsNullOrEmpty(className))
                {
                    retValue.Add(className);
                }
                else
                {
                    break;
                }
            } while (true);

            return retValue.ToArray();
        }

        /// <summary>
        /// 获取脚本中类名全称，允许循环调用，会依次编译该脚本中的所有类名
        /// <param name="textScript">脚本文本</param>
        /// <param name="inheritType">需要查找继承自该类型的类，允许为空，则默认查找脚本中所有类</param>
        /// </summary>
        static public string GetFullClassName(ref string textScript, params System.Type[] inheritTypes)
        {
            textScript = FilterScriptNotes(textScript);
            var indexFindNamespace = textScript.IndexOf("namespace");
            var indexFindClass = textScript.IndexOf("class");
            var namespaceTmp = string.Empty;

            if (indexFindNamespace >= 0 && indexFindClass >= 0 && indexFindNamespace < indexFindClass)
            {
                namespaceTmp = GetNamespace(ref textScript);
            }

            var classNameTmp = GetClassName(ref textScript, inheritTypes);

            if (string.IsNullOrEmpty(classNameTmp))
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(namespaceTmp))
            {
                return namespaceTmp + "." + classNameTmp;
            }
            else
            {
                return classNameTmp;
            }
        }

        /// <summary>
        /// 获取脚本中所有类名全称
        /// *一个脚本中最多只允许出现一个namespace，嵌套的namespace解析过于麻烦就暂时不做了*
        /// <param name="textScript">脚本文本</param>
        /// <param name="inheritType">需要查找继承自该类型的类，允许为空，则默认查找脚本中所有类</param>
        /// </summary>
        static public string[] GetFullClassNames(string textScript, params System.Type[] inheritTypes)
        {
            var retValue = new System.Collections.Generic.List<string>();
            string className = string.Empty;
            var nameSpace = shaco.Base.Utility.GetNamespace(ref textScript);
            do
            {
                className = shaco.Base.Utility.GetClassName(ref textScript, inheritTypes);
                if (!string.IsNullOrEmpty(className))
                {
                    if (!className.StartsWith(nameSpace))
                        className = nameSpace + "." + className;
                    retValue.Add(className);
                }
                else
                {
                    break;
                }
            } while (true);

            return retValue.ToArray();
        }

        /// <summary>
        /// 过滤脚本中的注释
        /// <param name="textScript">脚本内容</param>
        /// <return>获取过滤注释后的脚本</return>
        /// </summary>
        static public string FilterScriptNotes(string textScript)
        {
            var retValue = new System.Text.StringBuilder();
            var lineScripts = textScript.Split('\n');

            for (int i = 0; i < lineScripts.Length; ++i)
            {
                var lineString = lineScripts[i];
                var indexFind = lineString.IndexOf("//");
                if (indexFind >= 0)
                {
                    lineString = lineString.Remove(indexFind);
                }

                var indexFindBegin = lineString.IndexOf("/*");
                var indexFindEnd = lineString.IndexOf("*/");
                if (indexFindBegin >= 0 && indexFindEnd >= 0)
                {
                    lineString = lineString.Remove(indexFindBegin, indexFindEnd - indexFindBegin + 2);
                }

                if (!string.IsNullOrEmpty(lineString))
                {
                    retValue.Append(lineString);
                }
            }
            return retValue.ToString();
        }
		
        //获取脚本文本中通过begin和end筛选出的字符串，并自动移动脚本文本
        static private string GetScriptSelectString(ref string textScript, string begin, string end)
        {
            string retValue = string.Empty;
            var classTag = begin;
            var brackTag = end;
            var indexClass = textScript.IndexOf(classTag);

            if (indexClass < 0)
            {
                return retValue;
            }

            var indexBraceBegin = textScript.IndexOf(brackTag, indexClass);
            if (indexClass < 0)
            {
                return retValue;
            }

            //skip space、tab、return
            retValue = textScript.Substring(indexClass + classTag.Length, indexBraceBegin - indexClass - classTag.Length - brackTag.Length + 1);
            for (int i = retValue.Length - 1; i >= 0; --i)
            {
                var charTmp = retValue[i];
                if (charTmp == ' ' || charTmp == '\t' || charTmp == '\n' || charTmp == '\r')
                {
                    retValue = retValue.Remove(i, 1);
                }
            }

            textScript = textScript.Remove(indexClass, indexBraceBegin - indexClass);
            return retValue;
        }

        static private string GetNamespaceBase(ref string textScript)
        {
            var retValue = GetScriptSelectString(ref textScript, "namespace", "{");
            if (!string.IsNullOrEmpty(retValue))
            {
                return retValue + "." + GetNamespaceBase(ref textScript);
            }
            else
                return retValue;
        }
	}
}