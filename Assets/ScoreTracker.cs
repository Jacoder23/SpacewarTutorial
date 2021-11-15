using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    [HideInInspector]
    public int player1Score;
    public int player2Score;

    private static GameObject instance;

    private void Start()
    {
        GameObject canvas = transform.parent.gameObject;
        DontDestroyOnLoad(canvas);
        if (instance == null)
            instance = canvas;
        else
            Destroy(canvas);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Text>().text = player1Score + ":" + player2Score;
    }
}
