using UnityEngine;

public class HumidiityPanelController : MonoBehaviour
{
    [SerializeField] private GameObject humidityPanel;
    [SerializeField] private GameObject latestContent;
    [SerializeField] private GameObject historyContent;

    public void ToggleHumidityPanel()
    {
        if (humidityPanel == null) return;

        bool newState = !humidityPanel.activeSelf;
        humidityPanel.SetActive(newState);

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
