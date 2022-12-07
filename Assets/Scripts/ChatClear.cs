using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatClear : MonoBehaviour
{
	public float lastupdate = 0;
	public float elapsedtime = 0;

	void Start()
	{
		transform.GetComponent<TextMesh>().text = "";
		lastupdate = Time.time;
	}

	void Update()
	{
		elapsedtime = Time.time - lastupdate;
		if (elapsedtime > 3)
		{
			transform.GetComponent<TextMesh>().text = "";
		}
	}

}
