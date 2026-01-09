using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class mainmenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hiscoreText;
    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null)
        {
            hiscoreText.text = "High Score: " + GameManager.Instance.LoadHiscore().ToString();
        }
    }
}

