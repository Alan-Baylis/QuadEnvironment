using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MeshGenerator : MonoBehaviour {

    public class MeshData
    {
        public List<Vector3> verts;
        public List<Color> colors;
        public List<int> triangles;
        public MeshData()
        {
            verts = new List<Vector3>();
            colors = new List<Color>();
            triangles = new List<int>();
        }
    }
    public SquareGrid squareGrid;
    public SquareGrid surfaceSquareGrid;

    

    public MeshFilter MeshFilter_Interior;
    public MeshRenderer MeshRenderer_Interior;

    public MeshFilter MeshFilter_Surface;
    public MeshRenderer MeshRenderer_Surface;

    MeshData meshData_Interior;
    MeshData meshData_Surface;
    Mesh mesh_Interior;
    Mesh mesh_Surface;

    void Start()
    {
        MeshRenderer_Interior.material.shader = Shader.Find("Particles/Alpha Blended");
        MeshRenderer_Surface.material.shader = Shader.Find("Particles/Alpha Blended");

        mesh_Interior = new Mesh();
        mesh_Surface = new Mesh();

        MeshFilter_Interior.mesh = mesh_Interior;
        MeshFilter_Surface.mesh = mesh_Surface;
    }

    public void DrawQuadtree(Quadtree Tree)
    {
        GenerateSurfaceMeshData(200, Tree.TopLevelNode);
        GenerateInteriorMeshData(Tree.TopLevelNode);
        GenerateMesh(mesh_Surface, meshData_Surface);
        GenerateMesh(mesh_Interior, meshData_Interior);
    }

    #region Exterior Mesh Generation

    void GenerateSurfaceMeshData(int resolution, TreeNode rootNode)
    {
        meshData_Surface = new MeshData();


        int[,] map = new int[resolution, resolution];
        float squareSize = rootNode.bounds.width / map.GetLength(0);
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                float spaceX = squareSize * x;
                float spaceY = squareSize * y;
                int val = rootNode.GetValue(new Vector2(spaceX, spaceY), 8).typeindex;
                map[x, y] = val;
            }
        }

        SquareGrid squareGrid = new SquareGrid(map, squareSize);
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }
    }
    void AssignVerts_Surface(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = meshData_Surface.verts.Count;
                meshData_Surface.verts.Add(new Vector3(points[i].position.x, points[i].position.y, 0));
                meshData_Surface.colors.Add(Color.green);
            }
        }
    }
    void CreateTriangle_Surface(Node a, Node b, Node c)
    {
        meshData_Surface.triangles.Add(a.vertexIndex);
        meshData_Surface.triangles.Add(b.vertexIndex);
        meshData_Surface.triangles.Add(c.vertexIndex);
    }

    #endregion

    #region Interior Mesh Generation

    void GenerateInteriorMeshData(TreeNode rootNode)
    {
        meshData_Interior = new MeshData();
        GenrateAllInteriorQuads(rootNode);
    }
    void GenrateAllInteriorQuads(TreeNode rootNode)
    {
        if (rootNode.isLeaf())
        {
            GenerateInteriorQuad(rootNode);
            return;
        }
        foreach (TreeNode q in rootNode.nodes)
        {
            if (q != null)
            {
                GenrateAllInteriorQuads(q);
            }
        }
    }
    void GenerateInteriorQuad(TreeNode node)
    {
        Rect bounds = node.bounds;
        float space = 0f;
        meshData_Interior.verts.Add(new Vector3(bounds.x + space, bounds.y + space));
        meshData_Interior.verts.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + space));
        meshData_Interior.verts.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + bounds.height - space));
        meshData_Interior.verts.Add(new Vector3(bounds.x + bounds.width - space, bounds.y + bounds.height - space));
        meshData_Interior.verts.Add(new Vector3(bounds.x + space, bounds.y + bounds.height - space));
        meshData_Interior.verts.Add(new Vector3(bounds.x + space, bounds.y + space));

        int start = meshData_Interior.verts.Count - 6;
        for (int i = start; i < start + 6; i++)
        {
            if (node.TreeNodeData.typeindex == 0)
            {
                meshData_Interior.colors.Add(new Color(0, 0, 0, 0));
            }
            else
            {
                meshData_Interior.colors.Add(Color.gray);
            }
            meshData_Interior.triangles.Add(i);
        }
    }

    #endregion

    void GenerateMesh(Mesh mesh, MeshData data)
    {
        mesh.Clear();
        mesh.vertices = data.verts.ToArray();
        mesh.triangles = data.triangles.ToArray();
        mesh.colors = data.colors.ToArray();
        Debug.Log("NEW MESH");
    }
   
   
    void OnDrawGizmos()
    {
        if (squareGrid == null) return;
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                /*
               Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
               Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 4f);

               Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
               Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 4f);

               Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
               Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 4f);

               Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
               Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 4f);

               Gizmos.color = Color.gray;
               Gizmos.DrawCube(squareGrid.squares[x, y].centerTop.position, Vector3.one * 1.5f);
               Gizmos.DrawCube(squareGrid.squares[x, y].centerRight.position, Vector3.one * 1.5f);
               Gizmos.DrawCube(squareGrid.squares[x, y].centerBottom.position, Vector3.one * 1.5f);
               Gizmos.DrawCube(squareGrid.squares[x, y].centerLeft.position, Vector3.one * 1.5f);
               */
            }
        }
    }

    #region Marching Cube Helpers
    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            //1 point
            case 1:
                MeshFromPoints(square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 2:
                MeshFromPoints(square.centerRight, square.bottomRight, square.centerBottom);
                break;
            case 4:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;

            //2 points
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            //3 points
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            //4 points
            case 15:
                //simple way to only do surface marching cubes
                //  MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }

    }
    void MeshFromPoints(params Node[] points)
    {
        AssignVerts_Surface(points);
        if (points.Length >= 3)
        {
            CreateTriangle_Surface(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle_Surface(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle_Surface(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle_Surface(points[0], points[4], points[5]);
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;
        public SquareGrid (int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int x = 0; x < nodeCountX; x++)
            {
                for(int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(x * squareSize + squareSize / 2, y * squareSize + squareSize / 2,0);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX-1; x++)
            {
                for (int y = 0; y < nodeCountY-1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;
        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;

        }
    }
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;
        public ControlNode(Vector3 _pos, bool _active, float squaresize) : base(_pos)
        {
            active = _active;
            above = new Node(_pos + Vector3.up * (squaresize / 2.0f));
            right = new Node(_pos + Vector3.right * (squaresize / 2.0f));
        }
    }
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;
        public Node(Vector3 pos)
        {
            position = pos;
        }
    }
    #endregion


}
