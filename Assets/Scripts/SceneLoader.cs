using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void LoadLevel01()
    {
        SceneManager.LoadScene("Level01");
    }
}
