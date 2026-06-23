using System;
using LenixSO.Sequences;
using UnityEngine;

public class CompositeNode : Node
{
    [SerializeField] private RectTransform layoutGroup;
    
    public RectTransform LayoutGroup => layoutGroup;
    
    public void AddSubnode(Node subnode)
    {
        subnode.rectTransform.SetParent(layoutGroup);
    }
}