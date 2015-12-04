using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour {

    ParticleSystem.Particle[] cloud;
    ParticleSystem particles;

    KinectManager kinectManager = null;
    QuadManager quadManager = null;

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
        goodParticles = new List<ParticleSystem.Particle>();

        InvokeRepeating("UpdateKinectMesh2", 3.0f, 2.0f);
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
        UpdateKinectMesh2();

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

    List<ParticleSystem.Particle> goodParticles;

    public void UpdateKinectMesh2()
    {
        // NO DEPTH TO CAMERA SPACE, NO RESULTS!
        if (kinectManager.cameraSpacePoints == null) return;

        goodParticles.Clear();
        goodParticles = new List<ParticleSystem.Particle>();

        Vector2 imagePosition = new Vector2(0, 0);

        System.Array.Clear(kinectManager.depthZoneData, 0, kinectManager.depthZoneData.Length);
        
        for (int y = 0; y < kinectManager.depthHeight; y++)
        {
            for (int x = 0; x < kinectManager.depthWidth; x++)
            {
                imagePosition.x = x;
                imagePosition.y = y;

                if (ClipToZone)
                {
                    if (isInZone(imagePosition))
                    {
                        CopyDepthImageData(imagePosition);
                        testAddParticle(imagePosition);
                    }
                } else
                {
                    CopyDepthImageData(imagePosition);
                    testAddParticle(imagePosition);
                }

            }
        }
        
        particles.SetParticles(goodParticles.ToArray(), goodParticles.Count);

        kinectManager.textureDepthModified.LoadRawTextureData(kinectManager.depthZoneData);
        kinectManager.textureDepthModified.Apply();
    }

    
    void CopyDepthImageData(Vector2 imagePosition)
    {

        int depthIndex = (int)(imagePosition.x + ( (423 - imagePosition.y) * kinectManager.depthWidth)) * 4;

        if ( isNormalizeToZone )
        {
            Vector3 pos3D = kinectManager.GetCameraSpacePointfromDepthImageXY(imagePosition) * 1000.0f;
            Vector3 normalizedPos = normalizedRotation * pos3D;

            if ( ( pos3D.z > kinectManager.minDepth && pos3D.z < kinectManager.maxDepth) 
                && (!float.IsInfinity(normalizedPos.sqrMagnitude) && !float.IsNaN(normalizedPos.sqrMagnitude))
                )
            {
                float yDelta = (quadManager.quadCenter.position.y - normalizedPos.y)/ -100.0f;
                yDelta = Mathf.Clamp(yDelta, 0.0f, 1.0f);
                byte yColor = (byte)(255.0f * yDelta);
                kinectManager.depthZoneData[depthIndex] = yColor;
                kinectManager.depthZoneData[depthIndex + 1] = yColor;
                kinectManager.depthZoneData[depthIndex + 2] = yColor;
                kinectManager.depthZoneData[depthIndex + 3] = 255;
            }
            else
            {
                // SOMETHING WRONG, CLEAR THE PIXEL
                kinectManager.depthZoneData[depthIndex] = 0;
                kinectManager.depthZoneData[depthIndex + 1] = 0;
                kinectManager.depthZoneData[depthIndex + 2] = 0;
                kinectManager.depthZoneData[depthIndex + 3] = 0;
            }
            
        }
        else
        {
            kinectManager.depthZoneData[depthIndex] = kinectManager.depthColorData[depthIndex];
            kinectManager.depthZoneData[depthIndex + 1] = kinectManager.depthColorData[depthIndex + 1];
            kinectManager.depthZoneData[depthIndex + 2] = kinectManager.depthColorData[depthIndex + 2];
            kinectManager.depthZoneData[depthIndex + 3] = 255;
        }
    }

    void testAddParticle(Vector2 testPosition)
    {
        if ( testPosition.x % gridStep == 0 && testPosition.y % gridStep == 0 )
        {
            Vector3 pos3D = kinectManager.GetCameraSpacePointfromDepthImageXY(testPosition) * 1000.0f;

            if ( !float.IsInfinity(pos3D.sqrMagnitude) && !float.IsNaN(pos3D.sqrMagnitude))
            {
                ParticleSystem.Particle newParticle = new ParticleSystem.Particle();

                if (isNormalizeToZone)
                {
                    pos3D = normalizedRotation * pos3D;

                    float yDelta = (quadManager.quadCenter.position.y - pos3D.y) / -100.0f;
                    yDelta = Mathf.Clamp(yDelta, 0.0f, 1.0f);
                    newParticle.color = new Color(yDelta, yDelta, yDelta, 1.0f);
                }
                else
                {
                    newParticle.color = GetDepthColor(newParticle.position.z);
                }

                newParticle.position = pos3D;
                newParticle.size = Random.Range(2.0f, 5.0f);
         
                goodParticles.Add(newParticle);
            }
        }
    }

    bool isInZone(Vector2 testPosition)
    {
        if ( IsPointInTriangle(testPosition, _lb2d, _lt2d, _rb2d) ||
                IsPointInTriangle(testPosition, _rb2d, _lt2d, _rt2d) )
        {
            return true;
        } else
        {
            return false;
        }
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
