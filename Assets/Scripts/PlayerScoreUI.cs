using TMPro;
using UnityEngine;

public class PlayerScoreUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _playerNameTxt;
    [SerializeField]
    private TextMeshProUGUI _playerScoreTxt;

    public void Setup(string playerName, int score)
    {
        _playerNameTxt = transform.Find("Text(TMP) - Name").GetComponent<TextMeshProUGUI>();
        _playerScoreTxt = transform.Find("Text(TMP) - Score").GetComponent<TextMeshProUGUI>();


    }
}
