using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
public class Game : MonoBehaviour {

    public Quadtree MainTree;
    void Start ()
    {
    }
	
	void Update ()
    {
        ControlQuadTree();
    }
    int leveltodraw = 1;
    void ControlQuadTree()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            //Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
            //Debug.Log(hit.point);
            if (Input.GetMouseButton(1))
            {
                MainTree.SetValue(1, hit.point, leveltodraw);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            leveltodraw++;
        }
    }

}
