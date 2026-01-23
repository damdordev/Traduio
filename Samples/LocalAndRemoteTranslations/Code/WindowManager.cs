using UnityEngine;

namespace Damdor.Traduio.Samples
{
    public class WindowManager
    {
        private GameObject currentWindow;
        
        public void ChangeWindow(GameObject window)
        {
            if (currentWindow != null)
            {
                currentWindow.gameObject.SetActive(false);
                currentWindow = null;
            }

            currentWindow = window;
            currentWindow.gameObject.SetActive(true);
        }
    }
}