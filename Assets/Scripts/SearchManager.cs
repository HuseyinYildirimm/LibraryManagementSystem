using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public TextMeshProUGUI resultText;
    public ScrollRect scrollView;
    public GameObject contentPanel;

    public void Update()
    {
        Search();
    }

    void Search()
    {
        string searchValue = searchInputField.text.ToLower();

        foreach (Transform child in contentPanel.transform)
        {
            if (string.IsNullOrEmpty(searchValue))
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                bool shouldShowTitle = child.GetComponent<BookProperties>().titleText.text.ToLower().Contains(searchValue);
                bool shouldShowAuthor = child.GetComponent<BookProperties>().authorText.text.ToLower().Contains(searchValue);

                if (shouldShowAuthor || shouldShowTitle)
                    child.gameObject.SetActive(true);

                else
                    child.gameObject.SetActive(false);
            }
        }
    }

}
