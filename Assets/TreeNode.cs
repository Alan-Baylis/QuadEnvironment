using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TreeNode
{

    public class QuadtreeNodeData
    {
       public int typeindex = 0;
        public int isSurface = 0;
        public QuadtreeNodeData()
        {
            typeindex = 0;
             isSurface = 0;
        }
        public QuadtreeNodeData(int t)
        {
            typeindex = t;
            isSurface = 0;
        }

    }

    public int level;
    public List<object> objects;
    public QuadtreeNodeData TreeNodeData;
    public TreeNode parent;
    public TreeNode[] nodes;
    public Rect bounds;
    enum NODEINDEX
    {
        TopLeft = 0,
        TopRight,
        BottomRight,
        BottomLeft,
        Bad
    }
    public TreeNode(int Level, Rect Bounds, int t)
    {
        level = Level;
        bounds = Bounds;
        objects = new List<object>();
        nodes = new TreeNode[4];
        TreeNodeData = new QuadtreeNodeData(t);
    }
    public void clear()
    {
        objects.Clear();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].clear();
                nodes[i] = null;
            }
        }
    }
    public void split()
    {

        if (nodes[0] == null)
        {

            float subWidth = (float)(bounds.width / 2);
            float subHeight = (float)(bounds.width / 2);
            float x = (float)bounds.x;
            float y = (float)bounds.y;

            nodes[0] = new TreeNode(level + 1, new Rect(x, y + subHeight, subWidth, subHeight), TreeNodeData.typeindex);
            nodes[1] = new TreeNode(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), TreeNodeData.typeindex);
            nodes[2] = new TreeNode(level + 1, new Rect(x + subWidth, y, subWidth, subHeight), TreeNodeData.typeindex);
            nodes[3] = new TreeNode(level + 1, new Rect(x, y, subWidth, subHeight), TreeNodeData.typeindex);
            nodes[0].parent = this;
            nodes[1].parent = this;
            nodes[2].parent = this;
            nodes[3].parent = this;
        }
        TreeNodeData.typeindex = -1;
    }

    public bool isLeaf()
    {
        return nodes[0] == null;
    }
    public void SetValue(int newType, Vector2 pos, int reqLevel)
    {
        reqLevel = Mathf.Min(reqLevel, Quadtree.MAX_LEVELS);
        reqLevel = Mathf.Max(reqLevel, 1);
        if (TreeNodeData.typeindex == newType)
        {
            return;
        }
        if (nodes[0] == null)
        {
            split();
        }
        for (int i = 0; i < 4; i++)
        {

            if (nodes[i].bounds.Contains(pos))
            {
                if (nodes[i].level == reqLevel)
                {
                    nodes[i].DoChangeValue(newType);
                    TryMergeIntoNeighbors(); //merge up since this is a single change
                    break;
                }
                else
                {
                    nodes[i].SetValue(newType, pos, reqLevel);
                    break;
                }
            }
        }
    }

    public QuadtreeNodeData GetValue(Vector2 pos, int maxlevel)
    {
        maxlevel = Mathf.Min(maxlevel, Quadtree.MAX_LEVELS);
        maxlevel = Mathf.Max(maxlevel, 1);
        if (nodes[0] == null)
        {
            return TreeNodeData;
        }
        for (int i = 0; i < 4; i++)
        {
            if (nodes[i].bounds.Contains(pos))
            {
                if (nodes[i].level >= maxlevel)
                {
                    return nodes[i].TreeNodeData;
                }
                else
                {
                   return nodes[i].GetValue(pos, maxlevel);
                }
            }
        }
        return new QuadtreeNodeData(0);
    }



    void DoChangeValue(int newType)
    {
        clear();
        TreeNodeData.typeindex = newType;
    }
    void TryMergeIntoNeighbors()
    {
        if (level < 1)
        {
            return;
        }
        bool doreduce = true;
        for (int i = 0; i < 4; i++)
        {
            if (parent.nodes[i].TreeNodeData.typeindex != TreeNodeData.typeindex)
            {
                doreduce = false;
            }
        }
        if (doreduce)
        {
            parent.DoMergeIntoNeighbors(TreeNodeData.typeindex);

        }
    }
    void DoMergeIntoNeighbors(int t)
    {
        nodes[0] = null;
        nodes[1] = null;
        nodes[2] = null;
        nodes[3] = null;
        TreeNodeData.typeindex = t;
        TryMergeIntoNeighbors();
    }
 
    public void DoCircleAction(Vector2 Center, float Radius, int reqlevel, int newType)
    {
        if (level < reqlevel)
        {
            if (nodes[0] == null)
            {
                split();
            }
            for (int i = 0; i < 4; i++)
            {
                if (nodes[i].IsIntersectingCircle(Center, Radius))
                {
                    nodes[i].DoCircleAction(Center, Radius, reqlevel, newType);
                }
            }
        }

        //the current node is at the correct level
        if (level >= reqlevel)
        {
            if (this.IsIntersectingCircle(Center, Radius))
            {
                DoChangeValue(newType);
              //  TryMergeIntoNeighbors();
            }
        }
    }
    public void GetIntersectingNodes(Vector2 Center, float Radius, int reqlevel, int newType,List<TreeNode> IntersectingNodes)
    {
       
    }
    bool IsContainedByCircle(Vector2 Center, float Radius)
    {
        //check if farthest point is contained in circle
        Rect r = bounds;
        float dx = Mathf.Max(Center.x - r.xMin, r.xMax - Center.x);
        float dy = Mathf.Min(Center.y - r.yMax, r.yMin - Center.y);
        return Radius * Radius >= dx * dx + dy * dy;
    }
    bool IsIntersectingCircle(Vector2 Center, float Radius)
    {
        Rect r = bounds;
        if(r.Contains(Center))
        {
            //if circle center is within rectangle, skip the line-circle test
            return true;
        }
        Vector2 TL = new Vector2(r.xMin, r.yMax);
        Vector2 TR = new Vector2(r.xMax, r.yMax);
        Vector2 BL = new Vector2(r.xMin, r.yMin);
        Vector2 BR = new Vector2(r.xMax, r.yMin);
        if(DoesSegmentIntersectCircle(TL,TR,Center,Radius))
        {
            return true;
        }
        if (DoesSegmentIntersectCircle(TL, BL, Center, Radius))
        {
            return true;
        }
        if (DoesSegmentIntersectCircle(BR, TR, Center, Radius))
        {
            return true;
        }
        if (DoesSegmentIntersectCircle(BR, BL, Center, Radius))
        {
            return true;
        }
        if(IsContainedByCircle(Center,Radius))
        {
            return true;
        }
        return false;
    }
    bool DoesSegmentIntersectCircle(Vector2 start, Vector2 end, Vector2 center, float Radius)
    {
        Vector2 DirSeg = end - start;
        Vector2 DirCircle_Start = start - center;

        float a = Vector2.Dot(DirSeg, DirSeg);
        float b = Vector2.Dot(2 * DirCircle_Start, DirSeg);
        float c = Vector2.Dot(DirCircle_Start, DirCircle_Start) - (Radius * Radius);
        float disc = b * b - 4 * a * c;

        if(disc < 0)
        {
            return false;
        }
        else
        {
            disc = Mathf.Sqrt(disc);
            float t1 = (-b - disc) / (2 * a);
            float t2 = (-b + disc) / (2 * a);
            if (t1 >= 0 && t1 <= 1)
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                return true;
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if (t2 >= 0 && t2 <= 1)
            {
                // ExitWound
                return true;
            }

            // no intn: FallShort, Past, CompletelyInside
            return false;
        }
     
    }
}
