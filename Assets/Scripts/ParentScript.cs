using UnityEngine;

public class ChaplinScripts : MonoBehaviour
{
    // Parent Script for the project
    // Mainly for debugging

    #region Debug
        #region Debug - Variables
            [Header("Debugging Mode")]
                /// <summary>
                /// If true, shows the debug logs and warnings
                /// </summary>
                [SerializeField] [Tooltip("Show Debug Logs for the script")]
                private bool debugMode = false;


                /// <summary>
                /// If true, shows the warnings even if debugMode is off
                /// </summary>
                [SerializeField] [Tooltip("Show Warnings for the script")]
                private bool warningBypass = true;
        #endregion

        #region Debug - Methods
            /// <summary>
            /// Use log as opposed to Debug.Log();. <br/><br/>
            /// This should be for displaying data that needs checking: <br/>
            /// (I.e: Gun Damage is a random value between Min and Max, this log can display the actual damage value)
            /// </summary>
            /// <param name="msg"> The message to display to console </param>
            protected void Log(string msg)
            {
                if(debugMode)
                    Debug.Log($"Log from GameObject: {name}, Script: {GetType().Name} \n Message: \n{msg}");
            }


            /// <summary>
            /// Use Warning as opposed to Debug.LogWarning();. <br/><br/>
            /// This should be for displaying Breakpoints and Common Bug handling that does not stop execution: <br/>
            /// (I.e: Gun Object is missing so display it, Random value is outside of predefined range)
            /// </summary>
            /// <param name="msg">  The warning message to display to console  </param>
            protected void Warning(string msg)
            {
                if(debugMode || warningBypass)
                    Debug.LogWarning($"WARNING! From GameObject: {name}, Script: {GetType().Name} \n Message: \n{msg}");
            }


            /// <summary>
            /// Use Warning as opposed to Debug.LogError();. <br/><br/>
            /// This should be for displaying things that the game requires to be running: <br/>
            /// (I.e: A method is not working properly or script is not assigned)
            /// </summary>
            /// <param name="msg">  The ERROR message to display to console  </param>
            protected void Error(string msg)
            {
                Debug.LogError($"ERROR!!!! FIX IMMEDIATELY!!!! \nFrom GameObject: {name}, Script: {GetType().Name} \nMessage: \n{msg}");
            }
        #endregion

    #endregion


}
