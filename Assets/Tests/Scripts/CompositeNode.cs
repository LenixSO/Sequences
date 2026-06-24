using System;
using System.Collections.Generic;
using LenixSO.Sequences;
using LenixSO.Sequences.Decorator;
using UnityEngine;

public class CompositeNode : Node
{
    [SerializeField] private RectTransform layoutGroup;

    public int minNodes = 0;
    
    public RectTransform LayoutGroup => layoutGroup;

    public override ISequence nodeSequence
    {
        get
        {
            bool canConstruct = subNodes.Count >= minNodes;
            canConstruct &= sequenceConstructor != null;
            return InjectSequence(canConstruct ? sequenceConstructor() : backupSequence);
        }
        protected set => backupSequence = value;
    }

    public event Action OnSubnodeChanged;
    
    private ISequence backupSequence;

    public List<Node> subNodes = new();

    private Func<ISequence> sequenceConstructor;

    public void Setup(Func<ISequence> nodeSequenceFactory)
    {
        sequenceConstructor = nodeSequenceFactory;
    }

    public override ISequence InjectSequence(ISequence sequence)
    {
        var observer = new ObserverSequence(sequence, SetRunningColor);
        observer.OnFinished += SetFinishedColor;
        backupSequence ??= observer;
        return observer;
    }

    public void AddSubnode(Node subnode)
    {
        subNodes.Add(subnode);
        subnode.rectTransform.SetParent(layoutGroup);
        subnode.transform.SetAsFirstSibling();
        subnode.rectTransform.localScale = Vector3.one;
        OnSubnodeChanged?.Invoke();
    }

    public void RemoveSubnode(Node subnode, Transform newParent)
    {
        subNodes.Remove(subnode);
        subnode.rectTransform.SetParent(newParent);
        subnode.rectTransform.localScale = Vector3.one;
        OnSubnodeChanged?.Invoke();
    }

    public override void ResetNode()
    {
        base.ResetNode();
        for (int i = 0; i < subNodes.Count; i++)
            subNodes[i].ResetNode();
    }
}