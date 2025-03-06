#if ! UNITY
# if UNITY_5 || UNITY_2017_1_OR_NEWER
#  define UNITY
# endif
#endif

using System;
using System.Runtime.InteropServices;

using MrsServer = System.IntPtr;
using MrsConnection = System.IntPtr;
using MrsCipher = System.IntPtr;
#if UNITY
public class Mrs : UnityEngine.MonoBehaviour {
#else
public class Mrs {
#endif
// [common.hpp]
    public static String MRS_VERSION_KEY = "mrs";
    
    public enum MrsLogLevel {
        EMERG   = 0,
        ALERT   = 1,
        CRIT    = 2,
        ERR     = 3,
        WARNING = 4,
        NOTICE  = 5,
        INFO    = 6,
        DEBUG   = 7,
    };
    
    public enum MrsConnectionType {
        NONE    = 0,
        TCP     = 1,
        UDP     = 2,
        WS      = 3,
        WSS     = 4,
        TCP_SSL = 5,
        MRU     = 6,
    };
    
    public enum MrsCipherType {
        NONE = 0,
        ECDH = 1,
    };
    
    public enum MrsError {
        NO_ERROR        = 0,
        
        ENOENT          = 2,
        ENOMEM          = 12,
        EACCES          = 13,
        EMFILE          = 24,
        EADDRINUSE      = 48,
        EADDRNOTAVAIL   = 49,
        ENETUNREACH     = 51,
        ETIMEDOUT       = 60,
        EHOSTUNREACH    = 65,
        
        ECONNECTIONTYPE = 0xF001,
        EBACKLOG        = 0xF002,
        ECONNECTIONNUM  = 0xF003,
    };
    
    public enum MrsConnectionError {
        CONNECT_ERROR                   = 1,
        CONNECT_TIMEOUT                 = 2,
        WRITE_ERROR                     = 3,
        KEY_EXCHANGE_REQUEST_ERROR      = 4,
        KEY_EXCHANGE_RESPONSE_ERROR     = 5,
        PEER_CONNECTION_HARD_LIMIT_OVER = 6,
        CONNECTION_READBUF_SIZE_OVER    = 7,
        KEEPALIVE_TIMEOUT               = 8,
        PROTOCOL_ERROR                  = 9,
        READ_INVALID_RECORD_ERROR       = 10,
        LISTEN_ERROR                    = 11,
        RESOLVE_ADDRESS_ERROR           = 12,
        RESOLVE_ADDRESS_TIMEOUT         = 13,
        WRITE_ERROR_MRU_OVER_MTU        = 14,
        WRITE_ERROR_MRU_SENDQ_FULL      = 15,
    };
    
    public enum MrsRecordOption {
        NONE            = 0x00,
        ON_CRYPT        = 0x01,
        UDP_UNRELIABLE  = 0x02,
        UDP_UNSEQUENCED = 0x04,
    };
    
    public enum MrsPayloadType {
        BEGIN = 0x00,
        END   = 0xFF,
    };
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsLogOutputCallback( MrsLogLevel level, String msg );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsNewConnectionCallback( MrsServer server, IntPtr server_data, MrsConnection client );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsConnectCallback( MrsConnection connection, IntPtr connection_data );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsDisconnectCallback( MrsConnection connection, IntPtr connection_data );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsErrorCallback( MrsConnection connection, IntPtr connection_data, MrsConnectionError status );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsReadRecordCallback( MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsReadCallback( MrsConnection connection, IntPtr connection_data, IntPtr _data, UInt32 data_len );
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MrsKeyExchangeCallback( MrsConnection connection, IntPtr connection_data );
    
// [mrs.hpp]
    public static void MRS_LOG_EMERG( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.EMERG;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_ALERT( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.ALERT;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_CRIT( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.CRIT;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_ERR( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.ERR;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_WARNING( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.WARNING;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_NOTICE( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.NOTICE;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_INFO( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.INFO;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    public static void MRS_LOG_DEBUG( String format, params object[] args ){
        MrsLogLevel level = MrsLogLevel.DEBUG;
        if ( mrs_is_output_log_level( level ) ) mrs_output_log( level, String.Format( format, args ) );
    }
    
    #if ! UNITY_EDITOR && ( UNITY_IOS || UNITY_WEBGL )
    public const string DllName = "__Internal";
    #else
    public const string DllName = "mrs";
    #endif
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_initialize();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_update();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_update_keep_alive();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_run( UInt32 sleep_msec );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_stop_running();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_finalize();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_get_connection_num_hard_limit();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_get_connection_num_soft_limit();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_set_connection_num_soft_limit( UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_get_connection_num();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_server_get_connection_num( MrsServer server );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern MrsServer mrs_server_create( MrsConnectionType type, String addr, UInt16 port, Int32 backlog );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_server_set_new_connection_callback( MrsServer server, MrsNewConnectionCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_server_set_data( MrsServer server, IntPtr server_data );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr mrs_server_get_data( MrsServer server );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern MrsConnection mrs_connect( MrsConnectionType type, String addr, UInt16 port, UInt32 timeout_msec );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_connect_callback( MrsConnection connection, MrsConnectCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_disconnect_callback( MrsConnection connection, MrsDisconnectCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_error_callback( MrsConnection connection, MrsErrorCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_read_record_callback( MrsConnection connection, MrsReadRecordCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_read_callback( MrsConnection connection, MrsReadCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_connection_set_data( MrsConnection connection, IntPtr connection_data );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr mrs_connection_get_data( MrsConnection connection );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_connection_is_connected( MrsConnection connection );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_connection_set_readbuf_max_size( MrsConnection connection, UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_connection_get_readbuf_max_size( MrsConnection connection );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern MrsConnectionType mrs_connection_get_type( MrsConnection connection );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_connection_set_path( MrsConnection connection, String value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr mrs_connection_get_path( MrsConnection connection );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_write_record( MrsConnection connection, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len );
    public static bool mrs_write_record( MrsConnection connection, UInt16 options, UInt16 payload_type, byte[] payload, UInt32 payload_len ){
        IntPtr _payload = Marshal.AllocCoTaskMem( (Int32)payload_len );
        if ( 0 < payload_len ) Marshal.Copy( payload, 0, _payload, (Int32)payload_len );
        bool result = mrs_write_record( connection, options, payload_type, _payload, payload_len );
        Marshal.FreeCoTaskMem( _payload );
        return result;
    }
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_write( MrsConnection connection, IntPtr _data, UInt32 data_len );
    public static bool mrs_write( MrsConnection connection, byte[] data, UInt32 data_len ){
        IntPtr _data = Marshal.AllocCoTaskMem( (Int32)data_len );
        if ( 0 < data_len ) Marshal.Copy( data, 0, _data, (Int32)data_len );
        bool result = mrs_write( connection, _data, data_len );
        Marshal.FreeCoTaskMem( _data );
        return result;
    }
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern MrsCipher mrs_cipher_create( MrsCipherType type );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_cipher( MrsConnection connection, MrsCipher cipher );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern bool mrs_key_exchange( MrsConnection connection, MrsKeyExchangeCallback callback );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_close( MrsConnection connection );
    
    /**
     * ログ系メソッドをマーシャリングすると、
     * 原因不明のクラッシュが発生するので、
     * DLLを使用しない
     **/
    protected static MrsLogLevel          s_LogOutputLevel;
    protected static MrsLogOutputCallback s_LogOutputCallback;
    
    public static MrsLogLevel mrs_get_output_log_level(){
        return s_LogOutputLevel;
    }
    
    public static void mrs_set_output_log_level( MrsLogLevel level ){
        s_LogOutputLevel = level;
    }
    
    public static bool mrs_is_output_log_level( MrsLogLevel level ){
        return ( level <= s_LogOutputLevel );
    }
    
    public static void mrs_output_log( MrsLogLevel level, String msg ){
        if ( mrs_is_output_log_level( level ) ) s_LogOutputCallback( level, msg );
    }
    
    public static MrsLogOutputCallback mrs_get_log_callback(){
        return s_LogOutputCallback;
    }
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_log_callback( IntPtr callback );
    public static void mrs_set_log_callback( MrsLogOutputCallback callback ){
        s_LogOutputCallback = callback;
        mrs_set_log_callback( Marshal.GetFunctionPointerForDelegate( s_LogOutputCallback ) );
    }
    
#if UNITY
    [AOT.MonoPInvokeCallback(typeof(MrsLogOutputCallback))]
#endif
    public static void mrs_console_log( MrsLogLevel level, String msg ){
        switch ( level ){
        case MrsLogLevel.DEBUG:
        case MrsLogLevel.INFO:
        case MrsLogLevel.NOTICE:{
            #if UNITY
            UnityEngine.Debug.Log( msg );
            #else
            Console.WriteLine( msg );
            #endif
        }break;
        
        case MrsLogLevel.WARNING:{
            #if UNITY
            UnityEngine.Debug.LogWarning( msg );
            #else
            Console.WriteLine( msg );
            #endif
        }break;
        
        default:{
            #if UNITY
            UnityEngine.Debug.LogError( msg );
            #else
            Console.WriteLine( msg );
            #endif
        }break;
        }
    }
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern MrsError mrs_get_last_error();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    /**
     * 戻り値の型をIntPtrではなくStringにしたかったが、
     * Unityが強制終了してしまうので、
     * 戻り値をIntPtrからStringに変換するようにした
     **/
    public static extern IntPtr mrs_get_error_string( MrsError error );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern IntPtr mrs_get_connection_error_string( MrsConnectionError error );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_sleep( UInt32 sleep_msec );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_ssl_certificate_data( String data );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_ssl_private_key_data( String data );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_keep_alive_update_msec( UInt32 update_msec );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_get_keep_alive_update_msec();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_set_version( String key, UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_get_version( String key );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_connection_get_remote_version( MrsConnection connection, String key );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_udp_set_mtu( UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_udp_get_mtu();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_mru_set_client_peer_limit( UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_mru_get_client_peer_limit();
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern void mrs_mru_set_sendq_size( UInt32 value );
    
    [DllImport(DllName, CallingConvention=CallingConvention.Cdecl)]
    public static extern UInt32 mrs_mru_get_sendq_size();
    
// [unity]
    public static String ToString( byte[] value ){
        return System.Text.Encoding.UTF8.GetString( value ).TrimEnd( '\0' );
    }
    
    public static String ToString( IntPtr value ){
        return Marshal.PtrToStringAnsi( value );
    }
    
    public static byte[] ToBytes( String value ){
        return System.Text.Encoding.UTF8.GetBytes( value );
    }
    
    public static byte[] ToBytes( IntPtr value, UInt32 value_len ){
        byte[] result = new byte[ value_len ];
        if ( 0 < value_len ) Marshal.Copy( value, result, 0, (Int32)value_len );
        return result;
    }
    
    public static byte ToUInt8( String value ){
        byte result = 0;
        byte.TryParse( value, out result );
        return result;
    }
    
    public static UInt16 ToUInt16( String value ){
        UInt16 result = 0;
        UInt16.TryParse( value, out result );
        return result;
    }
    
    public static UInt32 ToUInt32( String value ){
        UInt32 result = 0;
        UInt32.TryParse( value, out result );
        return result;
    }
    
    public static UInt64 ToUInt64( String value ){
        UInt64 result = 0;
        UInt64.TryParse( value, out result );
        return result;
    }
    
    public static sbyte ToInt8( String value ){
        sbyte result = 0;
        sbyte.TryParse( value, out result );
        return result;
    }
    
    public static Int16 ToInt16( String value ){
        Int16 result = 0;
        Int16.TryParse( value, out result );
        return result;
    }
    
    public static Int32 ToInt32( String value ){
        Int32 result = 0;
        Int32.TryParse( value, out result );
        return result;
    }
    
    public static Int64 ToInt64( String value ){
        Int64 result = 0;
        Int64.TryParse( value, out result );
        return result;
    }
    
    #if ! UNITY_EDITOR && ( UNITY_WEBGL )
    [DllImport("__Internal")]
    public static extern void __mrs_setup__();
    #else
    public static void __mrs_setup__(){}
    #endif
    
    static Mrs(){
        s_LogOutputLevel = MrsLogLevel.DEBUG;
        mrs_set_log_callback( mrs_console_log );
        
        __mrs_setup__();
    }
    
#if UNITY_ANDROID
    private void __Android__(){
        // android.permission.INTERNET
        // android.permission.ACCESS_NETWORK_STATE
        UnityEngine.Debug.Log( "Application.internetReachability="+ UnityEngine.Application.internetReachability );
    }
#endif
}
