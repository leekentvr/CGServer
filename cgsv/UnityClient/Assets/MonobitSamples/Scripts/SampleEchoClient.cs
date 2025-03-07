using UnityEngine;
using System.Collections;
using System;

using MrsServer = System.IntPtr;
using MrsConnection = System.IntPtr;
using MrsCipher = System.IntPtr;
public class SampleEchoClient : Mrs {
    protected static String g_ArgConnectionType;
    protected static String g_ArgIsKeyExchange;
    protected static String g_ArgIsEncryptRecords;
    protected static String g_ArgWriteDataLen;
    protected static String g_ArgWriteCount;
    protected static String g_ArgConnections;
    protected static String g_ArgServerAddr;
    protected static String g_ArgServerPort;
    protected static String g_ArgTimeoutMsec;
    protected static String g_ArgIsValidRecord;
    protected static String g_ArgConnectionPath;
    
    protected static bool        g_IsKeyExchange;
    protected static bool        g_IsEncryptRecords;
    protected static UInt16      g_RecordOptions;
    protected static UInt32      g_WriteDataLen;
    protected static UInt32      g_WriteCount;
    protected static UInt32      g_Connections;
    protected static UInt32      g_ReadCount;
    protected static String      g_Header;
    protected static mrs.Connect g_Connect;
    protected static bool        g_IsValidRecord;
    
    static SampleEchoClient(){
#if UNITY_WEBGL
        g_ArgConnectionType   = "3";
        g_ArgIsKeyExchange    = "0";
#else
        g_ArgConnectionType   = "1";
        g_ArgIsKeyExchange    = "0";
#endif
        g_ArgIsEncryptRecords = "0";
        g_ArgWriteDataLen     = "1024";
        g_ArgWriteCount       = "10";
        g_ArgConnections      = "1";
        g_ArgServerAddr       = "127.0.0.1";
#if UNITY_WEBGL
        g_ArgServerPort       = "22223";
#else
        g_ArgServerPort       = "22222";
#endif
        g_ArgTimeoutMsec      = "5000";
        g_ArgIsValidRecord    = "1";
        g_ArgConnectionPath   = "/";
    }
    
    protected bool m_IsRunning;
    
    void Awake(){
        gameObject.AddComponent< mrs.ScreenLogger >();
        
        m_IsRunning = false;
        g_ReadCount = 0;
        g_Connect = new mrs.Connect();
    }
    
    void OnGUI(){
        if ( ! m_IsRunning ){
            GUILayout.BeginHorizontal();
            GUILayout.Label( "ConnectionType:" );
            g_ArgConnectionType = GUILayout.TextField( g_ArgConnectionType );
            GUILayout.Space( 30 );
#if ! UNITY_WEBGL
            GUILayout.Label( "IsKeyExchange:" );
            g_ArgIsKeyExchange = GUILayout.TextField( g_ArgIsKeyExchange );
            GUILayout.Space( 30 );
#endif
            GUILayout.Label( "IsEncryptRecords:" );
            g_ArgIsEncryptRecords = GUILayout.TextField( g_ArgIsEncryptRecords );
            GUILayout.EndHorizontal();
            
            GUILayout.Space( 30 );
            
            GUILayout.BeginHorizontal();
            GUILayout.Label( "WriteDataLen:" );
            g_ArgWriteDataLen = GUILayout.TextField( g_ArgWriteDataLen );
            GUILayout.Space( 30 );
            GUILayout.Label( "WriteCount:" );
            g_ArgWriteCount = GUILayout.TextField( g_ArgWriteCount );
            GUILayout.Space( 30 );
            GUILayout.Label( "Connections:" );
            g_ArgConnections = GUILayout.TextField( g_ArgConnections );
            GUILayout.EndHorizontal();
            
            GUILayout.Space( 30 );
            
            GUILayout.BeginHorizontal();
            GUILayout.Label( "ServerAddr:" );
            g_ArgServerAddr = GUILayout.TextField( g_ArgServerAddr );
            GUILayout.Space( 30 );
            GUILayout.Label( "ServerPort:" );
            g_ArgServerPort = GUILayout.TextField( g_ArgServerPort );
            GUILayout.Space( 30 );
            GUILayout.Label( "TimeoutMsec:" );
            g_ArgTimeoutMsec = GUILayout.TextField( g_ArgTimeoutMsec );
            GUILayout.Space( 30 );
            GUILayout.Label( "IsValidRecord:" );
            g_ArgIsValidRecord = GUILayout.TextField( g_ArgIsValidRecord );
            GUILayout.Space( 30 );
            GUILayout.Label( "ConnectionPath:" );
            g_ArgConnectionPath = GUILayout.TextField( g_ArgConnectionPath );
            GUILayout.EndHorizontal();
            
            GUILayout.Space( 30 );
            
            if ( GUILayout.Button( "Start Echo Client", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
                m_IsRunning = true;
                StartEchoClient();
            }
        }else{
            if ( GUILayout.Button( "Back", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
                mrs.Utility.LoadScene( "SampleMain" );
            }
        }
    }
    
    private static String connection_type_to_string( MrsConnection connection ){
        MrsConnectionType type = mrs_connection_get_type( connection );
        switch ( type ){
        case MrsConnectionType.NONE:{ return "NONE"; }
        case MrsConnectionType.TCP:{ return "TCP"; }
        case MrsConnectionType.UDP:{ return "UDP"; }
        case MrsConnectionType.WS:{ return "WS"; }
        case MrsConnectionType.WSS:{ return "WSS"; }
        case MrsConnectionType.TCP_SSL:{ return "TCP_SSL"; }
        case MrsConnectionType.MRU:{ return "MRU"; }
        }
        return "INVALID";
    }
    
    private static void read_echo( MrsConnection connection, byte[] payload, UInt32 payload_len ){
        mrs.Time read_time = new mrs.Time();
        read_time.Set();
        mrs.Buffer buffer = new mrs.Buffer();
        buffer.Write( payload, payload_len );
        while ( 0 < buffer.GetDataLen() ){
            mrs.Time write_time = buffer.ReadTime();
            MRS_LOG_DEBUG( "read_echo data={0} data_len={1} diff_time={2}({3} - {4})",
                ToString( buffer.GetData() ), g_WriteDataLen,
                ( read_time - write_time ).ToString(), read_time.ToString(), write_time.ToString() );
            if ( ! buffer.Read( null, g_WriteDataLen ) ){
                MRS_LOG_ERR( "Lost data. len={0} {1}", buffer.GetDataLen(), mrs.Utility.ToHex( buffer.GetData(), buffer.GetDataLen() ) );
                break;
            }
            
            ++g_ReadCount;
            if ( g_WriteCount * g_Connections <= g_ReadCount ){
                MRS_LOG_DEBUG( "Since all records have been received, it is finished." );
            }
        }
    }
    
    private static void parse_record( MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, byte[] payload, UInt32 payload_len ){
        MRS_LOG_DEBUG( "parse_record seqnum={0} options=0x{1:X2} payload=0x{2:X2}/{3}", seqnum, options, payload_type, payload_len );
        // MRS_PAYLOAD_TYPE_BEGIN - MRS_PAYLOAD_TYPE_ENDの範囲内で任意のIDを定義し、対応するアプリケーションコードを記述する
        switch ( payload_type ){
        case 0x01:{
            read_echo( connection, payload, payload_len );
        }break;
        
        default:{}break;
        }
    }
    
    private static void write_echo( MrsConnection connection, byte[] data, UInt32 data_len ){
        mrs.Time write_time = new mrs.Time();
        write_time.Set();
        mrs.Buffer buffer = new mrs.Buffer();
        buffer.WriteTime( write_time );
        buffer.Write( data, data_len );
        
        if ( g_IsValidRecord ){
            mrs_write_record( connection, g_RecordOptions, 0x01, buffer.GetData(), buffer.GetDataLen() );
        }else{
            mrs_write( connection, buffer.GetData(), buffer.GetDataLen() );
        }
    }
    
    private static void write_echo_all( MrsConnection connection ){
        g_Header = connection_type_to_string( connection ) +" ";
        g_Header += ( g_IsKeyExchange && g_IsEncryptRecords ) ? "  CRYPT" : "NOCRYPT";
        
        byte[] data = new byte[ g_WriteDataLen ];
        for ( UInt32 i = 0; i < g_WriteCount; ++i ){
            if ( 0 < g_WriteDataLen ){
                byte[] string_bytes = ToBytes( String.Format( "{0}: {1}", g_Header, i + 1 ) );
                int string_len = string_bytes.Length;
                if ( (int)g_WriteDataLen <= string_bytes.Length ){
                    string_len = (int)g_WriteDataLen;
                }
                Buffer.BlockCopy( string_bytes, 0, data, 0, string_len );
            }
            write_echo( connection, data, g_WriteDataLen );
        }
    }
    
    // 鍵交換した時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsKeyExchangeCallback))]
    private static void on_key_exchange( MrsConnection connection, IntPtr connection_data ){
        MRS_LOG_DEBUG( "on_key_exchange" );
        
        write_echo_all( connection );
    }
    
    // ソケット接続時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsConnectCallback))]
    private static void on_connect( MrsConnection connection, IntPtr connection_data ){
        MRS_LOG_DEBUG( "on_connect local_mrs_version=0x{0:X} remote_mrs_version=0x{1:X}",
            mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
        
        if ( g_IsKeyExchange ){
            mrs_set_cipher( connection, mrs_cipher_create( MrsCipherType.ECDH ) );
            mrs_key_exchange( connection, on_key_exchange );
        }else{
            write_echo_all( connection );
        }
    }
    
    // ソケット切断時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsDisconnectCallback))]
    private static void on_disconnect( MrsConnection connection, IntPtr connection_data ){
        MRS_LOG_DEBUG( "on_disconnect local_mrs_version=0x{0:X} remote_mrs_version=0x{1:X}",
            mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
    }
    
    // ソケットにエラーが発生した時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsErrorCallback))]
    private static void on_error( MrsConnection connection, IntPtr connection_data, MrsConnectionError status ){
        switch ( status ){
        case MrsConnectionError.CONNECT_ERROR:
        case MrsConnectionError.CONNECT_TIMEOUT:{
            MrsConnection client = g_Connect.FallbackConnect( connection );
            if ( MrsConnection.Zero != client ) return;
        }break;
        
        default: break;
        }
        
        MRS_LOG_ERR( "on_error local_mrs_version=0x{0:X} remote_mrs_version=0x{1:X} status={2}",
            mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ), ToString( mrs_get_connection_error_string( status ) ) );
    }
    
    // レコード受信時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsReadRecordCallback))]
    private static void on_read_record( MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len ){
        parse_record( connection, connection_data, seqnum, options, payload_type, ToBytes( _payload, payload_len ), payload_len );
    }
    
    // バイナリデータ受信時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsReadCallback))]
    private static void on_read( MrsConnection connection, IntPtr connection_data, IntPtr _data, UInt32 data_len ){
        read_echo( connection, ToBytes( _data, data_len ), data_len );
    }
    
    // フォールバック接続時に呼ばれる
    private static void on_fallback_connect( MrsConnection connection, mrs.Connect.Request request ){
        MRS_LOG_DEBUG( "on_fallback_connect connection_type="+ request.ConnectionType +" addr="+ request.Addr +" port="+ request.Port +" timeout_msec="+ request.TimeoutMsec );
        mrs_set_connect_callback( connection, on_connect );
        mrs_set_disconnect_callback( connection, on_disconnect );
        mrs_set_error_callback( connection, on_error );
        
        if ( g_IsValidRecord ){
            mrs_set_read_record_callback( connection, on_read_record );
        }else{
            mrs_set_read_callback( connection, on_read );
        }
        mrs_connection_set_path( connection, g_ArgConnectionPath );
    }
    
    void StartEchoClient(){
        MRS_LOG_DEBUG( "connection_type={0} is_key_exchange={1} is_encrypt_records={2} write_data_len={3} write_count={4} connections={5} server_addr={6} server_port={7} timeout_msec={8} is_valid_record={9} connection_path={10}",
            g_ArgConnectionType, g_ArgIsKeyExchange, g_ArgIsEncryptRecords, g_ArgWriteDataLen, g_ArgWriteCount, g_ArgConnections,
            g_ArgServerAddr, g_ArgServerPort, g_ArgTimeoutMsec, g_ArgIsValidRecord, g_ArgConnectionPath );
        
        g_IsKeyExchange = ( 0 != ToUInt32( g_ArgIsKeyExchange ) );
        g_IsEncryptRecords = ( 0 != ToUInt32( g_ArgIsEncryptRecords ) );
        g_RecordOptions = g_IsEncryptRecords ? (UInt16)MrsRecordOption.ON_CRYPT : (UInt16)MrsRecordOption.NONE;
//        g_RecordOptions |= (UInt16)MrsRecordOption.UDP_UNRELIABLE;
//        g_RecordOptions |= (UInt16)MrsRecordOption.UDP_UNSEQUENCED;
        g_WriteDataLen = ToUInt32( g_ArgWriteDataLen );
        g_WriteCount = ToUInt32( g_ArgWriteCount );
        g_Connections = ToUInt32( g_ArgConnections );
        g_IsValidRecord = ( 0 != ToUInt32( g_ArgIsValidRecord ) );
        
        mrs.Connect.Request connect_request = new mrs.Connect.Request();
        connect_request.ConnectionType = MrsConnectionType.NONE;
        connect_request.Addr           = g_ArgServerAddr;
        connect_request.Port           = ToUInt16( g_ArgServerPort );
        connect_request.TimeoutMsec    = ToUInt32( g_ArgTimeoutMsec );
        Int32 connection_type = ToInt32( g_ArgConnectionType );
        switch ( connection_type ){
        case (Int32)MrsConnectionType.TCP:
        case (Int32)MrsConnectionType.UDP:
        case (Int32)MrsConnectionType.WS:
        case (Int32)MrsConnectionType.WSS:
        case (Int32)MrsConnectionType.TCP_SSL:
        case (Int32)MrsConnectionType.MRU:{
            connect_request.ConnectionType = (MrsConnectionType)connection_type;
            g_Connect.AddRequest( connect_request );
        }break;
        
        default:{
#if UNITY_WEBGL
            connect_request.ConnectionType = MrsConnectionType.WSS;
            g_Connect.AddRequest( connect_request );
            connect_request.ConnectionType = MrsConnectionType.WS;
            connect_request.Port -= 1;
            g_Connect.AddRequest( connect_request );
#else
            connect_request.ConnectionType = MrsConnectionType.TCP;
            g_Connect.AddRequest( connect_request );
            connect_request.ConnectionType = MrsConnectionType.WSS;
            connect_request.Port += 2;
            g_Connect.AddRequest( connect_request );
            connect_request.ConnectionType = MrsConnectionType.WS;
            connect_request.Port -= 1;
            g_Connect.AddRequest( connect_request );
#endif
        }break;
        }
        g_Connect.SetFallbackConnectCallback( on_fallback_connect );
        mrs_initialize();
        do{
            for ( UInt32 i = 0; i < g_Connections; ++i ){
                MrsConnection client = g_Connect.FallbackConnect();
                if ( MrsConnection.Zero == client ){
                    MRS_LOG_ERR( "mrs_connect[{0}]: {1}", i, ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                    break;
                }
            }
        }while ( false );
    }
    
    void Update(){
        mrs_update();
    }
    
    void End(){
        mrs_finalize();
    }
    
    void OnDestroy(){
        End();
    }
    
    void OnApplicationPause( bool pause ){
        if ( pause ) mrs_update_keep_alive();
    }
}
