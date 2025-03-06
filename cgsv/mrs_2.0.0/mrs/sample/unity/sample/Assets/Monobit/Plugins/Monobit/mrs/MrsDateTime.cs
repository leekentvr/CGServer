using System;

namespace mrs {
public class DateTime {
    public static mrs.DateTime Now(){
        mrs.DateTime date_time = new mrs.DateTime();
        date_time.Set();
        return date_time;
    }
    
    protected UInt16 m_Year;
    protected byte   m_Month;
    protected byte   m_Date;
    protected byte   m_Hour;
    protected byte   m_Min;
    protected byte   m_Sec;
    protected UInt32 m_Usec;
    
    public UInt16 GetYear(){ return m_Year; }
    
    public byte GetMonth(){ return m_Month; }
    
    public byte GetDate(){ return m_Date; }
    
    public byte GetHour(){ return m_Hour; }
    
    public byte GetMin(){ return m_Min; }
    
    public byte GetSec(){ return m_Sec; }
    
    public UInt32 GetUsec(){ return m_Usec; }
    
    public DateTime( UInt16 year = 0, byte month = 0, byte date = 0, byte hour = 0, byte min = 0, byte sec = 0, UInt32 usec = 0 ){
        m_Year  = year;
        m_Month = month;
        m_Date  = date;
        m_Hour  = hour;
        m_Min   = min;
        m_Sec   = sec;
        m_Usec  = usec;
    }
    
    public DateTime( Time time ){
        Set( time );
    }
    
    public virtual void Set(){
        mrs.Time time = new mrs.Time();
        time.Set();
        Set( time );
    }
    
    public virtual void Set( mrs.Time time ){
        System.DateTime local_time = mrs.Time.UnixEpoch.AddSeconds( time.GetSec() ).ToLocalTime();
        
        m_Year  = (UInt16)local_time.Year;
        m_Month = (byte)local_time.Month;
        m_Date  = (byte)local_time.Day;
        m_Hour  = (byte)local_time.Hour;
        m_Min   = (byte)local_time.Minute;
        m_Sec   = (byte)local_time.Second;
        m_Usec  = (UInt32)time.GetUsec();
    }
    
    public override String ToString(){
        return String.Format( "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}.{6:000000}",
            m_Year, m_Month, m_Date, m_Hour, m_Min, m_Sec, m_Usec );
    }
}
}
