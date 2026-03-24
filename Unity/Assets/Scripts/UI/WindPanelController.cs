using UnityEngine;

public class WindPanelController : MonoBehaviour
{
    [SerializeField] private GameObject windPanel;
    [SerializeField] private GameObject latestContent;
    [SerializeField] private GameObject historyContent;

    public void ToggleWindPanel()
    {
        if (windPanel == null) return;

        bool newState = !windPanel.activeSelf;
        windPanel.SetActive(newState);

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
