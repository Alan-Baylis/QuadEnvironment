using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Quadtree : MonoBehaviour {

    private int MAX_OBJECTS = 1;
   // public int MAX_LEVELS = 8;
    List<Vector3> newVertices;
    //Vector2[] newUV;
    List<int> newTriangles;
    List<Color> colors;
    public TreeNode TopLevelNode;
    public Rect bounds;
    public MeshFilter meshfilter;
    Mesh mesh;
    public MeshRenderer meshRenderer;
    enum NODEINDEX
    {
        TopLeft = 0,
        TopRight,
        BottomRight,
        BottomLeft,
        Bad
    }
    void Start()
    {
        TopLevelNode = new TreeNode(0, bounds, 0);
        Mesh mesh = new Mesh();
        meshfilter.mesh = mesh;
        meshRenderer.material.shader = Shader.Find("Particles/Alpha Blended");
    }

    // Update is called once per frame
    void Update()
    {
        
     
    }
    public void clear()
    {
        TopLevelNode.clear();
    }
    public void SetValue(int newType, Vector2 pos, int reqLevel)
    {

        //TopLevelNode.GetValue(pos,reqLevel)
        TopLevelNode.SetValue(newType, pos, reqLevel);
        RegenerateMesh();
    }

    void DoCircleAction(Quadtree q, Vector2 Center, float Radius, int level)
    {
        /*
        if(IsNodeContainedByCircle(Center,Radius,q))
        {

        }
        */
                
    }
    void DrawTree(TreeNode node)
    {
        if (node.isLeaf())
        {
            MakeQuad(node);
            return;
        }
        foreach (TreeNode q in node.nodes)
        {
            if (q != null)
            {
                DrawTree(q);
            }
        }
    }
    void RegenerateMesh()
    {
        int[,] map = new int[200, 200];
        float squaresize = bounds.width / map.GetLength(0);
        for(int x = 0; x < map.GetLength(0); x++)
        {
            for(int y = 0; y < map.GetLength(1);y++)
            {
                float spaceX = squaresize * x;
                float spaceY = squaresize * y;
                int val = TopLevelNode.GetValue(new Vector2(spaceX, spaceY), 8).typeindex;
                map[x, y] = val;
            }
        }
        
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, squaresize, new Vector3(0,0,1200));

        newVertices = new List<Vector3>();
        newTriangles = new List<int>();
        colors = new List<Color>();
        DrawTree(TopLevelNode);
        Mesh m = meshfilter.mesh;
        m.Clear();
        m.vertices = newVertices.ToArray();
        //   mesh.uv = newUV;
        m.triangles = newTriangles.ToArray();
        m.colors = colors.ToArray();
        
        
    }
    
    void MakeQuad(TreeNode node)
    {
        Rect bounds = node.bounds;
        float space = 0f;
        newVertices.Add(new Vector3(bounds.x + space, bounds.y + space));
        newVertices.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + space));
        newVertices.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + bounds.height - space));
        newVertices.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + bounds.height - space));
        newVertices.Add(new Vector3(bounds.x + space, bounds.y + bounds.height - space));
        newVertices.Add(new Vector3(bounds.x + space, bounds.y + space));

        int start = newVertices.Count - 6;
        for (int i = start; i < start + 6; i++)
        {
            if (node.TreeNodeData.typeindex == 0)
            {
                colors.Add(new Color(0,0,0,0));
            }
            else
            {
                colors.Add(Color.gray);
            }
            newTriangles.Add(i);
        }

    }
}
