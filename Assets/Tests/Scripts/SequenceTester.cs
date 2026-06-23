using System;
using System.Collections;
using System.Collections.Generic;
using LenixSO.Sequences;
using LenixSO.Sequences.Composite;
using LenixSO.Sequences.Coroutines;
using LenixSO.Sequences.Decorator;
using UnityEngine;
using UnityEngine.InputSystem;

public class SequenceTester : MonoBehaviour
{
    private static SequenceTester instance;
    
    [SerializeField] private Transform parent;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private CompositeNode hCompositeNode;
    [SerializeField] private CompositeNode vCompositeNode;

    private List<Node> nodes = new();

    private void Awake()
    {
        instance = this;
        // GenericNode();
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            QueuedSequences queue = new();
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                queue.Add(nodes[i].nodeSequence);
            }

            queue.Begin();
        }
    }

    private void GenericNode()
    {
        var node = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, parent);
        node.InjectSequence(new CoroutineSequence(new(CoroutineExtensions.DelayCoroutine(2))));
        nodes.Add(node);
    }

    public ISequence LogSequence(ISequence sequence)
    {
        return new ObserverSequence(sequence,
                () => Debug.Log($"{sequence.name} begin"),
                () => Debug.Log($"{sequence.name} end"))
            .AddFinishedCallback(() => Debug.Log($"{sequence.name} finished"));
    }

    public static void CreateGenericNode() => instance.GenericNode();
}
