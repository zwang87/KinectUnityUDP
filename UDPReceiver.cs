using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver : MonoBehaviour {
	Thread receiveThread;
	UdpClient client;
	public const int port = 9090;
	
	public string lastReceivedUPDpacket = "";
	public string allReceivedUPDpacket = "";
	
	private GameObject marker;
	
	public double handPosX = 0;
	public double handPosY = 0;
	public double handPosZ = 0;
	
	public double headPosX = 0;
	public double headPosY = 0;
	public double headPosZ = 0;
	
	public string content = "";
	public static bool gripped = false;

	private System.Object thisLock = new System.Object();

	// Use this for initialization
	void Start () {
		receiveThread = new Thread(new ThreadStart(this.ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		
		marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		marker.name = "MidPtSph";
		marker.tag = "HandMarker";
		marker.AddComponent("TestObjCollider");
		marker.AddComponent<Rigidbody>();
		marker.collider.isTrigger = false;
		//marker.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
	}
	
	private void ReceiveData()
	{
		client = new UdpClient(port);
		Debug.Log("thread");
		
		while(true)
		{
			try
			{
				IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive(ref ip);
				lock(thisLock){//you need to have a lock mechanism if you need to assign it to global variables
					content = Encoding.UTF8.GetString(data);
					Debug.Log(content);
					lastReceivedUPDpacket = content;
					allReceivedUPDpacket += content;
		
					string[] array = content.Split(' ');
					if(array[0] == "pos:"){
						handPosX = Convert.ToDouble(array[1]);
						handPosY = -1 * Convert.ToDouble(array[2]);
						handPosZ = Convert.ToDouble(array[3]);
						headPosX = Convert.ToDouble(array[4]);
						headPosY = -1 * Convert.ToDouble(array[5]);
						headPosZ = Convert.ToDouble(array[6]);
					}
					else{
						if(array[2] == "Gripped")
							gripped = true;
						else if(array[2] == "Released")
							gripped = false;
					}						
				}		
			}
			catch(Exception e)
			{
				Debug.Log(e.ToString());
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(gripped){
			marker.transform.localScale = new Vector3(2, 2, 2);
			//Debug.Log("gripped");
		}
		else{
			marker.transform.localScale = new Vector3(1, 1, 1);
			//Debug.Log("released");
		}
		
		if(Input.GetMouseButtonDown(0))
		{
			Debug.Log(content);
		}
		marker.transform.position = new Vector3((float)handPosX, (float)handPosY, (float)(-10*handPosZ));
		this.transform.position = new Vector3((float)headPosX, (float)headPosY, (float)(-10*headPosZ));
	}
	
	void OnDisable()
	{
		if(receiveThread != null)
			receiveThread.Abort();
		client.Close();
		Debug.Log("Connection Lost.");
	}
}

