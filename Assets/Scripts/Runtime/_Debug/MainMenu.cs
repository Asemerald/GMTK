using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        throw new NotImplementedException();
    }

    private IEnumerator CallStartGameDelayed()
    {
        yield return new WaitForSeconds(5);
        StartGame();
    }
}
