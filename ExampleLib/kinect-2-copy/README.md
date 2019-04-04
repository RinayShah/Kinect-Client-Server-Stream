# kinect-2-body-recorder

Get Kinect 2 skeleton tracking data (joint locations and tracking status) and save as csv files.

SpikeCutter.CutSpike() filters out spikes in raw skeleton data. Filtered data saved in extra columns. Note that the filtered data have 1 frame delay.

The algorithm recognizes a spike (a sudden jump of joint location data) when the acceleration of the joint exceeds a certain threshold. The algorithm can also correct itself when a normal acceleration is falsely detected as a spike (a false alarm).

Use Microsoft Visual Studio to compile.

This project is based on Vangos Pterneas's beautiful source code
http://pterneas.com/2014/03/13/kinect-for-windows-version-2-body-tracking/

Details:
1. Only one body is recorded. Make sure there is only one body when you hit the record button.
2. Only upper body joints are recorded.
3. Joint Orientations are not recorded.
4. SpikeCutter method applied to elbow and wrist.
