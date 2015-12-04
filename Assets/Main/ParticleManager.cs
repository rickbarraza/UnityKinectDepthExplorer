using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour {

    ParticleSystem.Particle[] cloud;
    ParticleSystem particles;

    KinectManager kinectManager = null;
    QuadManager quadManager = null;


    public int totalParticles;

    public bool ClipToZone = false;
    bool isNormalizeToZone = false;

    int width = 256;
    int height = 212;

    public float depthColorResolution = 1000;
    Color[] depthColorLookup;

    public float MaxDepth = 500.0f;
    public float MinDepth = 0.0f;

    float low = 0.5f;
    float high = 0.5f;

    float deltaTime = 0.0f;

    public int gridStep = 2;

    Quaternion normalizedRotation;

    void Start () {

        CreateDepthColorLookup();

        low = MaxDepth;

        if (kinectManager == null)
        {
            GameObject go = GameObject.Find("goKinectManager");
            kinectManager = go.GetComponent<KinectManager>();
        }

        if ( quadManager == null )
        {
            GameObject go = GameObject.Find("goClippingQuad");
            quadManager = go.GetComponent<QuadManager>();
        }

        particles = this.gameObject.GetComponent<ParticleSystem>();

        totalParticles = (kinectManager.depthWidth / gridStep) * (kinectManager.depthHeight / gridStep); // (width / 2) * (height / 2);

        cloud = new ParticleSystem.Particle[totalParticles];

        InvokeRepeating("UpdateKinectMesh", 3.0f, 2.0f);
    }
    
    void CreateDepthColorLookup()
    {
        depthColorLookup = new Color[(int)depthColorResolution];

        Color startColor = new Color(255 / 255.0f, 255 / 255.0f, 255 / 255.0f);
        Color endColor = new Color(0 / 255.0f, 255 / 255.0f, 0 / 255.0f);

        for ( int i = 0; i < depthColorResolution; i++ )
        {
            depthColorLookup[i] = Color.Lerp(startColor, endColor, i / depthColorResolution);
        }
    }

    public void NormalizeToZone(bool isNormalized)
    {
        isNormalizeToZone = isNormalized;
        normalizedRotation = quadManager.QuadSpaceRotation;
        UpdateKinectMesh();

        if ( isNormalized )
        {
            quadManager.NormalizeToZone();
        }
        else
        {
            quadManager.RotationReset();
        }

    }
    Vector2 _lt2d = new Vector2(0,0);
    Vector2 _lb2d = new Vector2(0, 423);
    Vector2 _rt2d = new Vector2(511, 0);
    Vector2 _rb2d = new Vector2(511, 423);

    int highY = 0;
    int lowY = 424;
    int highX = 0;
    int lowX = 512;
        
    public void UpdateCorners(Vector2 lt2d, Vector2 lb2d, Vector2 rt2d, Vector2 rb2d)
    {
        _lt2d = lt2d; _lt2d.y = Mathf.Abs(_lt2d.y);
        _lb2d = lb2d; _lb2d.y = Mathf.Abs(_lb2d.y);
        _rt2d = rt2d; _rt2d.y = Mathf.Abs(_rt2d.y);
        _rb2d = rb2d; _rb2d.y = Mathf.Abs(_rb2d.y);

        highY = highX = 0;
        lowY = 424;
        lowX = 512;

        if (lt2d.x < lowX) lowX = (int)lt2d.x;
        if (lb2d.x < lowX) lowX = (int)lb2d.x;

        if (rt2d.x > highX) highX = (int)rt2d.x;
        if (rb2d.x > highX) highX = (int)rb2d.x;

        if (_lb2d.y < lowY) lowY = (int)_lb2d.y;
        if (_rb2d.y < lowY) lowY = (int)_rb2d.y;

        if (_lt2d.y > highY) highY = (int)_lt2d.y;
        if (_rt2d.y > highY) highY = (int)_rt2d.y;

        Debug.Log("low(x/y)/high(x/y): " + lowX + "," + lowY + "/" + highX + "," + highY);

        if (kinectManager.cameraSpacePoints == null) return;

        Vector3 v3lt = kinectManager.GetCameraSpacePointfromDepthImageXY(lt2d);
        Vector3 v3lb = kinectManager.GetCameraSpacePointfromDepthImageXY(lb2d);
        Vector3 v3rt = kinectManager.GetCameraSpacePointfromDepthImageXY(rt2d);
        Vector3 v3rb = kinectManager.GetCameraSpacePointfromDepthImageXY(rb2d);

        if ( !float.IsNegativeInfinity(v3lt.sqrMagnitude) && !float.IsPositiveInfinity(v3lt.sqrMagnitude))
        {
            quadManager.cornerLT.transform.position = v3lt * 1000.0f;
        }

        if (!float.IsNegativeInfinity(v3lb.sqrMagnitude) && !float.IsPositiveInfinity(v3lb.sqrMagnitude))
        {
            quadManager.cornerLB.transform.position = v3lb * 1000.0f;
        }

        if (!float.IsNegativeInfinity(v3rt.sqrMagnitude) && !float.IsPositiveInfinity(v3rt.sqrMagnitude))
        {
            quadManager.cornerRT.transform.position = v3rt * 1000.0f;
        }

        if (!float.IsNegativeInfinity(v3rb.sqrMagnitude) && !float.IsPositiveInfinity(v3rb.sqrMagnitude))
        {
            quadManager.cornerRB.transform.position = v3rb * 1000.0f;
        }

        quadManager.UpdateQuadMesh();
    }

    Color GetDepthColor(float depth)
    {
        int depthColorIndex = (int)(Mathf.InverseLerp(kinectManager.minDepth, kinectManager.maxDepth, depth)*(depthColorResolution-1));
        return depthColorLookup[depthColorIndex];
    }

    public void UpdateKinectMesh()
    {
        if (kinectManager.cameraSpacePoints == null) return;

        int index = 0;
        
        for (int y = 0; y < kinectManager.depthHeight; y += gridStep)
        {
            for (int x = 0; x < kinectManager.depthWidth; x += gridStep)
            {
                // MAKE SURE OUR INDEX DOESN'T EXCEED OUR PARTICLE ALLOCATION
                if (index >= cloud.Length) break;

                cloud[index].position = new Vector3(0, 0, 0);
                cloud[index].color = GetDepthColor(0);
                cloud[index].size = 1.0f;

                if ( ClipToZone )
                {
                    // DO A BASIC BOUNDS CHECK FOR THE OTHER BOUNDING RECT FIRST
                    if (x > lowX && x < highX && y > lowY && y < highY)
                    {
                        // DO BARYCENTRIC TRIANGLE COLLISION FOR QUAD CHECKING
                        if (
                            IsPointInTriangle(new Vector2(x, y), _lb2d, _lt2d, _rb2d) ||
                            IsPointInTriangle(new Vector2(x, y), _rb2d, _lt2d, _rt2d)
                            )
                        {
                            Vector3 pos = kinectManager.GetCameraSpacePointfromDepthImageXY(new Vector2(x, y));

                            if ( isNormalizeToZone ) { pos = normalizedRotation * pos;  }

                            if (!float.IsInfinity(pos.sqrMagnitude) && !float.IsNaN(pos.sqrMagnitude))
                            {
                                cloud[index].position = pos * 1000.0f;
                                cloud[index].color = GetDepthColor(cloud[index].position.z);
                                cloud[index].size = Random.Range(2.0f, 5.0f);
                            }
                        }

                    }
                } else
                {
                    Vector3 pos = kinectManager.GetCameraSpacePointfromDepthImageXY(new Vector2(x, y));
                    if (isNormalizeToZone) { pos = normalizedRotation * pos; }

                    if (!float.IsInfinity(pos.sqrMagnitude) && !float.IsNaN(pos.sqrMagnitude))
                    {
                        cloud[index].position = pos * 1000.0f;
                        cloud[index].color = GetDepthColor(cloud[index].position.z);
                        cloud[index].size = Random.Range(2.0f, 5.0f);
                    }
                }
                index++;
            }
        }
        particles.SetParticles(cloud, cloud.Length);
    }

    public bool IsPointInTriangle(Vector2 testPoint, Vector2 PointA, Vector2 PointB, Vector2 PointC)
    {
        Vector2 v0 = PointC - PointA;
        Vector2 v1 = PointB - PointA;
        Vector2 v2 = testPoint - PointA;

        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
    
}
