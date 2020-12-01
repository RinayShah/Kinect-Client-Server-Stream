This project streams the x,y,z coordinates of the 25 joints to a server computer and records the data on CSV file. It uses TCP/IP Client Server Netowrk to pass over the joint data to another computer.  This project uses the source code from: https://pterneas.com/2014/03/13/kinect-for-windows-version-2-body-tracking/

I have added features incuding:
- Record Joint Data on CSV file
- Stream Joint Data in Real time to Server Computer

To use the source code:
1. Run Server code on one computer and Clinet on another
2. Edit the IP address (on the Clinet Computer) to match the IP address of the Server Computer.
3. Kinect joint data will stream the data to the server computer.
4. To access the data recorded in the CSV file go to Bin->Debug->Recordings

