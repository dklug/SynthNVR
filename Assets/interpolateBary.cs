using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using VRTK;

public class interpolateBary : VRTK_InteractableObject{

    public List<Transform> tetraTransforms;
    public int soundServerPort = 4242;
    public string soundServerAddr = "127.0.0.1";
    private TcpClient serverConn;

    float p1, p2, p3, p4;

    void updateBarycentric() {
        float p1Dist = Vector3.Distance(tetraTransforms[0].position, transform.position);
        float p2Dist = Vector3.Distance(tetraTransforms[1].position, transform.position);
        float p3Dist = Vector3.Distance(tetraTransforms[2].position, transform.position);
        float p4Dist = Vector3.Distance(tetraTransforms[3].position, transform.position);

        float normalizer = Mathf.Sqrt(Mathf.Pow(p1Dist, 2) + Mathf.Pow(p2Dist, 2) + Mathf.Pow(p3Dist, 2) + Mathf.Pow(p4Dist, 2));

        p1 = p1Dist / normalizer;
        p2 = p2Dist / normalizer;
        p3 = p3Dist / normalizer;
        p4 = p4Dist / normalizer;
    }

    void transmitCoordinates() {
        if(serverConn == null) {
            Debug.Log("Server connection isnt open for transmitting coordinates");
            return;
        }
        try {
            NetworkStream stream = serverConn.GetStream();
            if(stream.CanWrite) {
                byte[] coords = new byte[4];
                coords[0] = (byte)Mathf.Round(p1 * 10);
                coords[1] = (byte)Mathf.Round(p2 * 10);
                coords[2] = (byte)Mathf.Round(p3 * 10);
                coords[3] = (byte)Mathf.Round(p4 * 10);
                stream.Write(coords, 0, 4);
                Debug.Log("Just sent coords to server");
            } else {
                Debug.Log("Socket isnt writeable");
            }

        } catch (SocketException sockExcep)
        {
            Debug.Log("Socket exception " + sockExcep);
        }
    }

	// Use this for initialization
	void Start () {
        try {
            serverConn = new TcpClient(soundServerAddr, soundServerPort);
        } catch (SocketException sockExcep) {
            Debug.Log("Socket exception " + sockExcep);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (IsGrabbed()) {
            updateBarycentric();
            transmitCoordinates();
        }
	}
}
