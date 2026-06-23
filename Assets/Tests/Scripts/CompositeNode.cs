using System;
using System.Collections.Generic;
using LenixSO.Sequences;
using UnityEngine;

public class CompositeNode : Node
{
    [SerializeField] private RectTransform layoutGroup;
    
    public RectTransform LayoutGroup => layoutGroup;
    
    public List<Node> subNodes = new();

    private Action<Node> addSequenceMethod;
    private Action<Node> removeSequenceMethod;

    public void Setup(Action<Node> addSequenceAction, Action<Node> removeSequenceAction)
    {
        addSequenceMethod = addSequenceAction;
        removeSequenceMethod = removeSequenceAction;
    }

    public void AddSubnode(Node subnode)
    {
        subNodes.Add(subnode);
        addSequenceMethod(subnode);
        subnode.rectTransform.SetParent(layoutGroup);
        subnode.rectTransform.localScale = Vector3.one;
    }

    public void RemoveSubnode(Node subnode, Transform newParent)
    {
        subNodes.Remove(subnode);
        removeSequenceMethod(subnode);
        subnode.rectTransform.SetParent(newParent);
        subnode.rectTransform.localScale = Vector3.one;
    }

    public override void ResetNode()
    {
        base.ResetNode();
        for (int i = 0; i < subNodes.Count; i++)
            subNodes[i].ResetNode();
    }
}