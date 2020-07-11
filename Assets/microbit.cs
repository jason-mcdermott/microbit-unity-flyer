using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.IO.Ports;
using System;


public class microbit : MonoBehaviour {


	public static bool isSerialInit = false;
	public bool AutomaticPort = true;
	public string COM = "COM5";
	public bool portFound = false;
	public bool searchingPort = false;
	public bool manualPortWorking = false;
	public bool checkingManualPort = false;
	public bool canEdit = true;

	static private int[] baseRotations = new int[3];
	static private int[] rotation = new int[3];
	static private int[] prev_rotation = new int[3];
	static private bool buttonAPressed = false;
	static private bool buttonBPressed = false;
	static private bool buttonAHeld = false;
	static private bool buttonBHeld = false;
	static private float buttonAStart;
	static private float buttonBStart;
	static private bool buttonAHoldCheck;
	static private bool buttonBHoldCheck;
	static private bool doReset = true;
	static private float delayInputTimer;

	static private bool buttonAClicked = false;

	private bool debug = false;


	public static bool isSerialStreamInit = false;

	private SerialPort stream;
	static private string inline;
	private int COMnr = 0;
	private int BAUD = 115200;
	private int timeout = 0;


	private float lastRecievedTime;
	static private bool timingOut = false;

	void Awake () {
		canEdit = false;
		if (isSerialStreamInit == true) {
			printDebug ("Destorying duplicate SerialStream");
			Destroy (this.gameObject);
		}
		isSerialStreamInit = true;

		this.stream = new SerialPort (COM, BAUD);
		if (this.timeout == 0) {
			this.stream.ReadTimeout = 5;
		} else {
			this.stream.ReadTimeout = this.timeout;
		}
		openStream ();

		if (isSerialInit == true) {
			printDebug ("Destorying SerialToController");
			Destroy (this.gameObject);
		}
		isSerialInit = true;
		searchingPort = true;

	}

	void Update () {

		StartCoroutine(AsyncReaduBit(10f));
		updateInputs (getLine());
		if (!portFound && AutomaticPort) {
			if (checkSerial ()) {
				searchingPort = false;
				portFound = true;
			} else {
				COMnr ++;
				COM = "COM" + COMnr;
//				Debug.Log (COM);

				this.stream = new SerialPort (COM, BAUD);
				if (this.timeout == 0) {
					this.stream.ReadTimeout = 1;
				} else {
					this.stream.ReadTimeout = this.timeout;
				}
				openStream ();
				if (COMnr > 50)
					COMnr = 0;
			}

		}
		if (!AutomaticPort && !manualPortWorking) {
			checkingManualPort = true;
			if (checkSerial ()) {
				manualPortWorking = true;
				checkingManualPort = false;
			}
		}

//		Debug.Log (getLine ());

	}

	private void updateInputs(string s){
		if (Time.realtimeSinceStartup < delayInputTimer) {
			rotation [0] = 0;
			rotation [1] = 0;
			rotation [2] = 0;
			buttonAPressed = false;
			buttonBPressed = false;
			buttonAHeld = false;
			buttonBHeld = false;
		} else {
			try {
				if (s.Equals ("Reset")) {
					//do resets
					doReset = true;
					return;
				}

				char[] seperator = { ',' };
				string[] splitString = s.Split (seperator, 4);
				// if any of them are null, exception will be thrown and the line will be ignored.
				if (splitString [0].Equals ("A") && !buttonAPressed){
					buttonAClicked = true;
				}else {
					buttonAClicked = false;
				}
				if (splitString [0].Equals ("A")) {
					buttonAPressed = true;
					buttonBPressed = false;
					buttonBHeld = false;
					buttonBHoldCheck = false;
					checkButtonA ();
				} else if (splitString [0].Equals ("B")) {
					buttonBPressed = true;
					buttonAPressed = false;
					buttonAHeld = false;
					buttonAHoldCheck = false;
					checkButtonB ();
				} else if (splitString [0].Equals ("C")) {
					buttonAPressed = true;
					buttonBPressed = true;

					checkButtonA ();
					checkButtonB ();

				} else {
					buttonAPressed = false;
					buttonBPressed = false;
					buttonAHoldCheck = false;
					buttonBHoldCheck = false;
					buttonAHeld = false;
					buttonBHeld = false;


				}

				string roll = splitString [1];
				string pitch = splitString [2];
				string yaw = splitString [3];

				rotation [0] = int.Parse (roll);
				rotation [1] = int.Parse (pitch);
				rotation [2] = int.Parse (yaw);


				if (doReset) {
					baseRotations [0] = int.Parse (roll);
					baseRotations [1] = int.Parse (pitch);
					baseRotations [2] = int.Parse (yaw);
					doReset = false;
				}


			} catch (System.Exception) {
				return;
			}
		}
	}

	private bool checkSerial(){
		for (int i = 0; i < 100; i++) {
			if (getLine () != null)
				return true;
		}
		return false;
	}


	private void checkButtonA(){
		if(buttonAHoldCheck == false){
			buttonAHoldCheck = true;
			buttonAStart = Time.realtimeSinceStartup;
		}
		else if(buttonAHoldCheck == true){
			if((Time.realtimeSinceStartup - buttonAStart)<0.25){
				//Do nothing
				buttonAHeld = false;
			}
			else{
				buttonAHeld = true;
			}
		}
	}


	private void checkButtonB(){
		if(buttonBHoldCheck == false){
			buttonBHoldCheck = true;
			buttonBStart = Time.realtimeSinceStartup;
		}
		else if(buttonBHoldCheck == true){
			if((Time.realtimeSinceStartup - buttonBStart)<0.25){
				//Do nothing
				buttonBHeld = false;
			}
			else{
				buttonBHeld = true;
			}
		}
	}

	static public bool getAClicked(){
		return buttonAClicked;
	}

	static public bool getAPressed(){
		return buttonAPressed;
	}


	static public bool getBPressed(){
		return buttonBPressed;
	}


	static public bool getAHeld(){
		return buttonAHeld;
	}


	static public bool getBHeld(){
		return buttonBHeld;
	}


	private int getRoll(){
		return rotation [0];
	}

	private int getPitch(){
		return rotation [1];
	}

	private int getYaw(){
		return rotation [2];
	}

	static private int getAdjustedRoll(){
		int curr = rotation[0] - baseRotations[0];
//		Debug.Log (curr);
//		if (prev_rotation [0] != null) {
		if (Mathf.Abs (prev_rotation [0] - curr) > 5) {
			prev_rotation [0] = curr;
//			}
		} 
//		else {
//			prev_rotation [0] = curr;
//		}
		return prev_rotation[0];
	}

	static private int getAdjustedPitch(){

		int curr = rotation[1] - baseRotations[1];

		if (Mathf.Abs (prev_rotation [1] - curr) > 5) {
			prev_rotation [1] = curr;
		}

		return prev_rotation[1];
	}

	static private int getAdjustedYaw(){
		if (Mathf.Abs (rotation [2] - baseRotations [2]) < 8) {
			return 0;
		} else if (rotation [2] - baseRotations [2] < -8) {
			return adjustAngle(rotation[2] - baseRotations[2] + 8, 90);
//			return adjustAngle(rotation[2] - baseRotations[2] + 8, 360);
		}
		else{
			return adjustAngle(rotation[2] - baseRotations[2] - 8, 90);
//			return adjustAngle(rotation[2] - baseRotations[2] - 8, 360);
		}
	}


	static private int adjustAngle(int a, int limit){
		int returnVal;
		if (a > limit) {
			returnVal = -limit + (limit - a);
		} else if (a < -limit) {
			returnVal = limit - (-limit - a);
		} else {
			returnVal = a;
		}

		//----------Deadzone------------//
		if (returnVal > limit - limit / 3) {
			return limit - limit / 3;
		} else if (returnVal < -limit + limit / 3) {
			return -limit + limit / 3;
		} else {
			return returnVal;
		}
	}


	static public Vector3 getRotations(){
		return new Vector3 (getAdjustedPitch(), getAdjustedYaw(), getAdjustedRoll());
	}

	static public Vector3 getRawRotations(){
		return new Vector3 (rotation[1],rotation[2], rotation[0]);
	}

	private void printDebug(string s){
		if (debug) {
			Debug.Log (s);
		}
	}

	static public bool isSerialTimeOut(){
		return isTimingOut ();
	}


	//Delays further input by s seconds
	static public void delayInput(float s){
		delayInputTimer = Time.realtimeSinceStartup + s;
	}


	public IEnumerator AsyncReaduBit(float timeout, System.Action fail = null){
		float startTime = Time.realtimeSinceStartup;
		float currentTime = 0;
		float diff = 0;

		pinguBit ();

		string readLine = null;
		do {
			try {
				readLine = readStream ();
			} catch (System.Exception) {
				//printDebug(e);
				readLine = null;
			}
			if(readLine != null){
				inline = readLine;
				this.stream.BaseStream.Flush();
				lastRecievedTime = Time.realtimeSinceStartup;
				timingOut = false;
			}
			else{
				if((Time.realtimeSinceStartup - lastRecievedTime)> 1f){
					printDebug ((lastRecievedTime-Time.realtimeSinceStartup).ToString());
					timingOut = true;
					StartCoroutine(retryConnection());
				}
				yield return new WaitForSeconds(0.016f);
			}

			currentTime = Time.realtimeSinceStartup;
			diff = (currentTime - startTime)*1000;
		} while(diff < timeout);

		if(fail !=null){
			fail();
		}
		else{
			yield return null;
		}

	}


	static public string getLine(){
		//Debug.Log (inline);
		return inline;
	}

	private void pinguBit(){
		try{
			this.stream.WriteLine ("SEND");
			this.stream.BaseStream.Flush ();
		}
		catch(System.Exception e){
			if (e is System.IO.IOException) {
				printDebug ("catch");
			}
		}
	}


	private string readStream(){
		try{
			String line = stream.ReadLine();
			printDebug(line);
			return line;
		}catch(System.Exception e){
			printDebug ("Error with reading stream");
			printDebug (e.ToString());
			if (e.GetType() == typeof (System.TimeoutException)) {
			}
			return null;
		}
	}

	private void openStream(){
		printDebug ("Initialised");
		try{
			this.stream.Open();
		}
		catch(System.IO.IOException e) {
			printDebug ("Something went wrong with opening stream");
			printDebug (e.ToString());

			//Attempt to reconnect
			StartCoroutine (retryConnection ());
		}
	}
	void OnDisable(){
		try{
			this.stream.Close ();
			printDebug("Closing Stream");
		}
		catch(System.IO.IOException){
		}
		catch(System.Exception){
		}
	}


	public IEnumerator retryConnection(){
		printDebug("retrying");
		if (this != null) {
			do {
				try {
					this.stream.Close ();
					this.stream.Open ();
				} catch (System.Exception) {

				}
				yield return new WaitForSeconds (0.016f);
			} while(!stream.IsOpen);

			printDebug ("Ending Retrys");

			yield break;
		}

	}

	static public bool isTimingOut(){
		return timingOut;
	}


}


[CustomEditor(typeof(microbit))]
public class MyScriptEditor : Editor
{
	override public void OnInspectorGUI()
	{
		var myScript = target as microbit;

		myScript.AutomaticPort = GUILayout.Toggle (myScript.AutomaticPort, "Automatic Port");

		if (!myScript.AutomaticPort) {
			if (myScript.canEdit)
				myScript.COM = EditorGUILayout.TextField (myScript.COM);
			if (myScript.checkingManualPort) {
				EditorGUILayout.LabelField ("No data for port " + myScript.COM);
			}
			if (myScript.manualPortWorking) {
				EditorGUILayout.LabelField ("Correct port " + myScript.COM);
			}
		}
		else {
			if (myScript.searchingPort) {
				EditorGUILayout.LabelField ("Searching for port...");
			}
			if (myScript.portFound) {
				EditorGUILayout.LabelField ("Port found. " + myScript.COM);
			}
		}

	}
}


