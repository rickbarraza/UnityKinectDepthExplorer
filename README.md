# UnityKinectDepthExplorer
####**:spaghetti: SPAGHETTI CODE ALERT :spaghetti:**####

A working sample / research repository of using Unity to explore Kinect Depth data and recontextualizing the Color to Camera Space using a user defined, spatial quad. The primary use case is to define a table top "zone of interest" and create an adjusted Depth Image that shades the pixels from the quad surface for subsequent vision processing.

####**THIS SKETCH FUNCTIONALLY:**####
  1. Converts the raw USHORT depth data from the Depth Frame to an RGBA32 data array based on a two color lerped Color Lookup Table.
  2. Use the DepthData and CoordinateMappoer to populate a ParticleSystem with Camera Space accurate particle data.
  3. Allows the user to define a skewed rectangle of coordinates on the DepthImage to act as a volumetric Zone Of Interest
  4. Allow clipping of the particle system based on the Zone of Interest
  5. Normalize Kinect Camera Space to Unity World Space to reorient the Particle System to use (0,1,0) as up based on the Zone Of Interest.
  6. Rotate Depth Color through the normalized Camera Space so the Adjusted Depth Image shades pixels based on the Quad Normal for future vision algorithms.
  
  
