using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameUIManager : MonoBehaviour
{
    bool isClicked;
    Vector2 clickPos;
    Vector3 cameraPosBeforeClick;

    Vector2 navVector;

    private void Update()
    {
        Camera.main.transform.position += (Vector3)navVector * Camera.main.orthographicSize * 0.2f;
        if (isClicked)
        {
            Camera.main.transform.position = cameraPosBeforeClick + ((Vector3)clickPos - Input.mousePosition) * 0.02f * 0.2f * Camera.main.orthographicSize;
        }
    }
    //public void RegularSpeed() { Time.timeScale = 1; timeScaleText.text = $"x{(int)Time.timeScale}"; }
    //public void Accelerate() { if (Time.timeScale < 16) Time.timeScale *= 2; else Time.timeScale = 1; timeScaleText.text = $"x{(int)Time.timeScale}"; }

    void OnNavigate(InputValue value)
    {
        navVector = value.Get<Vector2>();
    }

    void OnScrollWheel(InputValue value)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - value.Get<Vector2>().y * 0.01f, 1, 30);
    }

    void OnClick(InputValue value)
    {
        isClicked = value.Get<float>() > 0;
        if (isClicked)
        {
            clickPos = Input.mousePosition;
            cameraPosBeforeClick = Camera.main.transform.position;
        }
    }
}
