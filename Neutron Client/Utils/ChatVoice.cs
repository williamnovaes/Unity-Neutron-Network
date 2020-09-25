using UnityEngine;
using VoiceAPI;

public class ChatVoice : MonoBehaviour
{
    private string path;
    [SerializeField] private bool isEnable = true;
    [SerializeField] private int Frequency = 8000;
    [SerializeField] private int bitRate = 16;
    [SerializeField] private int KeyCode = 84;
    private void Start()
    {
        if (isEnable)
        {
#if UNITY_STANDALONE_WIN
            path = $"{Application.dataPath}\\VNeutron\\VNeutron.exe";
            if (!WinAPI.InitWindowsSoundAPI(path, NeutronConstants._IEPListen.Port, NeutronConstants._IEPSend.Address.ToString(), bitRate, Frequency, KeyCode))
            {
                Debug.LogError("SDK Sound Win API Not Found!");
            }
#endif
        }
    }
}