using UnityEngine;
using System.Collections;

using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void OnServe()
	{
		//SceneManager.LoadScene("Serve");
		Debug.Log("Serve selected");
	}
	public void OnForehand()
	{
		//SceneManager.LoadScene("Serve");
		Debug.Log("Forehand selected");
	}
	public void OnMiddlehand()
	{
		//SceneManager.LoadScene("Serve");
		Debug.Log("Middlehand selected");
	}
	public void OnBackhand()
	{
		//SceneManager.LoadScene("Serve");
		Debug.Log("Backhand selected");
	}
}
