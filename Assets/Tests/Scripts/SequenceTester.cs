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
using UnityEngine.UI;
using Input = InputSystemHelper.Input;

public class SequenceTester : MonoBehaviour
{
    private static SequenceTester instance;
    
    [SerializeField] private LayoutGroup parent;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private CompositeNode hCompositeNode;
    [SerializeField] private CompositeNode vCompositeNode;

    [SerializeField] private Node selectorNode;

    private int currentSelection
    {
        get => selectionTree[^1];
        set => selectionTree[^1] = value;
    }
    private readonly List<Node> nodes = new();
    private readonly List<int> selectionTree = new() { 0 };
    private readonly List<List<Node>> nodesTree = new();
    private List<Node> leafNodes => nodesTree[^1];

    private void Awake()
    {
        instance = this;
        Input.Map("UI").Action("Navigate").performed += OnNavigate;
        CreateGenericNode();
        CreateFollowUpNode();
        CreateParallelNode(2);
        CreateGenericNode();
        CoroutineExtensions.AwaitCoroutine(CoroutineExtensions.DelayCoroutine(.02f),
            () => SelectNode(nodes[0], 0));
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
        var node = Instantiate(nodePrefab, Vector3.zero, Quaternion.identity, parent.transform);
        node.transform.SetAsFirstSibling();
        ISequence genericSequence = new CoroutineSequence(new(() => CoroutineExtensions.DelayCoroutine(delay)));
        node.InjectSequence(genericSequence);
        return node;
    }

    private CompositeNode BaseCompositeNode(CompositeNode prefab)
    {
        if (nodes.Count == 0) CreateSelector();
        var node = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent.transform);
        node.transform.SetAsFirstSibling();
        node.InjectSequence(CustomSequence.EmptySequence());
        return node;
    }

    private CompositeNode HorizontalNode()
    {
        var node = BaseCompositeNode(hCompositeNode);
        float baseWidth = nodePrefab.rectTransform.sizeDelta.x * node.LayoutGroup.localScale.x;
        node.OnSubnodeChanged += ResizeNode;
        ResizeNode();
        return node;
        void ResizeNode()
        {
            float subWidth = 0;
            for (int i = 0; i < node.subNodes.Count; i++)
                subWidth += node.subNodes[i].rectTransform.sizeDelta.x * node.LayoutGroup.localScale.x;
            subWidth = Mathf.Max(baseWidth, subWidth);
            subWidth += 110 + 10;
            var size = node.rectTransform.sizeDelta;
            size.x = subWidth;
            node.rectTransform.sizeDelta = size;
        }
    }

    private void CreateGenericSubnodes(CompositeNode node, int subnodes)
    {
        for (int i = 0; i < subnodes; i++)
        {
            float delay = .5f + .5f * i;
            var subnode = GenericNode(delay);
            subnode.Text.SetText(i.ToString());
            node.AddSubnode(subnode);
        }
    }

    private CompositeNode ParallelNode(int subnodes)
    {
        if (nodes.Count == 0) CreateSelector();
        var node = BaseCompositeNode(vCompositeNode);
        node.Setup(() =>
        {
            ParallelSequences sequence = new();
            for (int i = 0; i < node.subNodes.Count; i++)
                sequence.Add(node.subNodes[i].nodeSequence);
            return sequence;
        });
        CreateGenericSubnodes(node, subnodes);
        return node;
    }

    private CompositeNode QueuedNode(int subnodes)
    {
        if (nodes.Count == 0) CreateSelector();
        var node = HorizontalNode();
        node.Setup(() =>
        {
            QueuedSequences sequence = new();
            for (int i = 0; i < node.subNodes.Count; i++)
                sequence.Add(node.subNodes[i].nodeSequence);
            return sequence;
        });
        CreateGenericSubnodes(node, subnodes);
        return node;
    }

    private CompositeNode FollowUpNode()
    {
        var node = BaseCompositeNode(vCompositeNode);
        node.minNodes = 2;
        node.Setup(() => new FollowUpSequence(node.subNodes[0].nodeSequence, node.subNodes[1].nodeSequence));
        return node;
    }
    #endregion

    #region Selector
    private void CreateSelector()
    {
        selectorNode.rectTransform.localScale = Vector3.one * 1.5f;
        selectorNode.Text.text = string.Empty;
        selectorNode.SetRunningColor();
        nodesTree.Add(nodes);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if (direction == Vector2.zero) return;
        Vector2Int input = new Vector2Int((int)direction.x, (int)direction.y);
        bool alt = Keyboard.current.shiftKey.isPressed;

        int newIndex = (leafNodes.Count + currentSelection + input.x) % leafNodes.Count;
        Node node = leafNodes[newIndex];
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
                node = leafNodes[newIndex];
            }
        }

        SelectNode(node, newIndex);
    }

    private void MoveNode(Vector2Int input, Node node, int newIndex)
    {
        if (input.y == 0)
        {
            if (Mathf.Abs(newIndex - currentSelection) > 1) return;
            Node currentNode = leafNodes[currentSelection];
            (leafNodes[currentSelection], leafNodes[newIndex]) = (leafNodes[newIndex], currentNode);
            node.transform.SetSiblingIndex(currentNode.transform.GetSiblingIndex());
            SelectNode(currentNode, newIndex);
            return;
        }

        if (input.y > 0)
        {
            if (nodesTree.Count < 2) return;
            //remove from composite and add to parent
            if (nodesTree[^2][selectionTree[^2]] is CompositeNode parentNode)
            {
                //still needs to check for parent of parent!!!!!!
                parentNode.RemoveSubnode(node, parent.transform);
                if (nodesTree.Count < 3)
                {
                    nodes.Insert(selectionTree[^2] + 1, node);
                    node.transform.SetSiblingIndex(parentNode.transform.GetSiblingIndex());
                }
                else
                {
                    var grandparent = nodesTree[^3][selectionTree[^3]] as CompositeNode;
                    grandparent!.AddSubnode(node);
                    grandparent.subNodes.Insert(selectionTree[^2] + 1, node);
                    grandparent.subNodes.RemoveAt(grandparent.subNodes.Count - 1);
                    node.transform.SetSiblingIndex(parentNode.transform.GetSiblingIndex() - 1);
                }
            }
            selectionTree.RemoveAt(selectionTree.Count - 1);
            nodesTree.RemoveAt(nodesTree.Count - 1);
            currentSelection++;
            SelectNode(node, currentSelection);
            return;
        }

        bool firstNode = newIndex == 0;
        Node targetNode = !firstNode ? leafNodes[newIndex - 1] : null;
        if (targetNode == null) targetNode = newIndex < leafNodes.Count - 1 ? leafNodes[newIndex + 1] : null;
        if (targetNode is not CompositeNode compositeNode) return;
        //add to composite
        if (nodesTree.Count < 2) leafNodes.Remove(node);
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
        SelectNode(node);
    }

    private void SelectNode(Node node, int? newIndex = null)
    {
        if (newIndex != null) currentSelection = newIndex.Value;
        StartCoroutine(SelectionDelay(node));
    }

    private IEnumerator SelectionDelay(Node node)
    {
        parent.enabled = true;
        yield return null;
        selectorNode.transform.position = node.transform.position;
        selectorNode.rectTransform.localScale = node.transform.lossyScale * 1.5f;
        selectorNode.rectTransform.sizeDelta = node.rectTransform.sizeDelta;
        yield return null;
        parent.enabled = false;
    }
    #endregion

    private void AddNode(Node node)
    {
        if (nodesTree.Count < 2) nodes.Add(node);
        else
        {
            var compositeNode = nodesTree[^2][selectionTree[^2]] as CompositeNode;
            compositeNode?.AddSubnode(node);
        }
        SelectNode(node, leafNodes.Count - 1);
    }

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
        node.Text.SetText($"{instance.leafNodes.Count}");
        instance.AddNode(node);
    }

    public static void CreateParallelNode(int subnodes)
    {
        var node = instance.ParallelNode(subnodes);
        node.Text.SetText($"P");
        instance.AddNode(node);
    }
    
    public static void CreateQueuedNode(int subnodes)
    {
        var node = instance.QueuedNode(subnodes);
        node.Text.SetText($"Q");
        instance.AddNode(node);
    }

    public static void CreateFollowUpNode()
    {
        var node = instance.FollowUpNode();
        node.Text.SetText($"F");
        instance.AddNode(node);
    }
}
