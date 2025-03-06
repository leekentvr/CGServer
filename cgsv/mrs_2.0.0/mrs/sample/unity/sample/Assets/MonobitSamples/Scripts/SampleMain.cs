using UnityEngine;
using System.Collections;
using System;

public class SampleMain : MonoBehaviour {
    void OnGUI(){
#if ! UNITY_WEBGL
        if ( GUILayout.Button( "BaseLoop", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
            mrs.Utility.LoadScene( "SampleBaseLoop" );
        }
#endif
        
        if ( GUILayout.Button( "Log", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
            mrs.Utility.LoadScene( "SampleLog" );
        }
        
#if ! UNITY_WEBGL
        if ( GUILayout.Button( "EchoServer", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
            mrs.Utility.LoadScene( "SampleEchoServer" );
        }
#endif
        
        if ( GUILayout.Button( "EchoClient", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
            mrs.Utility.LoadScene( "SampleEchoClient" );
        }
    }
}
