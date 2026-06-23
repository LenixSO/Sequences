using System;
using System.Collections;
using System.Collections.Generic;
using Cheat;
using UnityEngine;

namespace ModeCheat
{
    public static class NodeCheat
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Register()
        {
            CheatConsole.OnSetupDone += Setup;
        }

        public static void Setup()
        {
            CheatConsole.RegisterCommand("add", AddCommand);
        }

        private static void AddCommand(string[] parameters)
        {
            string typeName = parameters.Length > 0 ? parameters[0] : string.Empty;
            switch (typeName)
            {
                default:
                    SequenceTester.CreateGenericNode();
                    return;
            }
        }
    }
}
