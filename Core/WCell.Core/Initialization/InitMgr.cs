/*************************************************************************
 *
 *   file		: InitMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-05-22 19:03:37 +0200 (fr, 22 maj 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 910 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Util.Logging;
using WCell.Core.Localization;
using WCell.Util;

namespace WCell.Core.Initialization
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mgr"></param>
    /// <param name="step"></param>
    /// <returns>Whether or not to continue</returns>
    public delegate bool InitFailedHandler(InitMgr mgr, InitializationStep step);

    /// <summary>
    /// Handles the loading and execution of all initialization code.
    /// </summary>
    public class InitMgr
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        public static readonly InitFailedHandler VoidFailHandler = (mgr, step) => false;

        /// <summary>
        /// Handler asks user through Console whether to repeat the step and then continues or just stops
        /// </summary>
        public static readonly InitFailedHandler FeedbackRepeatFailHandler = (mgr, step) =>
        {
            s_log.Error(
                "\n\nInitialization Step failed - Do you want to repeat and continue (y) [or cancel startup (n)]? (y/n)");
            var response = Console.ReadLine();
            if (response != null && response.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
            {
                return mgr.Execute(step);
            }
            return false;
        };

        /// <summary>
        /// Initializes all Types of the given Assembly.
        /// </summary>
        /// <returns>Whether initialization could be performed for all found steps.</returns>
        public static bool Initialize(Assembly asm)
        {
            var initMgr = new InitMgr();
            initMgr.AddStepsOfAsm(asm);

            return initMgr.PerformInitialization();
        }

        /// <summary>
        /// Initializes the given Type.
        /// </summary>
        /// <returns>Whether initialization could be performed for all found steps in the given type.</returns>
        public static bool Initialize(Type type)
        {
            var initMgr = new InitMgr();
            var dependencies = new List<InitializationDependency>();
            initMgr.AddStepsOfType(type, dependencies);
            initMgr.InitDependencies(dependencies);

            return initMgr.PerformInitialization();
        }

        public readonly Dictionary<InitializationPass, List<InitializationStep>> InitSteps;
        private InitializationPass m_currentPass;
        private int totalFails, totalSuccess;
        private InitFailedHandler failHandler;
        private bool m_MeasureSteps;
        private bool m_newSteps;

        public readonly Dictionary<Type, GlobalMgrInfo> UnresolvedDependencies = new Dictionary<Type, GlobalMgrInfo>();

        public InitMgr()
            : this(true, VoidFailHandler)
        {
            failHandler = VoidFailHandler;
        }

        public InitMgr(InitFailedHandler failHandler)
            : this(true, failHandler)
        {
            this.failHandler = failHandler;
        }

        public InitMgr(bool measureSteps, InitFailedHandler failHandler)
        {
            m_MeasureSteps = measureSteps;
            this.failHandler = failHandler;
            InitSteps = new Dictionary<InitializationPass, List<InitializationStep>>();
            Init();
        }

        public bool MeasureSteps
        {
            get { return m_MeasureSteps; }
            set { m_MeasureSteps = value; }
        }

        /// <summary>
        /// The <see cref="InitializationPass"/> that is currently being executed.
        /// </summary>
        public InitializationPass CurrentPass
        {
            get { return m_currentPass; }
        }

        /// <summary>
        /// Finds, reads, and stores all initialization steps to be completed.
        /// </summary>
        private void Init()
        {
            // Create an empty list for every pass level.
            foreach (InitializationPass iPass in Enum.GetValues(typeof(InitializationPass)))
            {
                if (!InitSteps.ContainsKey(iPass))
                {
                    InitSteps.Add(iPass, new List<InitializationStep>());
                }
            }
        }

        /// <summary>
        /// Adds all InitializationSteps of the given Assembly.
        /// </summary>
        /// <param name="asm"></param>
        public void AddStepsOfAsm(Assembly asm)
        {
            // Go through every type in this assembly.
            var dependentInitors = new List<InitializationDependency>();
            foreach (var checkType in asm.GetTypes())
            {
                AddStepsOfType(checkType, dependentInitors);
            }
            InitDependencies(dependentInitors);
            // Report the total amount of steps loaded, even if we loaded none.
            //s_log.Info(string.Format(Resources.InitStepsLoaded, totalStepCount.ToString(), (totalStepCount == 1 ? "step" : "steps")));
        }

        public GlobalMgrInfo GetGlobalMgrInfo(Type t)
        {
            GlobalMgrInfo info;
            UnresolvedDependencies.TryGetValue(t, out info);
            return info;
        }

        private void InitDependencies(List<InitializationDependency> dependencies)
        {
            foreach (var depInitor in dependencies)
            {
                var canInit = true;
                foreach (var dep in depInitor.Info)
                {
                    var info = GetGlobalMgrInfo(dep.DependentType);
                    if (info == null)
                    {
                        throw new InitializationException("Invalid Dependency - " +
                                                          "{0} is dependent on {1} which is not a GlobalMgr.",
                                                          depInitor.Step.InitMethod.GetMemberName(), dep.DependentType.FullName);
                    }
                    info.Dependencies.Add(depInitor);
                    dep.DependentMgr = info;
                    if (!info.IsInitialized)
                    {
                        canInit = false;
                    }
                }

                if (canInit)
                {
                    DoExecute(depInitor.Step);
                }
            }
        }

        public void AddStepsOfType(Type type, List<InitializationDependency> dependentInitors)
        {
            var mgrAttr = type.GetCustomAttributes<GlobalMgrAttribute>().FirstOrDefault();
            if (mgrAttr != null)
            {
                UnresolvedDependencies.Add(type, new GlobalMgrInfo());
            }

            // Get all public static methods in this type.
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Check each method we found to see if it has an initialization attribute.
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttributes<InitializationAttribute>().FirstOrDefault();
                var depInitorAttrs = method.GetCustomAttributes<DependentInitializationAttribute>();

                // Can't have multiple instances of the attribute on a single method, so we check for 1.
                if (attribute != null)
                {
                    var part = new InitializationStep(attribute.Name, attribute.IsRequired, method);

                    if (depInitorAttrs.Length > 0)
                    {
                        var dep = new InitializationDependency(new InitializationStep(attribute.Name, attribute.IsRequired, method),
                            depInitorAttrs.TransformArray(attr => new DependentInitializationInfo(attr)));
                        dependentInitors.Add(dep);
                    }
                    else
                    {
                        InitSteps[attribute.Pass].Add(part);
                    }

                    m_newSteps = true;
                }
                else
                {
                    if (depInitorAttrs.Length > 0)
                    {
                        throw new InitializationException("Invalid {0} - Requires missing {1} for: {2}",
                            typeof(DependentInitializationAttribute).Name,
                            typeof(InitializationAttribute).Name,
                            method.GetMemberName());
                    }
                }

            }
        }

        /// <summary>
        /// Tries to execute all initialization steps, and returns the initialization result, 
        /// logging every failure and success.
        /// </summary>
        /// <returns>true if all initialization steps completed, false if a required step failed.</returns>
        public bool PerformInitialization()
        {
            m_newSteps = false;
            // Go through every pass level.
            foreach (InitializationPass pass in Enum.GetValues(typeof(InitializationPass)))
            {
                var steps = InitSteps[pass];

                // Make sure we have steps to go through.
                if (steps.Count > 0)
                {
                    m_currentPass = pass;
                    s_log.Info(string.Format(Resources.InitPass, (int)m_currentPass));
                    if (!Init(steps))
                    {
                        return false;
                    }
                }
            }

            s_log.Info(string.Format(Resources.InitComplete, totalSuccess, totalFails));

            return true;
        }

        private bool Init(List<InitializationStep> steps)
        {
            var currentSteps = steps.ToArray(); // create copy since collection might get modified

            foreach (var step in currentSteps)
            {
                if (step.Executed)
                {
                    continue;
                }
                if (!Execute(step))
                {
                    return false;
                }
            }

            while (m_newSteps)
            {
                // step added further steps -> Retroactively init all steps of previous passes that have been added
                m_newSteps = false;
                for (var pass = InitializationPass.First; pass <= m_currentPass; pass++)
                {
                    Init(InitSteps[pass]);
                }
            }
            return true;
        }

        public bool Execute(InitializationStep step)
        {
            step.Executed = true;
            var success = false;

            // check whether the Mgr is expected as first argument
            var args = step.GetArgs(this);
            var start = DateTime.Now;

            // If the invoke method returns a bool, we assume it is referring to whether or not
            // it actually completed.
            try
            {
                // Invoke the method and capture its return value
                var result = step.InitMethod.Invoke(null, args);

                if ((result is bool) && !(bool)result)
                {
                    // Step failed.  Check if it was required and and log the initial failure.
                    s_log.Error(Resources.InitStepFailed, step.InitStepName, step.InitMethod.Name, ".");
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                // Step failed.  Check if it was required and and log the initial failure.
                LogUtil.ErrorException(ex, Resources.InitStepFailed, step.InitStepName, step.InitMethod.Name, "");
            }

            if (success)
            {
                // Step succeeded

                totalSuccess++;
                if (!string.IsNullOrEmpty(step.InitStepName))
                {
                    var span = DateTime.Now - start;
                    if (m_MeasureSteps)
                    {
                        // save stats for later use
                    }
                    var timeStr = span.Minutes.ToString().PadLeft(2, '0') + ":" + span.Seconds.ToString().PadLeft(2, '0')
                                  + "." + span.Milliseconds.ToString().PadLeft(2, '0');
                    s_log.Info(string.Format(Resources.InitStepSucceeded, step.InitStepName, timeStr));
                }
            }
            else
            {
                if (!failHandler(this, step))
                {
                    // Step failed
                    if (step.IsRequired)
                    {
                        // It was required.  Log this, and cease any further initialization.
                        s_log.Fatal(string.Format(Resources.InitStepWasRequired, step.InitStepName, step.InitMethod.Name));

                        return false;
                    }
                    else
                    {
                        totalFails++;
                    }
                }
            }
            return true;
        }

        public void SignalGlobalMgrReady(Type type)
        {
            var info = GetGlobalMgrInfo(type);
            if (info == null)
            {
                throw new InitializationException("Invalid Signal - " +
                    "{0} signaled to be ready but did not register as a GlobalMgr.", type.FullName);
            }

            if (info.IsInitialized)
            {
                return;
                //throw new InitializationException("Invalid Signal - " +
                //    "{0} signaled to be ready more than once.", typeof(T).FullName);
            }

            info.IsInitialized = true;
            foreach (var depList in info.Dependencies)
            {
                var done = true;
                foreach (var dep in depList.Info)
                {
                    if (!dep.DependentMgr.IsInitialized)
                    {
                        done = false;
                        break;
                    }
                }

                if (done)
                {
                    DoExecute(depList.Step);
                }
            }
        }

        private void DoExecute(InitializationStep step)
        {
            if (!Execute(step))
            {
                throw new InitializationException("Failed to Execute dependent step: " + step);
            }
        }
    }
}