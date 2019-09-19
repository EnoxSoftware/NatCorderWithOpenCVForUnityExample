using UnityEngine;
using UnityEngine.SceneManagement;

namespace NatCorderWithOpenCVForUnityExample
{
    public class ShowLicense : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("NatCorderWithOpenCVForUnityExample");
        }
    }
}
