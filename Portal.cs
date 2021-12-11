using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
   
    [SerializeField] private ParticleSystem _particles;

    public void Teleport(int sceneID)
    {
        UnlockNextLevel();
        SceneManager.LoadScene(sceneID);
    }

    public void PlayParticles() 
    {
        _particles.Play();
    }

    private void UnlockNextLevel()
    {
        int currentLevelID = SceneManager.GetActiveScene().buildIndex;

        if(currentLevelID >= PlayerPrefs.GetInt("levels"))
        {
            if(currentLevelID + 1 != SceneManager.sceneCountInBuildSettings)
                PlayerPrefs.SetInt("levels", currentLevelID + 1);
        }
    }    
}
