using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //capping to 30 fps for now so it doesnt eat my laptop battery in the editor
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartMultiplayer()
    {
        SceneManager.LoadScene("Loading");
    }
}
