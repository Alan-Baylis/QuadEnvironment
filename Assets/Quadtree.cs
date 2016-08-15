using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Quadtree : MonoBehaviour {

    public static int MAX_LEVELS = 8;
    public TreeNode TopLevelNode;
    public Rect bounds;

    public delegate void TreeChangedHandler();
    public event TreeChangedHandler TreeDidChange;

    void Start()
    {
        TopLevelNode = new TreeNode(0, bounds, 0);
    }
    void Update()
    {
    }
    public void clear()
    {
        TopLevelNode.clear();
    }
    public void SetValue(int newType, Vector2 pos, int reqLevel)
    {
        TopLevelNode.SetValue(newType, pos, reqLevel);
        TreeDidChange();
    }

    public void DoCircleAction(Vector2 Center, float Radius, int level)
    {
        TopLevelNode.DoCircleAction(Center, Radius, level, 1);
        TreeDidChange();
    }
  
}
