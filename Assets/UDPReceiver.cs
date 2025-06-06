using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = true;
    private Quaternion receivedRotation = Quaternion.identity;

    private bool triggerPressed = false;
    private bool prevTriggerState = false; // Store previous trigger state

    private bool reloadPressed = false;

    public PlayerShoot gunController; // Reference to the script containing Shoot()

    private Queue<Action> workQueue = new Queue<Action>(); // Work queue for actions
    private object lockObject = new object(); // Lock to manage multi-threaded access

    void Start()
    {
        udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 5006)); // Allows receiving from any IP
        receiveThread = new Thread(new ThreadStart(ReceiveData))
        {
            IsBackground = true
        };
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 8888);
        while (isRunning)
        {
            try
            {
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                string receivedData = Encoding.UTF8.GetString(receivedBytes);
                Debug.Log($"Received UDP Data: {receivedData}");

                string[] values = receivedData.Split(' ');
                if (values.Length == 6)
                {
                    float qx = float.Parse(values[0]);
                    float qy = float.Parse(values[1]);
                    float qz = float.Parse(values[2]);
                    float qw = float.Parse(values[3]);
                    int reload = int.Parse(values[4]);
                    int trigger = int.Parse(values[5]);

                    receivedRotation = new Quaternion(qx, qy, qz, qw);

                    // Detect trigger change from 0 to 1
                    prevTriggerState = triggerPressed;
                    triggerPressed = trigger == 1;

                    reloadPressed = reload == 1;

                    if (!prevTriggerState && triggerPressed)
                    {
                        lock (lockObject)
                        {
                            workQueue.Enqueue(() => gunController.Shoot());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error receiving UDP data: {e.Message}");
            }
        }
    }

    void Update()
    {
        // Execute queued actions
        lock (lockObject)
        {
            while (workQueue.Count > 0)
            {
                Action action = workQueue.Dequeue();
                action?.Invoke();
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        udpClient.Close();
        receiveThread.Abort();
    }
}
