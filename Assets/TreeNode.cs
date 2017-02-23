using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TreeNode
{

    public class QuadtreeNodeData
    {
        public int mtypeindex = 0;
        public Rect mbounds;

        public QuadtreeNodeData()
        {
           mtypeindex = -1;
        }
        public QuadtreeNodeData(int t, Rect bounds)
        {
            mtypeindex = t;
            mbounds = bounds;
        }

    }

    public int level;
    public QuadtreeNodeData TreeNodeData;
    public TreeNode parent;
    public TreeNode[] children;
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
        children = new TreeNode[4];
        TreeNodeData = new QuadtreeNodeData(t,Bounds);
    }


    public void CollapseChildren(bool force)
    {
        if(force)
        {
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = null;
            }
            return;
        }
       
        if (IdenticalChildren())
        {
            TreeNodeData.mtypeindex = children[0].TreeNodeData.mtypeindex;
            for (int i = 0; i < children.Length; i++)
            {
              //  children[i].CollapseChildren(); <- dont think I need to recursively nullify
                children[i] = null;
            }
        }
    }
    bool IdenticalChildren()
    {
        bool Identical = false;

        if (!isLeaf())
        {
            
            Identical = true; 
            int t = children[0].TreeNodeData.mtypeindex;
            for(int i = 0; i < children.Length; i++)
            {
                if(children[i].TreeNodeData.mtypeindex != t)
                {
                    Identical = false;
                }
                if (children[i].TreeNodeData.mtypeindex == -1)
                {
                    Identical = false;
                }
            }
        }
        return Identical;
    }    
    public void Split()
    {
        if (isLeaf())
        {
            float subWidth = (float)(TreeNodeData.mbounds.width / 2);
            float subHeight = (float)(TreeNodeData.mbounds.width / 2);
            float x = (float)TreeNodeData.mbounds.x;
            float y = (float)TreeNodeData.mbounds.y;
            int newlevel = level + 1;
            int newtype = TreeNodeData.mtypeindex;
            children[0] = new TreeNode(newlevel, new Rect(x, y + subHeight, subWidth, subHeight), newtype);
            children[1] = new TreeNode(newlevel, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), newtype);
            children[2] = new TreeNode(newlevel, new Rect(x + subWidth, y, subWidth, subHeight), newtype);
            children[3] = new TreeNode(newlevel, new Rect(x, y, subWidth, subHeight), newtype);
            children[0].parent = this;
            children[1].parent = this;
            children[2].parent = this;
            children[3].parent = this;
            TreeNodeData.mtypeindex = -1;
        }
    }

    public bool isLeaf()
    {
        return children[0] == null;
    }

    public void SetValue(int newType, Vector2 pos, int reqLevel)
    {
        reqLevel = Mathf.Min(reqLevel, Quadtree.MAX_LEVELS);
        reqLevel = Mathf.Max(reqLevel, 1);
        if (TreeNodeData.mtypeindex == newType)
        {
            return;
        }
        if (children[0] == null)
        {
            Split();
        }
        for (int i = 0; i < children.Length; i++)
        {

            if (children[i].TreeNodeData.mbounds.Contains(pos))
            {
                if (children[i].level == reqLevel)
                {
                    children[i].DoChangeValue(newType);
                    break;
                }
                else
                {
                    children[i].SetValue(newType, pos, reqLevel);
                    break;
                }
            }
        }
      
    }
    
    public QuadtreeNodeData GetValue(Vector2 pos, int maxlevel)
    {
        maxlevel = Mathf.Min(maxlevel, Quadtree.MAX_LEVELS);
        maxlevel = Mathf.Max(maxlevel, 1);
        if(isLeaf())// || level >= maxlevel)
        {
            return TreeNodeData;
        }
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].TreeNodeData.mbounds.Contains(pos))
            {
                return children[i].GetValue(pos, maxlevel);
            }
        }
        return null;
    }

    void DoChangeValue(int newType)
    {
        TreeNodeData.mtypeindex = newType;
        CollapseChildren(true);

    }
 
    public bool DoCircleAction(Vector2 Center, float Radius, int reqlevel, int newType)
    {
        if (IsContainedByCircle(Center, Radius))
        {
            DoChangeValue(newType);
            return true;
        }
        if (IsIntersectingCircle(Center,Radius))
        {
            if (level < reqlevel)
            {
                Split();
            }
            if (!isLeaf())
            {
                if (TreeNodeData.mtypeindex != newType)
                {
                    int changes = 0;
                    for (int i = 0; i < children.Length; i++)
                    {
                        if (children[i].DoCircleAction(Center, Radius, reqlevel, newType))
                        {
                            changes++;
                        }
                    }
                }
                CollapseChildren(false);
            }
          
        }
        return false;
    }
    public void GetIntersectingNodes(Vector2 Center, float Radius, int reqlevel, int newType,List<TreeNode> IntersectingNodes)
    {
       
    }
    bool IsContainedByCircle(Vector2 Center, float Radius)
    {
        //check if farthest point is contained in circle
        Rect r = TreeNodeData.mbounds;
        float dx = Mathf.Max(Center.x - r.xMin, r.xMax - Center.x);
        float dy = Mathf.Min(Center.y - r.yMax, r.yMin - Center.y);
        return Radius * Radius >= dx * dx + dy * dy;
    }
    bool IsIntersectingCircle(Vector2 Center, float Radius)
    {
        Rect r = TreeNodeData.mbounds;
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
