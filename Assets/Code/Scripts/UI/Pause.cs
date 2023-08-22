using UnityEngine;

namespace FR8Runtime.UI
{
    public static class Pause
    {
        private static int pauseStack;
        private static float timeScale;
        private static CursorLockMode cursorLockMode;

        public static void Push()
        {
            if (pauseStack == 0)
            {
                timeScale = Time.timeScale;
                cursorLockMode = Cursor.lockState;
                
                Time.timeScale = 0.0f;
                Cursor.lockState = CursorLockMode.None;
            }
            
            pauseStack++;
        }

        public static void Pop()
        {
            if (pauseStack == 1)
            {
                Time.timeScale = timeScale;
                Cursor.lockState = cursorLockMode;
            }
            
            pauseStack--;
        }
    }
}