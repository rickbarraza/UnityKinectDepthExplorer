using UnityEngine;
using System.Collections;

public class ParticleManager : MonoBehaviour {

    ParticleSystem.Particle[] cloud;
    ParticleSystem particles;

    public int totalParticles;

    int width = 256;
    int height = 212;

    public float depthColorResolution = 1000;
    Color[] depthColorLookup;

    public float MaxDepth = 500.0f;
    public float MinDepth = 0.0f;

    float low = 0.5f;
    float high = 0.5f;

    float deltaTime = 0.0f;

    void Start () {

        CreateDepthColorLookup();

        low = MaxDepth;

        particles = this.gameObject.GetComponent<ParticleSystem>();
        totalParticles = (width/2) * (height/2);

        cloud = new ParticleSystem.Particle[totalParticles];
        UpdateParticles();
    }

    void CreateDepthColorLookup()
    {
        depthColorLookup = new Color[(int)depthColorResolution];

        Color startColor = new Color(0 / 255.0f, 97 / 255.0f, 62 / 255.0f);
        Color endColor = new Color(255 / 255.0f, 255 / 255.0f, 255 / 255.0f);
        
        for ( int i = 0; i < depthColorResolution; i++ )
        {
            depthColorLookup[i] = Color.Lerp(startColor, endColor, i / depthColorResolution);
        }
    }

    Color GetDepthColor(float depth)
    {
        int depthColorIndex = (int)(Mathf.InverseLerp(MinDepth, MaxDepth, depth)*(depthColorResolution-1));
        return depthColorLookup[depthColorIndex];
    }

    void UpdateParticles()
    {
        int step = 4;
        float startX = -width * (step / 2.0f);

        int index = 0;
        high = low = 0.5f;

        for (int x = 0; x < width; x+=2)
        {
            for (int z = 0; z < height; z+=2)
            {
                var perlinY = Mathf.PerlinNoise(
                    Time.time + (x * 5 / (float)width),
                    Time.time + (z * 5 / (float)height)
                    );

                if (perlinY > high) high = perlinY;
                if (perlinY < low) low = perlinY;

                perlinY = MinDepth + (perlinY * (MaxDepth - MinDepth));
                perlinY = Mathf.Clamp(perlinY, MinDepth, MaxDepth);

                cloud[index].position = new Vector3(startX + (x * step), perlinY, z * step);
                cloud[index].color = GetDepthColor(cloud[index].position.y);
                cloud[index].size = Random.Range(2.0f, 5.0f);
                index++;
            }
        }
        particles.SetParticles(cloud, cloud.Length);
    }

    void Update()
    {
        UpdateParticles();
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        string output = "INSTRUCTIONS:\nMOUSE DRAG TO ROTATE AROUND TARGET. \nARROW UP / DOWN TO ZOOM IN / OUT.\n\n";
        output += "> Perlin low -> high\n> [ " + low + " -> " + high + "]\n\n";
        output += "> FPS: " + string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        GUI.Label(new Rect(10, 10, 380, 140), output);        
    }

}
