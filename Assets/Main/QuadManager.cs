using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadManager : MonoBehaviour {

    public GameObject cornerLT, cornerRT, cornerLB, cornerRB, cameraTarget;

    int totalNormals = 3;
    int totalOutlines = 8;
    LineRenderer[] lrNormals;
    LineRenderer[] lrZoneOutline;
    GameObject[] AllLines;

    [SerializeField]
    [Range(50.0f, 400.0f)]
    public float ZoneHeight = 150.0f;
    private float _zoneHeight = 150.0f;

    void OnValidate()
    {
        if (_zoneHeight != ZoneHeight)
        {
            _zoneHeight = ZoneHeight;
            DrawZoneOutline();
        }
    }

    float NormalsHeight = 50.0f;

    Vector3 AverageNormal = Vector3.up;

    Vector3[] fatMeshVertices;
    public Quaternion QuadSpaceRotation;

    private Transform transformLT, transformRT, transformLB, transformRB;
    public Transform quadCenter;
    Mesh mesh;

    void Start () {

        SetupLineRenderers();

        transformLT = cornerLT.GetComponent<Transform>();
        transformLB = cornerLB.GetComponent<Transform>();
        transformRT = cornerRT.GetComponent<Transform>();
        transformRB = cornerRB.GetComponent<Transform>();
        quadCenter = cameraTarget.GetComponent<Transform>();
        
        mesh = this.gameObject.GetComponent<MeshFilter>().mesh =  MakeFatQuad();
        UpdateQuadMesh();
    }

    Mesh MakeFatQuad()
    {
        Mesh fat = new Mesh();

        var p1 = new Vector3(-100, -1, -100);
        var p2 = new Vector3(100, -1, -100);
        var p3 = new Vector3(-100, 1, -100);
        var p4 = new Vector3(-100, -1, 100);
        var p5 = new Vector3(100, -1, 100);
        var p6 = new Vector3(100, 1, -100);
        var p7 = new Vector3(-100, 1, 100);
        var p8 = new Vector3(100, 1, 100);

        fatMeshVertices = new[] { p1, p2, p3, p4, p5, p6, p7, p8 };
        int[] triangles = new[]
        {
            0,2,1, 1,2,5,
            3,0,1, 1,4,3,
            0,3,2, 2,3,6,
            1,5,4, 5,7,4,
            6,3,4, 6,4,7,
            6,5,2, 7,5,6
        };

        fat.vertices = fatMeshVertices;
        fat.triangles = triangles;
        return fat;
    }

    LineRenderer LineToKinect;

    void SetupLineRenderers()
    {
        lrNormals = new LineRenderer[totalNormals];
        lrZoneOutline = new LineRenderer[totalOutlines];
        LineToKinect = new LineRenderer();

        int totalLines = totalNormals + totalOutlines + 1;
        AllLines = new GameObject[totalLines];

        // CREATE AN EMPTY GAMEOBJECT TO HOLD EACH LINERENDERER
        for ( int i = 0; i < totalLines; i++ )
        {
            AllLines[i] = new GameObject();
        }

        int linesIndex = 0;

        for ( int i = 0; i < totalNormals; i++ )
        {
            lrNormals[i] = AllLines[linesIndex].AddComponent<LineRenderer>();
            AllLines[linesIndex++].name = "Normal_Line" + i.ToString();

            lrNormals[i].material = new Material(Shader.Find("Particles/Additive"));
            lrNormals[i].SetColors(Color.yellow, Color.yellow);
            lrNormals[i].SetWidth(2.0f, 2.0f);
            lrNormals[i].SetVertexCount(2);
            lrNormals[i].SetPosition(0, new Vector3(0, 0, 500));
            lrNormals[i].SetPosition(1, new Vector3( (i+1)*50, 100, 500));
        }

        lrNormals[0].SetColors(Color.white, Color.red);
        lrNormals[0].SetWidth(4.0f, 4.0f);
        lrNormals[0].SetPosition(1, new Vector3(0, 200, 500));

        Color ZoneOutlineColor = new Color(111.0f / 255.0f, 196.0f / 255.0f, 240.0f / 255.0f);

        for (int i = 0; i < totalOutlines; i++)
        {
            lrZoneOutline[i] = AllLines[linesIndex].AddComponent<LineRenderer>();
            AllLines[linesIndex++].name = "ZoneOutline_Line" + i.ToString();

            lrZoneOutline[i].material = new Material(Shader.Find("Particles/Additive"));
            lrZoneOutline[i].SetColors(ZoneOutlineColor, ZoneOutlineColor);
            lrZoneOutline[i].SetWidth(2.0f, 2.0f);
            lrZoneOutline[i].SetVertexCount(2);
            lrZoneOutline[i].SetPosition(0, new Vector3(0, 0, 500));
            lrZoneOutline[i].SetPosition(1, new Vector3(-80 + (i*10), 10, 400));
        }

        LineToKinect = AllLines[linesIndex].AddComponent<LineRenderer>();
        AllLines[linesIndex].name = "LineToKinect";
        LineToKinect.material = new Material(Shader.Find("Particles/Additive"));
        LineToKinect.SetColors(ZoneOutlineColor, ZoneOutlineColor);
        LineToKinect.SetWidth(3.0f, 3.0f);
        LineToKinect.SetPosition(0, new Vector3(0, 0, 0));
        LineToKinect.SetPosition(1, new Vector3(0, 0, 100));
    }

    void Update () {

        if ( DragCorners3D.isDirty == true )
        {
            DragCorners3D.isDirty = false;
            UpdateQuadMesh();
        }
       
    }

    public void UpdateQuadMesh()
    {

        CalculateNormals();

        Vector3 quadHeightNormal = AverageNormal;
        quadHeightNormal.Scale(new Vector3(.05f, .05f, .05f));

        fatMeshVertices[0] = transformLB.position;
        fatMeshVertices[1] = transformRB.position;
        fatMeshVertices[2] = transformLB.position + quadHeightNormal;
        fatMeshVertices[3] = transformLT.position;

        fatMeshVertices[4] = transformRT.position;
        fatMeshVertices[5] = transformRB.position + quadHeightNormal;
        fatMeshVertices[6] = transformLT.position + quadHeightNormal;
        fatMeshVertices[7] = transformRT.position + quadHeightNormal;

        mesh.vertices = fatMeshVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        DrawZoneOutline();
    }

    public void RotationReset()
    {
        transformRT.position = originalRT;
        transformRB.position = originalRB;
        transformLT.position = originalLT;
        transformLB.position = originalLB;
        UpdateQuadMesh();
    }

    public void NormalizeToZone()
    {
        originalLB = transformLB.position;
        originalLT = transformLT.position;
        originalRB = transformRB.position;
        originalRT = transformRT.position;

        RenderWithNormalizedRotation();
        UpdateQuadMesh();
    }

    Vector3 originalRT, originalRB, originalLT, originalLB;

    void RenderWithNormalizedRotation()
    {
        transformRT.position = QuadSpaceRotation * transformRT.position;
        transformRB.position = QuadSpaceRotation * transformRB.position;
        transformLT.position = QuadSpaceRotation * transformLT.position;
        transformLB.position = QuadSpaceRotation * transformLB.position;
    }

    void DrawZoneOutline()
    {
        Vector3 zoneHeightNormal = AverageNormal.normalized;
        zoneHeightNormal.Scale(new Vector3(_zoneHeight, _zoneHeight, _zoneHeight));

        lrZoneOutline[0].SetPosition(0, fatMeshVertices[0]);
        lrZoneOutline[0].SetPosition(1, fatMeshVertices[0] + zoneHeightNormal);
        lrZoneOutline[1].SetPosition(0, fatMeshVertices[1]);
        lrZoneOutline[1].SetPosition(1, fatMeshVertices[1] + zoneHeightNormal);
        lrZoneOutline[2].SetPosition(0, fatMeshVertices[3]);
        lrZoneOutline[2].SetPosition(1, fatMeshVertices[3] + zoneHeightNormal);
        lrZoneOutline[3].SetPosition(0, fatMeshVertices[4]);
        lrZoneOutline[3].SetPosition(1, fatMeshVertices[4] + zoneHeightNormal);

        lrZoneOutline[4].SetPosition(0, fatMeshVertices[0] + zoneHeightNormal);
        lrZoneOutline[4].SetPosition(1, fatMeshVertices[1] + zoneHeightNormal);
        lrZoneOutline[5].SetPosition(0, fatMeshVertices[1] + zoneHeightNormal);
        lrZoneOutline[5].SetPosition(1, fatMeshVertices[4] + zoneHeightNormal);
        lrZoneOutline[6].SetPosition(0, fatMeshVertices[4] + zoneHeightNormal);
        lrZoneOutline[6].SetPosition(1, fatMeshVertices[3] + zoneHeightNormal);
        lrZoneOutline[7].SetPosition(0, fatMeshVertices[3] + zoneHeightNormal);
        lrZoneOutline[7].SetPosition(1, fatMeshVertices[0] + zoneHeightNormal);
    }

    void CalculateNormals()
    {
        // RIGHT SIDE TRIANGLE NORMAL
        // ---------------------------------------------------------------
        Vector3 Rside1 = transformRB.position - transformRT.position;
        Vector3 Rside2 = transformLT.position - transformRT.position;
        Vector3 RNormal = Vector3.Cross(Rside1, Rside2).normalized;

        // FIND MID-POINT OF SIDES TO DETERMINE A CENTER APPROXIMATION
        Rside1.Scale(new Vector3(.5f, .5f, .5f));
        Rside2.Scale(new Vector3(.5f, .5f, .5f));

        Vector3 RNormalStart = transformRT.position + Rside1 + Rside2;
        lrNormals[1].SetPosition(0, RNormalStart);

        // SCALING THE NORMAL TO THE VISIBLE RANGE OF OUR SCENE
        RNormal.Scale(new Vector3(NormalsHeight, NormalsHeight, NormalsHeight));
        Vector3 RNormalEnd = RNormalStart + RNormal;
        lrNormals[1].SetPosition(1, RNormalEnd);

        // LEFT SIDE TRIANGLE NORMAL
        // ---------------------------------------------------------------
        Vector3 Lside1 = transformRT.position - transformLT.position;
        Vector3 Lside2 = transformLB.position - transformLT.position;
        Vector3 LNormal = Vector3.Cross(Lside1, Lside2).normalized;

        // FIND MID-POINT OF SIDES TO DETERMINE A CENTER APPROXIMATION
        Lside1.Scale(new Vector3(.5f, .5f, .5f));
        Lside2.Scale(new Vector3(.5f, .5f, .5f));

        Vector3 LNormalStart = transformLT.position + Lside1 + Lside2;
        lrNormals[2].SetPosition(0, LNormalStart);

        // SCALING THE NORMAL TO THE VISIBLE RANGE OF OUR SCENE
        LNormal.Scale(new Vector3(NormalsHeight, NormalsHeight, NormalsHeight));
        Vector3 LNormalEnd = LNormalStart + LNormal;
        lrNormals[2].SetPosition(1, LNormalEnd);

        // AVERAGE NORMAL & CENTER
        // ---------------------------------------------------------------
        Vector3 CenterAverage = RNormalStart + LNormalStart;
        CenterAverage.Scale(new Vector3(.5f, .5f, .5f));
        quadCenter.position = CenterAverage;

        AverageNormal = RNormal + LNormal;
        lrNormals[0].SetPosition(0, quadCenter.position);
        lrNormals[0].SetPosition(1, quadCenter.position + AverageNormal);

        LineToKinect.SetPosition(1, quadCenter.position);

        QuadSpaceRotation = Quaternion.FromToRotation(AverageNormal.normalized, new Vector3(0, 1, 0));

    }


}
