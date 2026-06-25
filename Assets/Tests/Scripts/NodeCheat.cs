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
        public static void Register() => CheatConsole.OnSetupDone += Setup;
        public static void Setup() => CheatConsole.RegisterCommand("add", AddCommand);

        private static readonly List<string> nodeTypes = new()
        {
            "par",
            "que",
            "loo",
            "shi",
            "fol",
        };
        
        private static void AddCommand(string[] parameters)
        {
            CheatConsole.GetKeyValuePair(parameters,nodeTypes, out string typeName, out int? value);
            int amount = value ?? 0;
            switch (typeName)
            {
                case "par":
                    SequenceTester.CreateParallelNode(amount);
                    return;
                case "fol":
                    SequenceTester.CreateFollowUpNode();
                    return;
                case "que":
                    SequenceTester.CreateQueuedNode(amount);
                    return;
                case "loo":
                    SequenceTester.CreateLoopingNode(amount);
                    return;
                case "shi":
                    SequenceTester.CreateShiftingNode(amount);
                    return;
                default:
                    for (int i = 0; i <= amount; i++)
                        SequenceTester.CreateGenericNode();
                    return;
            }
        }
    }
}
