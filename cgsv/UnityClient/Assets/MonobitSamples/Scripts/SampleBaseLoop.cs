using UnityEngine;
using System.Collections;
using System;

public class SampleBaseLoop : Mrs {
    void Awake(){
        gameObject.AddComponent< mrs.ScreenLogger >();
    }
    
    protected int m_Count;
    
    void Start(){
        mrs_initialize();
        MRS_LOG_DEBUG( "[{0}] start", mrs.DateTime.Now().ToString() );
        m_Count = 0;
    }
    
    void Update(){
        if ( m_Count < 3 ){
            mrs_update();
            ++m_Count;
            MRS_LOG_DEBUG( "[{0}] zzz {1}", mrs.DateTime.Now().ToString(), m_Count );
            mrs_sleep( 1000 );
        }else{
            mrs.Utility.LoadScene( "SampleMain" );
        }
    }
    
    void End(){
        MRS_LOG_DEBUG( "[{0}] end", mrs.DateTime.Now().ToString() );
        mrs_finalize();
    }
    
    void OnDestroy(){
        End();
    }
}
