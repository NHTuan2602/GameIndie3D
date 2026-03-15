using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class ScamComputer : MonoBehaviour
{
    [Header("Cài đặt Tương tác")]
    public float holdTimeRequired = 1.5f;
    private float currentHoldTime = 0f;

    [Header("Giao diện & Kết nối")]
    public GameObject scamUIPanel;
    public PlayerMovement player;

    [Header("Giao diện 3D (Hiện khi nhìn vào)")]
    public GameObject promptCanvas;
    public Image loadingCircle;

    private bool isWorking = false;

    void Start()
    {
        if (scamUIPanel != null) scamUIPanel.SetActive(false);
        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (loadingCircle != null) loadingCircle.fillAmount = 0f;
    }

    void Update()
    {
        if (player == null || player.playerCamera == null) return;

        if (isWorking)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopWorking();
            }
            return;
        }

        Ray ray = player.playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        bool isLookingAtScreen = false;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            // ==============================================================
            // PHÉP THUẬT NÂNG CẤP: Xuyên qua các lớp con để tìm thằng cha!
            // ==============================================================
            ScamComputer hitComputer = hit.collider.GetComponentInParent<ScamComputer>();

            // Nếu tia đập trúng cái máy tính này (hoặc bất kỳ bộ phận con nào của nó)
            if (hitComputer != null && hitComputer == this)
            {
                isLookingAtScreen = true;

                if (promptCanvas != null && !promptCanvas.activeSelf)
                {
                    promptCanvas.SetActive(true);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    currentHoldTime += Time.deltaTime;

                    if (loadingCircle != null)
                    {
                        loadingCircle.fillAmount = currentHoldTime / holdTimeRequired;
                    }

                    if (currentHoldTime >= holdTimeRequired)
                    {
                        StartWorking();
                    }
                }
                else
                {
                    currentHoldTime = 0f;
                    if (loadingCircle != null) loadingCircle.fillAmount = 0f;
                }
            }
        }

        if (!isLookingAtScreen)
        {
            currentHoldTime = 0f;
            if (promptCanvas != null) promptCanvas.SetActive(false);
            if (loadingCircle != null) loadingCircle.fillAmount = 0f;
        }
    }

    void StartWorking()
    {
        isWorking = true;
        currentHoldTime = 0f;

        if (scamUIPanel != null) scamUIPanel.SetActive(true);
        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (loadingCircle != null) loadingCircle.fillAmount = 0f;

        player.canLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void StopWorking()
    {
        isWorking = false;

        if (scamUIPanel != null) scamUIPanel.SetActive(false);

        player.canLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}