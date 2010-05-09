/*************************************************************************
 *
 *   file		: ScriptMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using Microsoft.CSharp;
using MyDelegateHandler=DynamicMethods.MyDelegateHandler;

namespace DynamicMethods
{
    public delegate void MyDelegateHandler(int blah, int blahTwo);

    public class ScriptMgr
    {
        private Dictionary<string, Dictionary<string, Delegate>> m_loadedDeleMethods;
        private Dictionary<string, Dictionary<string, DynamicMethod>> m_loadedDMethods;
        private Dictionary<string, Dictionary<string, MethodInfo>> m_loadedMIMethods;
        private Dictionary<string, Type> m_registeredDelegateTypes;

        public ScriptMgr()
        {
            m_registeredDelegateTypes = new Dictionary<string, Type>();
            m_loadedDeleMethods = new Dictionary<string, Dictionary<string, Delegate>>();
        }

        private string[] GetLoadedAssemblyFilenames()
        {
            List<string> loadedAsmFiles = new List<string>();

            foreach(Assembly loadedAsm in AppDomain.CurrentDomain.GetAssemblies())
            {
                loadedAsmFiles.Add(loadedAsm.Location);
            }

            return loadedAsmFiles.ToArray();
        }

        private CompilerResults GenerateAssemblyFromFile(string codeFilename, ScriptLanguage scriptLanguage)
        {
            CSharpCodeProvider csharpProvider = new CSharpCodeProvider();

            CompilerParameters compileParams = new CompilerParameters();
            compileParams.ReferencedAssemblies.AddRange(GetLoadedAssemblyFilenames());
            compileParams.GenerateInMemory = true;
            compileParams.CompilerOptions = "/optimize+";

            return csharpProvider.CompileAssemblyFromFile(compileParams, new string[] {codeFilename});
        }

        private CompilerResults GenerateAssemblyFromFiles(string Path, bool CompileSubDirs, ScriptLanguage scriptLanguage)
        {
            string[] Files;
            string[] SubDirs;
            List<string> FilesToCompile = new List<string>();

            Files = Directory.GetFiles(Path);

            foreach (string TmpFile in Files)
                FilesToCompile.Add(TmpFile);

            if (CompileSubDirs)
            {
                SubDirs = Directory.GetDirectories(Path);

                foreach (string SubDir in SubDirs)
                {
                    Files = Directory.GetFiles(SubDir);

                    foreach (string TmpFile in Files)
                        FilesToCompile.Add(TmpFile);
                }
            }

            CompilerParameters compileParams = new CompilerParameters();
            compileParams.ReferencedAssemblies.AddRange(GetLoadedAssemblyFilenames());
            compileParams.GenerateInMemory = true;
            compileParams.CompilerOptions = "/optimize+";

            CSharpCodeProvider csharpProvider = new CSharpCodeProvider();

            return csharpProvider.CompileAssemblyFromFile(compileParams, FilesToCompile.ToArray());
        }

        private CompilerResults GenerateAssemblyFromCode(string[] scriptCode, ScriptLanguage scriptLanguage)
        {
            CompilerParameters compileParams = new CompilerParameters();
            compileParams.ReferencedAssemblies.AddRange(GetLoadedAssemblyFilenames());
            compileParams.GenerateInMemory = true;
            compileParams.CompilerOptions = "/optimize+";

            CSharpCodeProvider csharpProvider = new CSharpCodeProvider();

            return csharpProvider.CompileAssemblyFromSource(compileParams, scriptCode);
        }

        public void RegisterDelegate(Type registerType, Type methodDelegate)
        {
            if (!m_registeredDelegateTypes.ContainsKey(registerType.FullName))
            {
                m_registeredDelegateTypes.Add(registerType.FullName, methodDelegate);
            }
        }

        public CompilerResults CompileScripts(CompilationStrategy compileStrategy, ScriptLanguage scriptLanguage,
                                              object compileArgs)
        {
            CompilerResults results = null;

            switch (compileStrategy)
            {
                case CompilationStrategy.FromSingleFile:
                    if (compileArgs is string)
                    {
                        results = GenerateAssemblyFromFile(compileArgs.ToString(), scriptLanguage);
                    }
                    break;
                case CompilationStrategy.FromMultipleFiles:
                    if (compileArgs is List<string>)
                    {
                        results = GenerateAssemblyFromFiles(compileArgs as string, true, scriptLanguage);
                    }
                    break;
                case CompilationStrategy.FromMemory:
                    if (compileArgs is string)
                    {
                        results = GenerateAssemblyFromCode(compileArgs as string[], scriptLanguage);
                    }
                    break;
            }

            if (results != null && results.Errors.Count == 0)
            {
                foreach (Type exportedType in results.CompiledAssembly.GetExportedTypes())
                {
                    MethodInfo[] compiledMethods = exportedType.GetMethods(BindingFlags.Public | BindingFlags.Static);

                    foreach (MethodInfo compiledMethod in compiledMethods)
                    {
                        foreach (KeyValuePair<string, Type> dmDelegate in m_registeredDelegateTypes)
                        {
                            if(DoesDelegateSigMatch(dmDelegate.Value, compiledMethod))
                            {
                                if(!m_loadedDeleMethods.ContainsKey(dmDelegate.Key))
                                {
                                    m_loadedDeleMethods.Add(dmDelegate.Key, new Dictionary<string, Delegate>());
                                }

                                DynamicMethod dMethod = DynamicMethodHelper.ConvertFrom(compiledMethod);

                                m_loadedDeleMethods[dmDelegate.Key].Add(compiledMethod.Name, dMethod.CreateDelegate(dmDelegate.Value));
                            }
                        }
                    }
                }
            }

            return results;
        }

        public bool DoesDelegateSigMatch(Type delegateType, MethodInfo methodInfo)
        {
            ParameterInfo[] deleParams = delegateType.GetMethod("Invoke").GetParameters();

            ParameterInfo[] testParams = methodInfo.GetParameters();

            for (int i = 0; i < deleParams.Length; i++)
            {
                if (deleParams[i] == null || testParams[i] == null)
                {
                    return false;
                }

                if (deleParams[i].IsOut != testParams[i].IsOut)
                {
                    return false;
                }

                if (deleParams[i].ParameterType != testParams[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        public object ExecuteDelegate(Type registeredType, string methodName, params object[] dMethodArgs)
        {
            if (m_loadedDeleMethods.ContainsKey(registeredType.FullName))
            {
                if (m_loadedDeleMethods[registeredType.FullName].ContainsKey(methodName))
                {
                    return m_loadedDeleMethods[registeredType.FullName][methodName].DynamicInvoke(dMethodArgs);
                }
            }

            return null;
        }

        public Delegate RetrieveDelegate(Type registeredType, string methodName)
        {
            if (m_loadedDeleMethods.ContainsKey(registeredType.FullName))
            {
                if (m_loadedDeleMethods[registeredType.FullName].ContainsKey(methodName))
                {
                    return m_loadedDeleMethods[registeredType.FullName][methodName];
                }
            }

            return null;
        }
    }
}