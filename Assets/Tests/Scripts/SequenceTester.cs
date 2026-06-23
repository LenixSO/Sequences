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
    private List<Node> leafNode => nodesTree[^1];

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

        int newIndex = (leafNode.Count + currentSelection + input.x) % leafNode.Count;
        Node node = leafNode[newIndex];
        if (alt) MoveNode(input, node, newIndex);
        else MoveSelection(input, node, newIndex);
    }

    private void MoveSelection(Vector2Int input, Node node, int newIndex)
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
                node = leafNode[newIndex];
            }
        }

        SelectNode(node, newIndex);
    }

    private void MoveNode(Vector2Int input, Node node, int newIndex)
    {
        if (input.y == 0)
        {
            Node currentNode = leafNode[currentSelection];
            (leafNode[currentSelection], leafNode[newIndex]) = (leafNode[newIndex], currentNode);
            node.transform.SetSiblingIndex(currentNode.transform.GetSiblingIndex());
            CoroutineExtensions.WaitAFrame(() => SelectNode(currentNode, newIndex));
            return;
        }

        if (input.y > 0)
        {
            //remove from composite and add to parent
            return;
        }

        bool firstNode = newIndex == 0;
        Node targetNode = !firstNode ? leafNode[newIndex - 1] : null;
        if (targetNode == null) targetNode = newIndex < leafNode.Count - 1 ? leafNode[newIndex + 1] : null;
        if (targetNode is not CompositeNode compositeNode) return;
        //add to composite
        if (nodesTree.Count < 2) leafNode.Remove(node);
        else
        {
            //check once nested composite becomes available
            var parentNode = nodesTree[^2][selectionTree[^2]] as CompositeNode;
            parentNode?.RemoveSubnode(node, compositeNode.LayoutGroup);
        }

        newIndex = compositeNode.subNodes.Count;
        compositeNode.AddSubnode(node);
        if (!firstNode) currentSelection--;
        nodesTree.Add(compositeNode.subNodes);
        selectionTree.Add(newIndex);
        Debug.Log(currentSelection);
        // node.transform.SetAsFirstSibling();
        CoroutineExtensions.WaitAFrame(() => SelectNode(node));
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
