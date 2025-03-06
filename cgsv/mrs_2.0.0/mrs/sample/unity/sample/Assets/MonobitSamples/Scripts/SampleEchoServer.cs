using UnityEngine;
using System.Collections;
using System;

using MrsServer = System.IntPtr;
using MrsConnection = System.IntPtr;
using MrsCipher = System.IntPtr;
public class SampleEchoServer : Mrs {
    protected static String g_SslCertificateData = @"Certificate:
    Data:
        Version: 3 (0x2)
        Serial Number: 1 (0x1)
    Signature Algorithm: sha256WithRSAEncryption
        Issuer: C=JP, ST=Tokyo, L=Tokyo, O=Monobit Inc, CN=root-ca.monobit.com
        Validity
            Not Before: Feb  5 09:44:33 2018 GMT
            Not After : Feb  3 09:44:33 2028 GMT
        Subject: C=JP, ST=Tokyo, O=Monobit Inc, CN=monobit.com
        Subject Public Key Info:
            Public Key Algorithm: rsaEncryption
                Public-Key: (2048 bit)
                Modulus:
                    00:be:49:4b:94:e2:46:be:85:a9:3c:1b:99:ae:1b:
                    e1:0b:17:b1:9d:b8:d9:5e:15:2d:77:05:6a:b0:a7:
                    c3:85:b6:71:c7:99:37:58:04:0e:67:81:b8:fd:20:
                    67:ca:dc:3a:0e:55:08:d2:d3:80:cd:e5:b4:e8:70:
                    8f:ab:09:ac:26:ac:0b:4e:ed:a9:78:46:5f:ae:54:
                    a8:8a:ea:1c:11:43:e6:a3:61:bf:73:dd:a3:6d:d0:
                    7b:51:09:a8:8f:b2:0b:6e:6b:2a:0e:d6:41:99:61:
                    eb:01:09:cd:f6:36:1e:b1:38:f3:25:ff:a8:6a:aa:
                    45:a8:32:79:28:46:28:4e:62:34:57:6d:99:77:7a:
                    cb:9d:ef:ee:d6:c4:b5:15:a5:3e:cb:24:68:8e:37:
                    31:a6:ea:b9:dd:49:0b:fe:3f:a7:f0:c9:13:bb:bf:
                    26:91:fe:68:dc:79:5b:9c:ff:e0:7d:83:14:83:5e:
                    8d:c9:0a:b9:0d:2d:9a:45:23:a3:0d:76:5e:70:57:
                    b6:9e:82:bb:91:47:74:9a:e5:41:32:8a:e2:c8:04:
                    be:08:4c:90:41:66:4f:8d:a2:c7:ac:7d:87:0d:f6:
                    a7:55:37:06:33:16:89:56:85:c8:76:ba:bd:c7:07:
                    fb:cc:66:e5:3d:21:26:4f:9e:1e:58:39:60:2e:5e:
                    c1:e9
                Exponent: 65537 (0x10001)
        X509v3 extensions:
            X509v3 Subject Alternative Name: 
                DNS:monobit.com
    Signature Algorithm: sha256WithRSAEncryption
         3b:7e:e0:02:08:ee:9f:aa:df:ea:de:03:11:08:6f:2e:0f:35:
         28:59:93:ad:14:5e:36:cc:61:ba:f1:37:d9:bf:20:46:a7:c8:
         cc:f1:41:70:23:90:f2:26:2a:2d:a6:5e:2f:d2:3a:a8:5e:bd:
         aa:97:f1:ad:17:41:9a:f1:85:63:6b:52:89:b8:b6:56:c3:96:
         54:ae:26:f9:4c:ff:4c:22:c6:70:7d:40:97:c0:e2:4f:a9:0c:
         13:c7:eb:b9:d5:ea:3d:5e:89:16:eb:91:b6:4f:a1:db:82:69:
         4a:9f:91:08:f6:6c:0d:b4:28:99:8e:38:57:8b:b6:1b:21:b1:
         c1:74:44:68:6d:a8:f5:29:dc:10:dc:2c:08:ec:dc:91:51:02:
         ba:d9:7a:b9:74:c4:59:f5:78:23:2e:60:81:8e:d2:53:07:8e:
         79:16:21:f9:75:5f:70:0f:46:be:83:8c:07:84:7b:87:16:46:
         04:df:31:c7:e9:7b:69:dc:56:fc:bc:a2:d5:6d:c1:94:c3:fa:
         01:1f:71:30:4c:c0:36:47:68:17:3d:6d:fc:81:1f:be:a5:9b:
         b6:5d:d3:10:56:57:1a:09:9f:5b:68:a3:73:17:d6:f6:ba:57:
         31:f3:01:d5:5d:1a:e7:b6:e3:36:68:2e:8c:38:c4:8b:12:07:
         8e:76:5c:cc
-----BEGIN CERTIFICATE-----
MIIDPzCCAiegAwIBAgIBATANBgkqhkiG9w0BAQsFADBhMQswCQYDVQQGEwJKUDEO
MAwGA1UECAwFVG9reW8xDjAMBgNVBAcMBVRva3lvMRQwEgYDVQQKDAtNb25vYml0
IEluYzEcMBoGA1UEAwwTcm9vdC1jYS5tb25vYml0LmNvbTAeFw0xODAyMDUwOTQ0
MzNaFw0yODAyMDMwOTQ0MzNaMEkxCzAJBgNVBAYTAkpQMQ4wDAYDVQQIDAVUb2t5
bzEUMBIGA1UECgwLTW9ub2JpdCBJbmMxFDASBgNVBAMMC21vbm9iaXQuY29tMIIB
IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvklLlOJGvoWpPBuZrhvhCxex
nbjZXhUtdwVqsKfDhbZxx5k3WAQOZ4G4/SBnytw6DlUI0tOAzeW06HCPqwmsJqwL
Tu2peEZfrlSoiuocEUPmo2G/c92jbdB7UQmoj7ILbmsqDtZBmWHrAQnN9jYesTjz
Jf+oaqpFqDJ5KEYoTmI0V22Zd3rLne/u1sS1FaU+yyRojjcxpuq53UkL/j+n8MkT
u78mkf5o3HlbnP/gfYMUg16NyQq5DS2aRSOjDXZecFe2noK7kUd0muVBMoriyAS+
CEyQQWZPjaLHrH2HDfanVTcGMxaJVoXIdrq9xwf7zGblPSEmT54eWDlgLl7B6QID
AQABoxowGDAWBgNVHREEDzANggttb25vYml0LmNvbTANBgkqhkiG9w0BAQsFAAOC
AQEAO37gAgjun6rf6t4DEQhvLg81KFmTrRReNsxhuvE32b8gRqfIzPFBcCOQ8iYq
LaZeL9I6qF69qpfxrRdBmvGFY2tSibi2VsOWVK4m+Uz/TCLGcH1Al8DiT6kME8fr
udXqPV6JFuuRtk+h24JpSp+RCPZsDbQomY44V4u2GyGxwXREaG2o9SncENwsCOzc
kVECutl6uXTEWfV4Iy5ggY7SUweOeRYh+XVfcA9GvoOMB4R7hxZGBN8xx+l7adxW
/Lyi1W3BlMP6AR9xMEzANkdoFz1t/IEfvqWbtl3TEFZXGgmfW2ijcxfW9rpXMfMB
1V0a57bjNmgujDjEixIHjnZczA==
-----END CERTIFICATE-----";
    
    protected static String g_SslPrivateKeyData = @"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC+SUuU4ka+hak8
G5muG+ELF7GduNleFS13BWqwp8OFtnHHmTdYBA5ngbj9IGfK3DoOVQjS04DN5bTo
cI+rCawmrAtO7al4Rl+uVKiK6hwRQ+ajYb9z3aNt0HtRCaiPsgtuayoO1kGZYesB
Cc32Nh6xOPMl/6hqqkWoMnkoRihOYjRXbZl3esud7+7WxLUVpT7LJGiONzGm6rnd
SQv+P6fwyRO7vyaR/mjceVuc/+B9gxSDXo3JCrkNLZpFI6MNdl5wV7aegruRR3Sa
5UEyiuLIBL4ITJBBZk+NosesfYcN9qdVNwYzFolWhch2ur3HB/vMZuU9ISZPnh5Y
OWAuXsHpAgMBAAECggEBAKvCVSrqpJjM5VmQQEPcfmIY3QZVGD/INeW9SfRzOhWz
/TgBoOcdojLj8Srq2UVPTEgrkW9b4mP3+DfngocMkAvIN9ziwZoDS9J8MDZT40ni
VIkdbkcTxLUW/njDkxdByve8W5ZQ552fuRSS2QppB4NIuJGQF2FQmNed16b2zOMS
R8UMBwpax2KSM1FEi2/wm7D/dTM9KrKF+mL0m9qLpXJBLkb3Ysz1jYVFLzKb6led
C8rhTGdjGJzBkvUqEpntTaxiPcMPJrwXnFZLCsnRY59aQ0HAeaIHofz1F+PrPnlG
LhldWoBJ9DHSvlxptl1H32fWaOiUOHx/4HTWmYrVg0ECgYEA/H+jO8Os+e9a/586
vvFZohSDd/kX+LgekYnGX+d99615/ef6QcVpATjGy4amAz84U1o9vc108HpkL8l2
n5Sz0o8DWlCzq0umLfDcVolw4CUEHSMIlnDiOwuvNMvVlj9mMPD29u2tMZkYhtl7
hBnqRiGEST14NDzwVaVUO5HfKb0CgYEAwOzOT/cQOwsHS2VXmmnCxuQs5nYyHeDc
IYCA/4/pn4QJngXf9Y3MUbrYyBqYikuJdFsUoRG6Gaei1TSqQyN0tE82/kT+d+TW
tez51bYnFy5BKrOjca0ynk+vzQAue3EWKzubvB+Ry2PeXDBiDwLTiFnL6ZrBDdXZ
b6KKg5YC3Z0CgYBMTNWqzaqLrES3UgqSgKQxUjmYG3Ge+yRhnlyPxohOO+HNVDhP
f7QHZnzMK9gmywfeSDq4DEn2EUYNGrf56Rmd2xGMTS696JJC97HdhJLTaNwhYeDK
dTon1ZQQRDg6utXKnEZEv/XCMx0yQq4McThWEPLEnwqf3npRpzAZAC+LkQKBgENd
bNz3RC+Ztj5ZcLF2ZJDWc+c1NmLAdZ0tJd4W/li43jLTklRH4yRWvgOBZepEXgbH
Fvj3G6iBpJYWAa3X35RmZKl8pe5vdZmo2cQKCrRJbm/esh+rfpVQ9e37Nj/cSQVn
lwWlcF84zBgcvODI99wQnuc/JlISbg5RD1TLOMzxAoGBAKAaxCAQSdArvOSf6Q1P
AlCeMGGxqjL9phyh37VDuvZspC9dFZuhgG//EVcf/1q9dZd4Nev4SRl9TjMu5MPY
thPRFkZrj/0ArbT6PBreFUodUv5kp48Qnitk3KMn1QfcR3pO3A5BPaRjyc/7d81F
Md2qrDIp8KRceWB4YV9pjPOX
-----END PRIVATE KEY-----";
    
    protected static String g_ArgServerAddr;
    protected static String g_ArgServerPort;
    protected static String g_ArgBacklog;
    protected static String g_ArgIsValidRecord;
    
    protected static bool g_IsValidRecord;
    
    static SampleEchoServer(){
        g_ArgServerAddr    = "0.0.0.0";
        g_ArgServerPort    = "22222";
        g_ArgBacklog       = "10";
        g_ArgIsValidRecord = "1";
    }
    
    protected bool m_IsRunning;
    
    void Awake(){
        gameObject.AddComponent< mrs.ScreenLogger >();
        
        m_IsRunning = false;
    }
    
    void OnGUI(){
        if ( ! m_IsRunning ){
            GUILayout.BeginHorizontal();
            GUILayout.Label( "ServerAddr:" );
            g_ArgServerAddr = GUILayout.TextField( g_ArgServerAddr );
            GUILayout.Space( 30 );
            GUILayout.Label( "ServerPort:" );
            g_ArgServerPort = GUILayout.TextField( g_ArgServerPort );
            GUILayout.Space( 30 );
            GUILayout.Label( "Backlog:" );
            g_ArgBacklog = GUILayout.TextField( g_ArgBacklog );
            GUILayout.Space( 30 );
            GUILayout.Label( "IsValidRecord:" );
            g_ArgIsValidRecord = GUILayout.TextField( g_ArgIsValidRecord );
            GUILayout.EndHorizontal();
            
            GUILayout.Space( 30 );
            
            if ( GUILayout.Button( "Start Echo Server", GUILayout.Width( 300 ), GUILayout.Height( 50 ) ) ){
                m_IsRunning = true;
                StartEchoServer();
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
    
    private static void parse_record( MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, byte[] payload, UInt32 payload_len ){
        MRS_LOG_DEBUG( "parse_record seqnum={0} options=0x{1:X2} payload=0x{2:X2}/{3}", seqnum, options, payload_type, payload_len );
        switch ( payload_type ){
        // MrsPayloadType.BEGIN - MrsPayloadType.ENDの範囲内で任意のIDを定義し、対応するアプリケーションコードを記述する
        case 0x01:{
            mrs_write_record( connection, options, payload_type, payload, payload_len );
        }break;
        
        default:{}break;
        }
    }
    
    // ソケット接続時に呼ばれる
    private static void on_connect( MrsConnection connection ){
        MRS_LOG_DEBUG( "on_connect {0} local_mrs_version=0x{1:X} remote_mrs_version=0x{2:X}",
            connection_type_to_string( connection ), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
        
        mrs_set_cipher( connection, mrs_cipher_create( MrsCipherType.ECDH ) );
    }
    
    // ソケット切断時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsDisconnectCallback))]
    private static void on_disconnect( MrsConnection connection, IntPtr connection_data ){
        MRS_LOG_DEBUG( "on_disconnect {0} local_mrs_version=0x{1:X} remote_mrs_version=0x{2:X}",
            connection_type_to_string( connection ), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
    }
    
    // ソケットにエラーが発生した時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsErrorCallback))]
    private static void on_error( MrsConnection connection, IntPtr connection_data, MrsConnectionError status ){
        MRS_LOG_ERR( "on_error {0} local_mrs_version=0x{1:X} remote_mrs_version=0x{2:X} status={3}",
            connection_type_to_string( connection ), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ), ToString( mrs_get_connection_error_string( status ) ) );
    }
    
    // レコード受信時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsReadRecordCallback))]
    private static void on_read_record( MrsConnection connection, IntPtr connection_data, UInt32 seqnum, UInt16 options, UInt16 payload_type, IntPtr _payload, UInt32 payload_len ){
        parse_record( connection, connection_data, seqnum, options, payload_type, ToBytes( _payload, payload_len ), payload_len );
    }
    
    // バイナリデータ受信時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsReadCallback))]
    private static void on_read( MrsConnection connection, IntPtr connection_data, IntPtr _data, UInt32 data_len ){
        mrs_write( connection, ToBytes( _data, data_len ), data_len );
    }
    
    // ソケットが新しい接続を生成した時に呼ばれる
    [AOT.MonoPInvokeCallback(typeof(MrsNewConnectionCallback))]
    private static void on_new_connection( MrsServer server, IntPtr server_data, MrsConnection client ){
        MRS_LOG_DEBUG( "on_new_connection {0}", connection_type_to_string( client ) );
        
        mrs_set_disconnect_callback( client, on_disconnect );
        mrs_set_error_callback( client, on_error );
        mrs_set_read_record_callback( client, on_read_record );
        on_connect( client );
    }
    
    protected MrsServer m_TcpServer;
    protected MrsServer m_UdpServer;
    protected MrsServer m_WsServer;
    protected MrsServer m_WssServer;
    protected MrsServer m_TcpSslServer;
    protected MrsServer m_MruServer;
    
    void StartEchoServer(){
        MRS_LOG_DEBUG( "server_addr={0} server_port={1} backlog={2} is_valid_record={3}",
            g_ArgServerAddr, g_ArgServerPort, g_ArgBacklog, g_ArgIsValidRecord );
        
        UInt16 server_port = ToUInt16( g_ArgServerPort );
        Int32 backlog = ToInt32( g_ArgBacklog );
        g_IsValidRecord = ( 0 != ToUInt32( g_ArgIsValidRecord ) );
        mrs_initialize();
        mrs_set_ssl_certificate_data( g_SslCertificateData );
        mrs_set_ssl_private_key_data( g_SslPrivateKeyData );
        m_TcpServer    = MrsServer.Zero;
        m_UdpServer    = MrsServer.Zero;
        m_WsServer     = MrsServer.Zero;
        m_WssServer    = MrsServer.Zero;
        m_TcpSslServer = MrsServer.Zero;
        m_MruServer    = MrsServer.Zero;
        do{
            m_TcpServer = mrs_server_create( MrsConnectionType.TCP, g_ArgServerAddr, server_port, backlog );
            if ( MrsServer.Zero == m_TcpServer ){
                MRS_LOG_ERR( "TCP mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            m_UdpServer = mrs_server_create( MrsConnectionType.UDP, g_ArgServerAddr, server_port, backlog );
            if ( MrsServer.Zero == m_UdpServer ){
                MRS_LOG_ERR( "UDP mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            m_WsServer = mrs_server_create( MrsConnectionType.WS, g_ArgServerAddr, (UInt16)(server_port + 1), backlog );
            if ( MrsServer.Zero == m_WsServer ){
                MRS_LOG_ERR( "WS mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            m_WssServer = mrs_server_create( MrsConnectionType.WSS, g_ArgServerAddr, (UInt16)(server_port + 2), backlog );
            if ( MrsServer.Zero == m_WssServer ){
                MRS_LOG_ERR( "WSS mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            m_TcpSslServer = mrs_server_create( MrsConnectionType.TCP_SSL, g_ArgServerAddr, (UInt16)(server_port + 3), backlog );
            if ( MrsServer.Zero == m_TcpSslServer ){
                MRS_LOG_ERR( "TCP_SSL mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            m_MruServer = mrs_server_create( MrsConnectionType.MRU, g_ArgServerAddr, (UInt16)(server_port + 4), backlog );
            if ( MrsServer.Zero == m_MruServer ){
                MRS_LOG_ERR( "MRU mrs_server_create: {0}", ToString( mrs_get_error_string( mrs_get_last_error() ) ) );
                break;
            }
            
            mrs_server_set_new_connection_callback( m_TcpServer, on_new_connection );
            mrs_server_set_new_connection_callback( m_UdpServer, on_new_connection );
            mrs_server_set_new_connection_callback( m_WsServer, on_new_connection );
            mrs_server_set_new_connection_callback( m_WssServer, on_new_connection );
            mrs_server_set_new_connection_callback( m_TcpSslServer, on_new_connection );
            mrs_server_set_new_connection_callback( m_MruServer, on_new_connection );
            
            mrs_set_error_callback( m_TcpServer, on_error );
            mrs_set_error_callback( m_UdpServer, on_error );
            mrs_set_error_callback( m_WsServer, on_error );
            mrs_set_error_callback( m_WssServer, on_error );
            mrs_set_error_callback( m_TcpSslServer, on_error );
            mrs_set_error_callback( m_MruServer, on_error );
            
            if ( ! g_IsValidRecord ){
                mrs_set_read_callback( m_TcpServer, on_read );
                mrs_set_read_callback( m_UdpServer, on_read );
                mrs_set_read_callback( m_WsServer, on_read );
                mrs_set_read_callback( m_WssServer, on_read );
                mrs_set_read_callback( m_TcpSslServer, on_read );
                mrs_set_read_callback( m_MruServer, on_read );
            }
            
            MRS_LOG_DEBUG( "{0} listening on {1} {2}", connection_type_to_string( m_TcpServer ), g_ArgServerAddr, server_port );
            MRS_LOG_DEBUG( "{0} waiting on {1} {2}", connection_type_to_string( m_UdpServer ), g_ArgServerAddr, server_port );
            MRS_LOG_DEBUG( "{0} listening on {1} {2}", connection_type_to_string( m_WsServer ), g_ArgServerAddr, server_port + 1 );
            MRS_LOG_DEBUG( "{0} listening on {1} {2}", connection_type_to_string( m_WssServer ), g_ArgServerAddr, server_port + 2 );
            MRS_LOG_DEBUG( "{0} listening on {1} {2}", connection_type_to_string( m_TcpSslServer ), g_ArgServerAddr, server_port + 3 );
            MRS_LOG_DEBUG( "{0} waiting on {1} {2}", connection_type_to_string( m_MruServer ), g_ArgServerAddr, server_port + 4 );
        }while ( false );
    }
    
    void Update(){
        mrs_update();
    }
    
    void End(){
        if ( MrsServer.Zero != m_TcpServer ){
            mrs_close( m_TcpServer );
            m_TcpServer = MrsServer.Zero;
        }
        
        if ( MrsServer.Zero != m_UdpServer ){
            mrs_close( m_UdpServer );
            m_UdpServer = MrsServer.Zero;
        }
        
        if ( MrsServer.Zero != m_WsServer ){
            mrs_close( m_WsServer );
            m_WsServer = MrsServer.Zero;
        }
        
        if ( MrsServer.Zero != m_WssServer ){
            mrs_close( m_WssServer );
            m_WssServer = MrsServer.Zero;
        }
        
        if ( MrsServer.Zero != m_TcpSslServer ){
            mrs_close( m_TcpSslServer );
            m_TcpSslServer = MrsServer.Zero;
        }
        
        if ( MrsServer.Zero != m_MruServer ){
            mrs_close( m_MruServer );
            m_MruServer = MrsServer.Zero;
        }
        
        mrs_finalize();
    }
    
    void OnDestroy(){
        End();
    }
    
    void OnApplicationPause( bool pause ){
        if ( pause ) mrs_update_keep_alive();
    }
}
