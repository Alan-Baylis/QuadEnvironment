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
    private int MAX_OBJECTS = 1;
    public static int MAX_LEVELS = 50;

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
        reqLevel = Mathf.Min(reqLevel, MAX_LEVELS);
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
                    nodes[i].TryMergeIntoNeighbors();
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
        maxlevel = Mathf.Min(maxlevel, MAX_LEVELS);
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

   

    void DoCircleAction(Quadtree q, Vector2 Center, float Radius, int level)
    {
        if (IsNodeContainedByCircle(Center, Radius, q))
        {

        }

    }
    bool IsNodeContainedByCircle(Vector2 Center, float Radius, Quadtree q)
    {
        Rect r = q.bounds;
        float dx = Mathf.Max(Center.x - r.xMin, r.xMax - Center.x);
        float dy = Mathf.Min(Center.y - r.yMax, r.yMin - Center.y);
        return Radius * Radius >= dx * dx + dy * dy;
    }
    /*
    void RecalculateSurfaceNodes()
    {
        
        if (nodes[0] == null)
        {
            return;
        }
        TreeNodeData.isSurface = 0;

        for (int i = 0; i < 4; i++)
        {
            if (nodes[i].TreeNodeData.typeindex == 1)
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
     
    }
    private NODEINDEX getIndex(Rect pRect)
    {
        NODEINDEX index = NODEINDEX.Bad;

        double verticalMidpoint = bounds.center.x;
        double horizontalMidpoint = bounds.center.y;
        //todo bottom = !top;
        // Object can completely fit within the top quadrants
        bool topQuadrant = (pRect.y > horizontalMidpoint);
        // Object can completely fit within the bottom quadrants
        bool bottomQuadrant = (pRect.y < horizontalMidpoint && pRect.y + pRect.height < horizontalMidpoint);

        // Object can completely fit within the left quadrants
        if (pRect.x < verticalMidpoint && pRect.x + pRect.width < verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = NODEINDEX.TopLeft;
            }
            else if (bottomQuadrant)
            {
                index = NODEINDEX.BottomLeft;
            }
        }
        // Object can completely fit within the right quadrants
        else if (pRect.x > verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = NODEINDEX.TopRight;
            }
            else if (bottomQuadrant)
            {
                index = NODEINDEX.BottomRight;
            }
        }

        return index;
    }
    public void insert(Rect pRect)
    {
        //Has this node already been split?
        if (nodes[0] != null)
        {
            NODEINDEX index = getIndex(pRect);

            if (index != NODEINDEX.Bad)
            {
                //try to insert one level deeper
                nodes[(int)index].insert(pRect);
                return;
            }
        }

        //This node has not yet been split, so add the object to the object list
        objects.Add(pRect);

        //Does this object break the MAX_OBJECTS constant?
        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            //Split the tree 
            // if (nodes[0] == null)
            // {
            split();
            // }

            int i = 0;
            while (i < objects.Count)
            {
                int ind = (int)getIndex((Rect)objects[i]);
                if (ind != (int)NODEINDEX.Bad)
                {
                    nodes[ind].insert((Rect)objects[i]);

                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }
*/
}
