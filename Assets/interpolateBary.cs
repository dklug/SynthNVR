using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using VRTK;

public class interpolateBary : VRTK_InteractableObject
{

    public Transform[] tetraTransforms;
    public Transform[] sliderBasePoints;
    public GameObject[] sliders;
    Vector3 lastPosition;

    public int soundServerPort = 4242;
    public string soundServerAddr = "127.0.0.1";
    private TcpClient serverConn;

    public float[] points = new float[4];


    float[] normalize(float[] distances)
    {
        float sumSquares = 0;
        for (int i = 0; i < distances.Length; ++i)
        {
            sumSquares += Mathf.Pow(distances[i], 2);
        }
        float normalizer = Mathf.Sqrt(sumSquares);
        // Handle divide by zero
        if (normalizer == 0)
        {
            distances[0] = 1;
            return distances;
        }

        for (int i = 0; i < distances.Length; ++i)
        {
            distances[i] /= normalizer;
        }
        return distances;
    }

    void updateBarycentric()
    {
        float[] dist = new float[4];
        float sumSquares = 0;
        for (int i = 0; i < tetraTransforms.Length; ++i)
        {
            dist[i] = 0.7f - Vector3.Distance(tetraTransforms[i].position, transform.position);
            //sumSquares += Mathf.Pow (dist [i], 2);
        }
        /*
		float normalizer = Mathf.Sqrt(sumSquares);
		for (int i = 0; i < tetraTransforms.Length; ++i) {
			points [i] = dist [i] / normalizer;
		}
		*/
        points = normalize(dist);
    }

    void updateSliders()
    {
        for (int i = 0; i < sliderBasePoints.Length; ++i)
        {
            Vector3 newPosition = new Vector3(sliderBasePoints[i].position.x, sliderBasePoints[i].position.y, sliderBasePoints[i].position.z);
            newPosition.y += points[i];
            sliders[i].transform.position = newPosition;
        }
    }

    public void updateControlFromSliders()
    {
        Debug.Log("Updating from sliders");
        float[] distances = new float[4];
        for (int i = 0; i < sliders.Length; ++i)
        {
            distances[i] = Vector3.Distance(sliderBasePoints[i].position, sliders[i].transform.position);
        }
        distances = normalize(distances);

        // Convert barycentric coords to xyz for
        Vector3 newControlPosition;
        newControlPosition.x = newControlPosition.y = newControlPosition.z = 0;
        for (int i = 0; i < tetraTransforms.Length; ++i)
        {
            newControlPosition.x += distances[i] * tetraTransforms[i].position.x;
            newControlPosition.y += distances[i] * tetraTransforms[i].position.y;
            newControlPosition.z += distances[i] * tetraTransforms[i].position.z;
        }
    }

    void transmitCoordinates()
    {
        if (serverConn == null)
        {
            Debug.Log("Server connection isnt open for transmitting coordinates");
            return;
        }
        try
        {
            NetworkStream stream = serverConn.GetStream();
            if (stream.CanWrite)
            {
                byte[] coords = new byte[4];
                for (int i = 0; i < points.Length; ++i)
                {
                    coords[i] = (byte)Mathf.Round(points[i] * 10);
                }
                stream.Write(coords, 0, 4);
                //Debug.Log("Just sent coords to server");
            }
            else
            {
                Debug.Log("Socket isnt writeable");
            }

        }
        catch (SocketException sockExcep)
        {
            Debug.Log("Socket exception " + sockExcep);
        }
    }

    // Use this for initialization
    void Start()
    {
        try
        {
            serverConn = new TcpClient(soundServerAddr, soundServerPort);
        }
        catch (SocketException sockExcep)
        {
            Debug.Log("Socket exception " + sockExcep);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lastPosition == null || transform.position != lastPosition)
        {
            updateBarycentric();
            transmitCoordinates();
            updateSliders();
            lastPosition = transform.position;
        }
    }
}

