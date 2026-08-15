using TMPro;
using UnityEngine;

public class LangText : MonoBehaviour
{
    [SerializeField] private string textRus;
    [SerializeField] private string textEng;
    
    private TextMeshProUGUI _textMash;
    private string _language = "ru";
    
    private void Start()
    {
        _textMash = GetComponent<TextMeshProUGUI>();
    }
    
    private void OnEnable()
    {
        UpdateLanguage();
    }
    
    private void Update()
    {
        if (_language != SaveGame.Instance.language)
        {
            UpdateLanguage();
        }
    }
    
    private void UpdateLanguage()
    {
        if (SaveGame.Instance == null || _textMash == null)
        {
            return;
        }

        _language = SaveGame.Instance.language;
        _textMash.text = _language == "ru" ? textRus : textEng;
    }
}
