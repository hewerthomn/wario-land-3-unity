using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Continue : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.anyKey.isPressed)
            LoadScene();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("OutOfTheWoods");
    }
}
