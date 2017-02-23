using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
public class Game : MonoBehaviour {

    public Quadtree MainTree;
    void Start ()
    {
        Application.targetFrameRate = 1000;
    }
	
	void Update ()
    {
        ControlQuadTree();
    }
    int leveltodraw = 1;
    void ControlQuadTree()
    {
        if (Input.GetMouseButton(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                MainTree.DoCircleAction(hit.point, 100, leveltodraw);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            leveltodraw++;
        }
    }

}
