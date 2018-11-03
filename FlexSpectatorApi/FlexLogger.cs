using System;
using System.Collections.Generic;

namespace FlexSpectatorApi
{
    /// <summary>
    /// Universal logger, log messages can be listened using <see cref="RegisterLogCallback(Action{string})"/>
    /// </summary>
    public static class FlexLogger
    {
        private static List<Action<string>> listeners = new List<Action<string>>();

        /// <summary>
        /// Write to <see cref="Console"/>, and call the registered listeners with the given <see cref="object.ToString"/>
        /// </summary>
        /// <param name="o"></param>
        public static void Log(object o)
        {
            string s = o.ToString();
            Console.WriteLine(s);

            foreach (var l in listeners)
                l.Invoke(s);
        }

        /// <summary>
        /// Registers an action. 
        /// This action will be invoked on every <see cref="Log(object)"/> call
        /// </summary>
        /// <param name="logger"></param>
        public static void RegisterLogCallback(Action<string> logger)
        {
            listeners.Add(logger);
        }
    }
}