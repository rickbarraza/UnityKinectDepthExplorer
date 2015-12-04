using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using UnityEngine.UI;


public class KinectManager : MonoBehaviour {

    KinectSensor sensor;
    CoordinateMapper coordinateMapper;
    MultiSourceFrameReader reader;

    public RawImage riDepth;
    public RawImage riDepthModified;

    public CameraSpacePoint[] cameraSpacePoints;
    public int depthWidth, depthHeight;

    byte[][] GreyLookup;
    int totalGreys;
    
    ushort[] depthData;
    public byte[] depthColorData;
    public byte[] depthZoneData;

    public float minDepth;
    public float maxDepth;

    Texture2D textureDepthOriginal;
    public Texture2D textureDepthModified;

    public Text myText;

    void Start () {
        sensor = KinectSensor.GetDefault();

        if ( sensor != null )
        {
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            coordinateMapper = sensor.CoordinateMapper;

            FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthWidth = depthFrameDesc.Width;
            depthHeight = depthFrameDesc.Height;

            depthData = new ushort[depthFrameDesc.LengthInPixels];
            depthColorData = new byte[depthFrameDesc.LengthInPixels * 4]; // 4 BPP for RGBA
            depthZoneData = new byte[depthFrameDesc.LengthInPixels * 4];
            cameraSpacePoints = new CameraSpacePoint[depthFrameDesc.LengthInPixels];

            textureDepthOriginal = new Texture2D(depthWidth, depthHeight, TextureFormat.RGBA32, false);
            textureDepthModified = new Texture2D(depthWidth, depthHeight, TextureFormat.RGBA32, false);


            riDepth.texture = textureDepthOriginal;
            riDepthModified.texture = textureDepthModified;
            
            if ( !sensor.IsOpen)
            {
                sensor.Open();
            }

        } else
        {
            Debug.LogError("Can't find Kinect Sensor.");
        }

        SetupGreyLookup();
	}
	
    void SetupGreyLookup()
    {
        totalGreys = 3 * 255;
        GreyLookup = new byte[totalGreys][];

        for ( int i = 0; i < totalGreys; i++  )
        {
            float partial = (float)i / 3.0f;

            GreyLookup[i] = new byte[] {
                (byte)(255 - (byte)(Mathf.Floor(partial))),
                (byte)(255 - (byte)(Mathf.Round(partial))),
                (byte)(255 - (byte)(Mathf.Ceil(partial)))
            };
        }
    }

    public Vector3 GetCameraSpacePointfromDepthImageXY(Vector2 imagePos)
    {
        int adjY = (int)(depthHeight - Mathf.Abs(imagePos.y));
        int depthIndex = (int)(imagePos.x + (adjY * depthWidth));
        depthIndex = Mathf.Clamp(depthIndex, 0, cameraSpacePoints.Length - 1);
        CameraSpacePoint csp = cameraSpacePoints[depthIndex];
        return new Vector3(csp.X, csp.Y, csp.Z);
    }

    void Update()
    {
        if ( reader != null )
        {
            MultiSourceFrame MSFrame = reader.AcquireLatestFrame();

            if ( MSFrame != null )
            {
                using ( DepthFrame frame = MSFrame.DepthFrameReference.AcquireFrame())
                {
                    if ( frame != null )
                    {
                        frame.CopyFrameDataToArray(depthData);
                        coordinateMapper.MapDepthFrameToCameraSpace(depthData, cameraSpacePoints);
                        ConvertDepth2RGBA();
                    }
                }
                MSFrame = null;
            }
        }
    }

    public void ConvertDepth2RGBA()
    {
        int depthColorIndex = 0;

        int highestIndex = 0;
        int lowestIndex = 765;

        for ( int i = 0; i < depthData.Length; i++ )
        {
            ushort rawDepth = depthData[i];

            // BLACK OUT UNDEFINED OR OUT OF RANGE PIXELS
            if ( rawDepth.ToString() == "0" || rawDepth <= minDepth || rawDepth >= maxDepth )
            {
                depthColorData[depthColorIndex] = 0;
                depthColorData[depthColorIndex + 1] = 0;
                depthColorData[depthColorIndex + 2] = 0;
                depthColorData[depthColorIndex + 3] = 255;
            }
            else
            {
                // THIS SHOULD LERP / CLAMP THE DEPTH TO A MIN/MAX DEPTH PERCENTAGE
                // TO FIND THE APPROPRIATE DEPTH COLOR RANGE FOR OUR 765 SHADES OF GREY
                int lookupIndex =  (int)(totalGreys*( (rawDepth - minDepth) / (maxDepth - minDepth) ));
                Mathf.Clamp(lookupIndex, 0, totalGreys - 1);

                if (lookupIndex > highestIndex) highestIndex = lookupIndex;
                if (lookupIndex < lowestIndex) lowestIndex = lookupIndex;

                depthColorData[depthColorIndex] = GreyLookup[lookupIndex][0];
                depthColorData[depthColorIndex + 1] = GreyLookup[lookupIndex][1];
                depthColorData[depthColorIndex + 2] = GreyLookup[lookupIndex][2];
                depthColorData[depthColorIndex + 3] = 255;
            }
            depthColorIndex += 4;
        }

        textureDepthOriginal.LoadRawTextureData(depthColorData);
        textureDepthOriginal.Apply();

    }

    void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            sensor = null;
        }
    }

}
