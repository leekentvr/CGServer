using System;

namespace mrs {
public class Time {
    public static System.DateTime UnixEpoch = new System.DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
    
    public static mrs.Time Now(){
        mrs.Time time = new mrs.Time();
        time.Set();
        return time;
    }
    
    protected UInt64 m_Sec;
    protected UInt32 m_Usec;
    
    public UInt64 GetSec(){ return m_Sec; }
    
    public UInt32 GetUsec(){ return m_Usec; }
    
    public Time( UInt64 sec = 0, UInt32 usec = 0 ){
        Set( sec, usec );
    }
    
    public Time( double time ){
        Set( time );
    }
    
    public virtual void Set(){
        System.DateTime date_time = System.DateTime.Now.ToUniversalTime();
        
        m_Sec  = (UInt64)( date_time - UnixEpoch ).TotalSeconds;
        m_Usec = (UInt32)( date_time.Millisecond * 1000 );
    }
    
    public virtual void Set( UInt64 sec, UInt32 usec ){
        m_Sec  = sec;
        m_Usec = usec;
    }
    
    public virtual void Set( double time ){
        UInt64 sec = (UInt64)time;
        UInt32 usec = (UInt32)(( time - sec ) * 1000000);
        Set( sec, usec );
    }
    
    public override String ToString(){
        return String.Format( "{0}.{1:000000}", m_Sec, m_Usec );
    }
    
    public static mrs.Time operator -( mrs.Time time1, mrs.Time time2 ){
        UInt64 diff_time = ( time1.m_Sec - time2.m_Sec ) * 1000000 + time1.m_Usec - time2.m_Usec;
        return new mrs.Time( diff_time / 1000000, (UInt32)( diff_time % 1000000 ) );
    }
}
}
