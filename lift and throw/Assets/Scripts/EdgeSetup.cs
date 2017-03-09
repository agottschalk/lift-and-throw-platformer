using UnityEngine;
using System.Collections;

public class EdgeSetup : MonoBehaviour {
    //set the points you want to use in the editor
    //make sure offset for the edge collider is set to 0
    public Vector2[] edgePoints;

    //displays position of edges when playing
    private LineRenderer line;
    public bool showEdges;
    public Material lineMaterial;
    public Color lineColor;

    private EdgeCollider2D edges;

	// Use this for initialization
	void Start () {
        edges = GetComponent<EdgeCollider2D>();

        edges.points = edgePoints;

        //makes edges show up during gameplay, for testing/debugging
        if(showEdges)
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.SetVertexCount(edgePoints.Length);
            line.SetWidth(0.05f, 0.05f);
            line.material = lineMaterial;
            line.SetColors(Color.cyan, Color.cyan);
            line.useWorldSpace = false;

            if(lineMaterial == null)
            {
                Debug.Log("Please assign a material for the line renderer.", gameObject);
            }

            //line renderer needs Vector3's for points instead of 2's
            for(int i=0; i<edgePoints.Length; i++)
            {
                line.SetPosition(i, new Vector3(edgePoints[i].x, edgePoints[i].y, -1));
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
