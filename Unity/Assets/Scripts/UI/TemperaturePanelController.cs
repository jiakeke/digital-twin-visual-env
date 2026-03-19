using UnityEngine;

public class TemperaturePanelController : MonoBehaviour
{
    [SerializeField] private GameObject temperaturePanel;
    [SerializeField] private GameObject latestContent;
    [SerializeField] private GameObject historyContent;

    public void ToggleTemperaturePanel()
    {
        if (temperaturePanel == null) return;

        bool newState = !temperaturePanel.activeSelf;
        temperaturePanel.SetActive(newState);

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
        if (latestContent != null) latestContent.SetActive(false);
        if (historyContent != null) historyContent.SetActive(true);
    }

    public void HideAllContent()
    {
        if (latestContent != null) latestContent.SetActive(false);
        if (historyContent != null) historyContent.SetActive(false);
    }
}