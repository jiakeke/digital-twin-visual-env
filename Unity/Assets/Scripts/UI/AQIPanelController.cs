using UnityEngine;

public class AQIPanelController : MonoBehaviour
{
    [SerializeField] private GameObject aqiPanel;
    [SerializeField] private GameObject latestContent;
    [SerializeField] private GameObject historyContent;
    [SerializeField] private AQIBarUI aqiLatestBar;

    public void ToggleAQIPanel()
    {
        if (aqiPanel == null) return;

        bool newState = !aqiPanel.activeSelf;

        //clear ball
        if (!newState && aqiLatestBar != null)
        {
            aqiLatestBar.ClearBalls();
        }
        aqiPanel.SetActive(newState);

        if (newState)
        {
            HideAllContent();
        }
    }

    public void ShowLatestContent()
    {
        if (latestContent != null) latestContent.SetActive(true);
        if (historyContent != null) historyContent.SetActive(false);
    }

    public void ShowHistoryContent()
    {
        if (aqiLatestBar != null)
        {
            aqiLatestBar.ClearBalls();
        }

        if (latestContent != null) latestContent.SetActive(false);
        if (historyContent != null) historyContent.SetActive(true);
    }

    public void HideAllContent()
    {
        if (latestContent != null) latestContent.SetActive(false);
        if (historyContent != null) historyContent.SetActive(false);
    }
}