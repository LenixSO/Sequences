using System;
using System.Collections;
using System.Collections.Generic;
using InputSystemHelper;
using LenixSO.Sequences;
using LenixSO.Sequences.Composite;
using LenixSO.Sequences.Coroutines;
using LenixSO.Sequences.Decorator;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = InputSystemHelper.Input;

public class SequenceTester : MonoBehaviour
{
    private static SequenceTester instance;
    
    [SerializeField] private Transform parent;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private CompositeNode hCompositeNode;
    [SerializeField] private CompositeNode vCompositeNode;

    private Node selectorNode;

    private int currentSelection
    {
        get => selectionTree[^1];
        set => selectionTree[^1] = value;
    }
    private readonly List<Node> nodes = new();
    private readonly List<int> selectionTree = new() { 0 };
    private readonly List<List<Node>> nodesTree = new();

    private void Awake()
    {
        instance = this;
        Input.Map("UI").Action("Navigate").performed += OnNavigate;
    }

    private void Update()
    {
        if (!Keyboard.current.tabKey.wasPressedThisFrame) return;
        if (Keyboard.current.shiftKey.isPressed)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].ResetNode();
            }
            return;
        }
        QueuedSequences queue = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            queue.Add(nodes[i].nodeSequence);
        }

        queue.Begin();
    }

    #region NodeCreation
    private Node GenericNode(float delay = 1)
    {
        if (nodes.Count == 0) CreateSelector();
        var node = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, parent);
        node.transform.SetAsFirstSibling();
        ISequence genericSequence = new CoroutineSequence(new(() => CoroutineExtensions.DelayCoroutine(delay)));
        node.InjectSequence(genericSequence);
        return node;
    }

    private CompositeNode ParallelNode(int subnodes)
    {
        if (nodes.Count == 0) CreateSelector();
        var node = Instantiate(vCompositeNode, Vector3.zero, Quaternion.identity, parent);
        node.transform.SetAsFirstSibling();
        ParallelSequences sequence = new();
        for (int i = 0; i < subnodes; i++)
        {
            float delay = .5f + .5f * i;
            var subnode = GenericNode(delay);
            subnode.Text.SetText(i.ToString());
            node.AddSubnode(subnode);
            sequence.Add(subnode.nodeSequence);
        }
        node.InjectSequence(sequence);
        return node;
    }
    #endregion

    #region Selector
    private void CreateSelector()
    {
        selectorNode = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, parent.parent);
        selectorNode.rectTransform.SetAsFirstSibling();
        selectorNode.rectTransform.localScale = Vector3.one * 1.2f;
        selectorNode.SetRunningColor();
        nodesTree.Add(nodes);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if (direction == Vector2.zero) return;
        Vector2Int input = new Vector2Int((int)direction.x, (int)direction.y);
        bool alt = Keyboard.current.shiftKey.isPressed;

        int newIndex = (nodesTree[^1].Count + currentSelection + input.x) % nodesTree[^1].Count;
        Node node = nodesTree[^1][newIndex];
        if (!alt)
        {
            if (input.y != 0)
            {
                if (input.y < 0 && node is CompositeNode { subNodes: { Count: > 0 } } compositeNode)
                {
                    selectionTree.Add(0);
                    nodesTree.Add(compositeNode.subNodes);
                    newIndex = 0;
                    node = compositeNode.subNodes[newIndex];
                }
                else if (input.y > 0 && nodesTree is { Count: > 1 })
                {
                    selectionTree.RemoveAt(selectionTree.Count - 1);
                    nodesTree.RemoveAt(nodesTree.Count - 1);
                    newIndex = currentSelection;
                    node = nodesTree[^1][newIndex];
                }
            }

            SelectNode(node, newIndex);
            return;
        }

        Node currentNode = nodesTree[^1][currentSelection];
        (nodesTree[^1][currentSelection], nodesTree[^1][newIndex]) = (nodesTree[^1][newIndex], currentNode);
        node.transform.SetSiblingIndex(currentNode.transform.GetSiblingIndex());
        CoroutineExtensions.WaitAFrame(() => SelectNode(currentNode, newIndex));
    }

    private void SelectNode(Node node, int? newIndex = null)
    {
        selectorNode.transform.position = node.transform.position;
        selectorNode.rectTransform.localScale = node.transform.lossyScale * 1.2f;
        if (newIndex != null) currentSelection = newIndex.Value;
    }
    #endregion

    public ISequence LogSequence(ISequence sequence)
    {
        return new ObserverSequence(sequence,
                () => Debug.Log($"{sequence.name} begin"),
                () => Debug.Log($"{sequence.name} end"))
            .AddFinishedCallback(() => Debug.Log($"{sequence.name} finished"));
    }

    public static void CreateGenericNode()
    {
        var node = instance.GenericNode();
        node.Text.SetText($"{instance.nodes.Count}");
        instance.nodes.Add(node);
        CoroutineExtensions.WaitAFrame(() => instance.SelectNode(node, instance.nodes.Count - 1));
    }

    public static void CreateParallelNode(int subnodes)
    {
        var node = instance.ParallelNode(subnodes);
        node.Text.SetText($"P");
        instance.nodes.Add(node);
        CoroutineExtensions.WaitAFrame(() => instance.SelectNode(node, instance.nodes.Count - 1));
    }
}
