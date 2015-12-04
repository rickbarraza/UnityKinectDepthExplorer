using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    KinectManager kinectManager;
    ParticleManager particleManager;

    Slider depthZMinSlider;
    Text depthMinText;

    Slider depthZMaxSlider;
    Text depthMaxText;

    Toggle toggleClipToZone;
    Toggle toggleNormalizeToZone;

    DragCorners2D dragCorners2D;

    Text txtOut;

	void Start () {
        
        GameObject go;

        // SETUP PANEL CONTROLS
        go = GameObject.Find("sliderDepthMin");
        depthZMinSlider = go.GetComponent<Slider>();

        go = GameObject.Find("sliderDepthMax");
        depthZMaxSlider = go.GetComponent<Slider>();

        go = GameObject.Find("sliderDepthMinLabel");
        depthMinText = go.GetComponent<Text>();

        go = GameObject.Find("sliderDepthMaxLabel");
        depthMaxText = go.GetComponent<Text>();

        go = GameObject.Find("Text");
        txtOut = go.GetComponent<Text>();

        go = GameObject.Find("EditorPanel");
        dragCorners2D = go.GetComponent<DragCorners2D>();

        go = GameObject.Find("goParticles");
        particleManager = go.GetComponent<ParticleManager>();

        go = GameObject.Find("toggleClipToZone");
        toggleClipToZone = go.GetComponent<Toggle>();

        go = GameObject.Find("toggleNormalizeToZone");
        toggleNormalizeToZone = go.GetComponent<Toggle>();

        toggleClipToZone.onValueChanged.AddListener(delegate { toggleClipToZoneChanged(); });
        toggleNormalizeToZone.onValueChanged.AddListener(delegate { toggleNormalizeToZoneChanged(); });

        txtOut.text = "UPDATE GO:";

        depthZMinSlider.onValueChanged.AddListener(delegate { slidersMinMaxChanged(); });
        depthZMaxSlider.onValueChanged.AddListener(delegate { slidersMinMaxChanged(); });

        depthZMinSlider.value = 500;
        depthZMaxSlider.value = 1800;

        slidersMinMaxChanged();
	}
	
    public void toggleClipToZoneChanged()
    {
        particleManager.ClipToZone = toggleClipToZone.isOn;
        particleManager.UpdateKinectMesh2();
    }

    public void toggleNormalizeToZoneChanged()
    {
        particleManager.NormalizeToZone(toggleNormalizeToZone.isOn);
    }

    public void slidersMinMaxChanged()
    {
        if ( kinectManager == null )
        {
            GameObject go = GameObject.Find("goKinectManager");
            kinectManager = go.GetComponent<KinectManager>();
        }

        depthMinText.text = "MIN " + depthZMinSlider.value;
        depthMaxText.text = "MAX " + depthZMaxSlider.value;

        kinectManager.minDepth = depthZMinSlider.value;
        kinectManager.maxDepth = depthZMaxSlider.value;
    }

	void Update () {

        if ( dragCorners2D.isDirty == true )
        {
            dragCorners2D.isDirty = false;
            particleManager.UpdateCorners(
                dragCorners2D.markerLT.transform.localPosition,
                dragCorners2D.markerLB.transform.localPosition,
                dragCorners2D.markerRT.transform.localPosition,
                dragCorners2D.markerRB.transform.localPosition);
        }
	}

}
