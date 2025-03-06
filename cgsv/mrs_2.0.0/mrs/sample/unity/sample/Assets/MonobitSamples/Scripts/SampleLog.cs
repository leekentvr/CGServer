using UnityEngine;
using System.Collections;
using System;

public class SampleLog : MonoBehaviour {
    void Awake(){
        gameObject.AddComponent< mrs.ScreenLogger >();
    }
    
    void OnGUI(){
        if ( GUILayout.Button( "Back", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
            mrs.Utility.LoadScene( "SampleMain" );
        }
    }
    
    [AOT.MonoPInvokeCallback(typeof(Mrs.MrsLogOutputCallback))]
    private static void output( Mrs.MrsLogLevel level, String msg ){
        Mrs.mrs_console_log( level, String.Format( "output function called: {0}", msg ) );
    }
    
    void Start(){
        Mrs.MRS_LOG_DEBUG( "setting level: MrsLogLevel.DEBUG(default)" );
        Mrs.MRS_LOG_EMERG(   "EMERG   1" );
        Mrs.MRS_LOG_ALERT(   "ALERT   1" );
        Mrs.MRS_LOG_CRIT(    "CRIT    1" );
        Mrs.MRS_LOG_ERR(     "ERR     1" );
        Mrs.MRS_LOG_WARNING( "WARNING 1" );
        Mrs.MRS_LOG_NOTICE(  "NOTICE  1" );
        Mrs.MRS_LOG_INFO(    "INFO    1" );
        Mrs.MRS_LOG_DEBUG(   "DEBUG   1" );
        
        Mrs.MRS_LOG_DEBUG( "setting level: MrsLogLevel.WARNING" );
        Mrs.mrs_set_output_log_level( Mrs.MrsLogLevel.WARNING );
        Mrs.MRS_LOG_EMERG(   "EMERG   2" );
        Mrs.MRS_LOG_ALERT(   "ALERT   2" );
        Mrs.MRS_LOG_CRIT(    "CRIT    2" );
        Mrs.MRS_LOG_ERR(     "ERR     2" );
        Mrs.MRS_LOG_WARNING( "WARNING 2" );
        Mrs.MRS_LOG_NOTICE(  "NOTICE  2" );
        Mrs.MRS_LOG_INFO(    "INFO    2" );
        Mrs.MRS_LOG_DEBUG(   "DEBUG   2" );
        Mrs.mrs_set_output_log_level( Mrs.MrsLogLevel.DEBUG );
        Mrs.MRS_LOG_DEBUG( "setting level: MRS_LOG_LEVEL_DEBUG" );
        
        Mrs.MRS_LOG_DEBUG( "setting output callback: output" );
        Mrs.MrsLogOutputCallback default_output = Mrs.mrs_get_log_callback();
        Mrs.mrs_set_log_callback( output );
        Mrs.MRS_LOG_EMERG(   "EMERG   3" );
        Mrs.MRS_LOG_ALERT(   "ALERT   3" );
        Mrs.MRS_LOG_CRIT(    "CRIT    3" );
        Mrs.MRS_LOG_ERR(     "ERR     3" );
        Mrs.MRS_LOG_WARNING( "WARNING 3" );
        Mrs.MRS_LOG_NOTICE(  "NOTICE  3" );
        Mrs.MRS_LOG_INFO(    "INFO    3" );
        Mrs.MRS_LOG_DEBUG(   "DEBUG   3" );
        Mrs.mrs_set_log_callback( default_output );
        Mrs.MRS_LOG_DEBUG( "setting output callback: default" );
    }
}
