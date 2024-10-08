using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadLobby : MonoBehaviour
{
    public GameObject fade;
    public GameObject music1;
    public GameObject music2;
    public PlayerMovement pm;

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(LoadLobbyAfterDelay(3f));
        }
    }

    IEnumerator LoadLobbyAfterDelay(float delay)
    {
        fade.SetActive(true);
        music1.SetActive(false);
        music2.SetActive(false);
        pm.canOnlyMoveCam = false;
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Lobby");
    }
}
