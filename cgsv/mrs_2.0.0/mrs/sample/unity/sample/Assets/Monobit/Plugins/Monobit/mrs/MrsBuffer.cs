using System.Collections.Generic;
using System;

namespace mrs {
public class Buffer {
    protected byte[] m_Data;
    protected UInt32 m_WriteLen;
    protected UInt32 m_ReadLen;
    protected UInt32 m_AlignLen;
    
    public byte[] GetData(){
        UInt32 data_len = GetDataLen();
        byte[] data = new byte[ data_len ];
        System.Buffer.BlockCopy( m_Data, (int)m_ReadLen, data, 0, (int)data_len );
        return data;
    }
    
    public UInt32 GetDataLen(){ return m_WriteLen - m_ReadLen; }
    
    public UInt32 GetAlignLen(){ return m_AlignLen; }
    public void SetAlignLen( UInt32 value ){ m_AlignLen = value; }
    
    public Buffer( UInt32 data_len = 0 ){
        m_Data = new byte[ (int)data_len ];
        m_WriteLen = 0;
        m_ReadLen = 0;
        m_AlignLen = 1024;
    }
    
    public Buffer( Buffer buffer ) : this( (UInt32)buffer.GetDataLen() ){
        WriteBuffer( buffer );
    }
    
    protected virtual void WriteCheck( UInt32 data_len ){
        UInt32 max_len = m_WriteLen + data_len;
        if ( m_Data.Length < (int)max_len ){
            if ( 1 < m_AlignLen ){
                UInt32 block_num = max_len / m_AlignLen;
                if ( 0 < max_len % m_AlignLen ) ++block_num;
                max_len = block_num * m_AlignLen;
            }
            Array.Resize< byte >( ref m_Data, (int)max_len );
        }
    }
    
    public virtual bool Write( byte[] data ){
        return Write( data, (UInt32)data.Length );
    }
    
    public virtual bool Write( byte[] data, UInt32 data_len ){
        WriteCheck( data_len );
        System.Buffer.BlockCopy( data, 0, m_Data, (int)m_WriteLen, (int)data_len );
        m_WriteLen += data_len;
        return true;
    }
    
    public virtual bool WriteBool( bool data ){
        return WriteUInt8( data ? (byte)1 : (byte)0 );
    }
    
    public virtual bool WriteUInt8( byte data ){
        WriteCheck( 1 );
        m_Data[ m_WriteLen++ ] = data;
        return true;
    }
    
    public virtual bool WriteUInt16( UInt16 data ){
        WriteCheck( 2 );
        // Little Endian only
        m_Data[ m_WriteLen++ ] = (byte)( data & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 8 ) & 0xFF );
        return true;
    }
    
    public virtual bool WriteUInt32( UInt32 data ){
        WriteCheck( 4 );
        // Little Endian only
        m_Data[ m_WriteLen++ ] = (byte)( data & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 8 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 16 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 24 ) & 0xFF );
        return true;
    }
    
    public virtual bool WriteUInt64( UInt64 data ){
        WriteCheck( 8 );
        // Little Endian only
        m_Data[ m_WriteLen++ ] = (byte)( data & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 8 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 16 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 24 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 32 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 40 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 48 ) & 0xFF );
        m_Data[ m_WriteLen++ ] = (byte)( ( data >> 56 ) & 0xFF );
        return true;
    }
    
    public virtual bool WriteInt8( sbyte data ){
        return WriteUInt8( (byte)data );
    }
    
    public virtual bool WriteInt16( Int16 data ){
        return WriteUInt16( (UInt16)data );
    }
    
    public virtual bool WriteInt32( Int32 data ){
        return WriteUInt32( (UInt32)data );
    }
    
    public virtual bool WriteInt64( Int64 data ){
        return WriteUInt64( (UInt64)data );
    }
    
    public virtual bool WriteFloat( float data ){
        return Write( BitConverter.GetBytes( data ) );
    }
    
    public virtual bool WriteDouble( double data ){
        return Write( BitConverter.GetBytes( data ) );
    }
    
    public virtual bool WriteBuffer( byte[] data, UInt32 data_len ){
        if ( ! WriteUInt32( data_len ) ) return false;
        if ( ! Write( data, data_len ) ){
            Unwrite( 4 );
            return false;
        }
        return true;
    }
    
    public virtual bool WriteBuffer( Buffer buffer ){
        return WriteBuffer( buffer.GetData(), buffer.GetDataLen() );
    }
    
    public virtual bool WriteTime( UInt64 sec, UInt32 usec ){
    if ( ! WriteUInt64( sec ) ) return false;
        if ( ! WriteUInt32( usec ) ){
            Unwrite( 8 );
            return false;
        }
        return true;
    }
    
    public virtual bool WriteTime( mrs.Time time ){
        return WriteTime( time.GetSec(), time.GetUsec() );
    }
    
    protected virtual bool ReadCheck( UInt32 data_len ){
        return ( data_len <= GetDataLen() );
    }
    
    public virtual bool Read( byte[] data, UInt32 data_len ){
        if ( ! ReadCheck( data_len ) ) return false;
        
        if ( null != data ) System.Buffer.BlockCopy( m_Data, (int)m_ReadLen, data, 0, (int)data_len );
        m_ReadLen += data_len;
        return true;
    }
    
    public virtual bool ReadBool(){
        return ( 0 != ReadUInt8() );
    }
    
    public virtual byte ReadUInt8(){
        if ( ! ReadCheck( 1 ) ) return 0;
        
        return m_Data[ m_ReadLen++ ];
    }
    
    public virtual UInt16 ReadUInt16(){
        if ( ! ReadCheck( 2 ) ) return 0;
        
        // Little Endian only
        UInt16 data = m_Data[ m_ReadLen++ ];
        data += (UInt16)( (UInt16)m_Data[ m_ReadLen++ ] << 8 );
        return data;
    }
    
    public virtual UInt32 ReadUInt32(){
        if ( ! ReadCheck( 4 ) ) return 0;
        
        // Little Endian only
        UInt32 data = m_Data[ m_ReadLen++ ];
        data += (UInt32)( (UInt32)m_Data[ m_ReadLen++ ] << 8 );
        data += (UInt32)( (UInt32)m_Data[ m_ReadLen++ ] << 16 );
        data += (UInt32)( (UInt32)m_Data[ m_ReadLen++ ] << 24 );
        return data;
    }
    
    public virtual UInt64 ReadUInt64(){
        if ( ! ReadCheck( 8 ) ) return 0;
        
        // Little Endian only
        UInt64 data = m_Data[ m_ReadLen++ ];
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 8 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 16 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 24 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 32 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 40 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 48 );
        data += (UInt64)( (UInt64)m_Data[ m_ReadLen++ ] << 56 );
        return data;
    }
    
    public virtual sbyte ReadInt8(){
        return (sbyte)ReadUInt8();
    }
    
    public virtual Int16 ReadInt16(){
        return (Int16)ReadUInt16();
    }
    
    public virtual Int32 ReadInt32(){
        return (Int32)ReadUInt32();
    }
    
    public virtual Int64 ReadInt64(){
        return (Int64)ReadUInt64();
    }
    
    public virtual float ReadFloat(){
        if ( ! ReadCheck( 4 ) ) return 0.0f;
        
        float data = BitConverter.ToSingle( m_Data, (int)m_ReadLen );
        m_ReadLen += 4;
        return data;
    }
    
    public virtual double ReadDouble(){
        if ( ! ReadCheck( 8 ) ) return 0.0;
        
        double data = BitConverter.ToDouble( m_Data, (int)m_ReadLen );
        m_ReadLen += 8;
        return data;
    }
    
    public virtual Buffer ReadBuffer(){
        UInt32 len = ReadUInt32();
        Buffer buffer = new Buffer( len );
        if ( 0 < len ){
            buffer.Write( GetData(), len );
            m_ReadLen += len;
        }
        return buffer;
    }
    
    public virtual mrs.Time ReadTime(){
        UInt64 sec  = ReadUInt64();
        UInt32 usec = ReadUInt32();
        return new mrs.Time( sec, usec );
    }
    
    public virtual void Unread( UInt32 data_len ){
        if ( m_ReadLen < data_len ) data_len = m_ReadLen;
        m_ReadLen -= data_len;
    }
    
    public virtual void Unwrite( UInt32 data_len ){
        if ( m_WriteLen < data_len ) data_len = m_WriteLen;
        m_WriteLen -= data_len;
    }
    
    public virtual void Delete( UInt32 data_len ){
        if ( 0 == data_len ) return;
        
        if ( m_WriteLen < data_len ) data_len = m_WriteLen;
        UInt32 move_len = m_WriteLen - data_len;
        if ( 0 < move_len ){
            byte[] data = new byte[ move_len ];
            System.Buffer.BlockCopy( m_Data, (int)data_len, data, 0, (int)move_len );
            m_Data = data;
        }
        m_WriteLen -= data_len;
        m_ReadLen = ( data_len < m_ReadLen ) ? m_ReadLen - data_len : 0;
    }
    
    public virtual void Clear(){
        m_Data = new byte[ 0 ];
        m_WriteLen = 0;
        m_ReadLen  = 0;
    }
}
}
