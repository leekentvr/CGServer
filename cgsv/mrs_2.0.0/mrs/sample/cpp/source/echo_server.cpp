#include <mrs.hpp>

static const char* g_SslCertificateData = "Certificate:\n"
"    Data:\n"
"        Version: 3 (0x2)\n"
"        Serial Number: 1 (0x1)\n"
"    Signature Algorithm: sha256WithRSAEncryption\n"
"        Issuer: C=JP, ST=Tokyo, L=Tokyo, O=Monobit Inc, CN=root-ca.monobit.com\n"
"        Validity\n"
"            Not Before: Feb  5 09:44:33 2018 GMT\n"
"            Not After : Feb  3 09:44:33 2028 GMT\n"
"        Subject: C=JP, ST=Tokyo, O=Monobit Inc, CN=monobit.com\n"
"        Subject Public Key Info:\n"
"            Public Key Algorithm: rsaEncryption\n"
"                Public-Key: (2048 bit)\n"
"                Modulus:\n"
"                    00:be:49:4b:94:e2:46:be:85:a9:3c:1b:99:ae:1b:\n"
"                    e1:0b:17:b1:9d:b8:d9:5e:15:2d:77:05:6a:b0:a7:\n"
"                    c3:85:b6:71:c7:99:37:58:04:0e:67:81:b8:fd:20:\n"
"                    67:ca:dc:3a:0e:55:08:d2:d3:80:cd:e5:b4:e8:70:\n"
"                    8f:ab:09:ac:26:ac:0b:4e:ed:a9:78:46:5f:ae:54:\n"
"                    a8:8a:ea:1c:11:43:e6:a3:61:bf:73:dd:a3:6d:d0:\n"
"                    7b:51:09:a8:8f:b2:0b:6e:6b:2a:0e:d6:41:99:61:\n"
"                    eb:01:09:cd:f6:36:1e:b1:38:f3:25:ff:a8:6a:aa:\n"
"                    45:a8:32:79:28:46:28:4e:62:34:57:6d:99:77:7a:\n"
"                    cb:9d:ef:ee:d6:c4:b5:15:a5:3e:cb:24:68:8e:37:\n"
"                    31:a6:ea:b9:dd:49:0b:fe:3f:a7:f0:c9:13:bb:bf:\n"
"                    26:91:fe:68:dc:79:5b:9c:ff:e0:7d:83:14:83:5e:\n"
"                    8d:c9:0a:b9:0d:2d:9a:45:23:a3:0d:76:5e:70:57:\n"
"                    b6:9e:82:bb:91:47:74:9a:e5:41:32:8a:e2:c8:04:\n"
"                    be:08:4c:90:41:66:4f:8d:a2:c7:ac:7d:87:0d:f6:\n"
"                    a7:55:37:06:33:16:89:56:85:c8:76:ba:bd:c7:07:\n"
"                    fb:cc:66:e5:3d:21:26:4f:9e:1e:58:39:60:2e:5e:\n"
"                    c1:e9\n"
"                Exponent: 65537 (0x10001)\n"
"        X509v3 extensions:\n"
"            X509v3 Subject Alternative Name: \n"
"                DNS:monobit.com\n"
"    Signature Algorithm: sha256WithRSAEncryption\n"
"         3b:7e:e0:02:08:ee:9f:aa:df:ea:de:03:11:08:6f:2e:0f:35:\n"
"         28:59:93:ad:14:5e:36:cc:61:ba:f1:37:d9:bf:20:46:a7:c8:\n"
"         cc:f1:41:70:23:90:f2:26:2a:2d:a6:5e:2f:d2:3a:a8:5e:bd:\n"
"         aa:97:f1:ad:17:41:9a:f1:85:63:6b:52:89:b8:b6:56:c3:96:\n"
"         54:ae:26:f9:4c:ff:4c:22:c6:70:7d:40:97:c0:e2:4f:a9:0c:\n"
"         13:c7:eb:b9:d5:ea:3d:5e:89:16:eb:91:b6:4f:a1:db:82:69:\n"
"         4a:9f:91:08:f6:6c:0d:b4:28:99:8e:38:57:8b:b6:1b:21:b1:\n"
"         c1:74:44:68:6d:a8:f5:29:dc:10:dc:2c:08:ec:dc:91:51:02:\n"
"         ba:d9:7a:b9:74:c4:59:f5:78:23:2e:60:81:8e:d2:53:07:8e:\n"
"         79:16:21:f9:75:5f:70:0f:46:be:83:8c:07:84:7b:87:16:46:\n"
"         04:df:31:c7:e9:7b:69:dc:56:fc:bc:a2:d5:6d:c1:94:c3:fa:\n"
"         01:1f:71:30:4c:c0:36:47:68:17:3d:6d:fc:81:1f:be:a5:9b:\n"
"         b6:5d:d3:10:56:57:1a:09:9f:5b:68:a3:73:17:d6:f6:ba:57:\n"
"         31:f3:01:d5:5d:1a:e7:b6:e3:36:68:2e:8c:38:c4:8b:12:07:\n"
"         8e:76:5c:cc\n"
"-----BEGIN CERTIFICATE-----\n"
"MIIDPzCCAiegAwIBAgIBATANBgkqhkiG9w0BAQsFADBhMQswCQYDVQQGEwJKUDEO\n"
"MAwGA1UECAwFVG9reW8xDjAMBgNVBAcMBVRva3lvMRQwEgYDVQQKDAtNb25vYml0\n"
"IEluYzEcMBoGA1UEAwwTcm9vdC1jYS5tb25vYml0LmNvbTAeFw0xODAyMDUwOTQ0\n"
"MzNaFw0yODAyMDMwOTQ0MzNaMEkxCzAJBgNVBAYTAkpQMQ4wDAYDVQQIDAVUb2t5\n"
"bzEUMBIGA1UECgwLTW9ub2JpdCBJbmMxFDASBgNVBAMMC21vbm9iaXQuY29tMIIB\n"
"IjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvklLlOJGvoWpPBuZrhvhCxex\n"
"nbjZXhUtdwVqsKfDhbZxx5k3WAQOZ4G4/SBnytw6DlUI0tOAzeW06HCPqwmsJqwL\n"
"Tu2peEZfrlSoiuocEUPmo2G/c92jbdB7UQmoj7ILbmsqDtZBmWHrAQnN9jYesTjz\n"
"Jf+oaqpFqDJ5KEYoTmI0V22Zd3rLne/u1sS1FaU+yyRojjcxpuq53UkL/j+n8MkT\n"
"u78mkf5o3HlbnP/gfYMUg16NyQq5DS2aRSOjDXZecFe2noK7kUd0muVBMoriyAS+\n"
"CEyQQWZPjaLHrH2HDfanVTcGMxaJVoXIdrq9xwf7zGblPSEmT54eWDlgLl7B6QID\n"
"AQABoxowGDAWBgNVHREEDzANggttb25vYml0LmNvbTANBgkqhkiG9w0BAQsFAAOC\n"
"AQEAO37gAgjun6rf6t4DEQhvLg81KFmTrRReNsxhuvE32b8gRqfIzPFBcCOQ8iYq\n"
"LaZeL9I6qF69qpfxrRdBmvGFY2tSibi2VsOWVK4m+Uz/TCLGcH1Al8DiT6kME8fr\n"
"udXqPV6JFuuRtk+h24JpSp+RCPZsDbQomY44V4u2GyGxwXREaG2o9SncENwsCOzc\n"
"kVECutl6uXTEWfV4Iy5ggY7SUweOeRYh+XVfcA9GvoOMB4R7hxZGBN8xx+l7adxW\n"
"/Lyi1W3BlMP6AR9xMEzANkdoFz1t/IEfvqWbtl3TEFZXGgmfW2ijcxfW9rpXMfMB\n"
"1V0a57bjNmgujDjEixIHjnZczA==\n"
"-----END CERTIFICATE-----\n";
static const char* g_SslPrivateKeyData = "-----BEGIN PRIVATE KEY-----\n"
"MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC+SUuU4ka+hak8\n"
"G5muG+ELF7GduNleFS13BWqwp8OFtnHHmTdYBA5ngbj9IGfK3DoOVQjS04DN5bTo\n"
"cI+rCawmrAtO7al4Rl+uVKiK6hwRQ+ajYb9z3aNt0HtRCaiPsgtuayoO1kGZYesB\n"
"Cc32Nh6xOPMl/6hqqkWoMnkoRihOYjRXbZl3esud7+7WxLUVpT7LJGiONzGm6rnd\n"
"SQv+P6fwyRO7vyaR/mjceVuc/+B9gxSDXo3JCrkNLZpFI6MNdl5wV7aegruRR3Sa\n"
"5UEyiuLIBL4ITJBBZk+NosesfYcN9qdVNwYzFolWhch2ur3HB/vMZuU9ISZPnh5Y\n"
"OWAuXsHpAgMBAAECggEBAKvCVSrqpJjM5VmQQEPcfmIY3QZVGD/INeW9SfRzOhWz\n"
"/TgBoOcdojLj8Srq2UVPTEgrkW9b4mP3+DfngocMkAvIN9ziwZoDS9J8MDZT40ni\n"
"VIkdbkcTxLUW/njDkxdByve8W5ZQ552fuRSS2QppB4NIuJGQF2FQmNed16b2zOMS\n"
"R8UMBwpax2KSM1FEi2/wm7D/dTM9KrKF+mL0m9qLpXJBLkb3Ysz1jYVFLzKb6led\n"
"C8rhTGdjGJzBkvUqEpntTaxiPcMPJrwXnFZLCsnRY59aQ0HAeaIHofz1F+PrPnlG\n"
"LhldWoBJ9DHSvlxptl1H32fWaOiUOHx/4HTWmYrVg0ECgYEA/H+jO8Os+e9a/586\n"
"vvFZohSDd/kX+LgekYnGX+d99615/ef6QcVpATjGy4amAz84U1o9vc108HpkL8l2\n"
"n5Sz0o8DWlCzq0umLfDcVolw4CUEHSMIlnDiOwuvNMvVlj9mMPD29u2tMZkYhtl7\n"
"hBnqRiGEST14NDzwVaVUO5HfKb0CgYEAwOzOT/cQOwsHS2VXmmnCxuQs5nYyHeDc\n"
"IYCA/4/pn4QJngXf9Y3MUbrYyBqYikuJdFsUoRG6Gaei1TSqQyN0tE82/kT+d+TW\n"
"tez51bYnFy5BKrOjca0ynk+vzQAue3EWKzubvB+Ry2PeXDBiDwLTiFnL6ZrBDdXZ\n"
"b6KKg5YC3Z0CgYBMTNWqzaqLrES3UgqSgKQxUjmYG3Ge+yRhnlyPxohOO+HNVDhP\n"
"f7QHZnzMK9gmywfeSDq4DEn2EUYNGrf56Rmd2xGMTS696JJC97HdhJLTaNwhYeDK\n"
"dTon1ZQQRDg6utXKnEZEv/XCMx0yQq4McThWEPLEnwqf3npRpzAZAC+LkQKBgENd\n"
"bNz3RC+Ztj5ZcLF2ZJDWc+c1NmLAdZ0tJd4W/li43jLTklRH4yRWvgOBZepEXgbH\n"
"Fvj3G6iBpJYWAa3X35RmZKl8pe5vdZmo2cQKCrRJbm/esh+rfpVQ9e37Nj/cSQVn\n"
"lwWlcF84zBgcvODI99wQnuc/JlISbg5RD1TLOMzxAoGBAKAaxCAQSdArvOSf6Q1P\n"
"AlCeMGGxqjL9phyh37VDuvZspC9dFZuhgG//EVcf/1q9dZd4Nev4SRl9TjMu5MPY\n"
"thPRFkZrj/0ArbT6PBreFUodUv5kp48Qnitk3KMn1QfcR3pO3A5BPaRjyc/7d81F\n"
"Md2qrDIp8KRceWB4YV9pjPOX\n"
"-----END PRIVATE KEY-----\n";

static bool g_IsValidRecord = true;

std::string connection_type_to_string( MrsConnection connection ){
    MrsConnectionType type = mrs_connection_get_type( connection );
    switch ( type ){
    case MRS_CONNECTION_TYPE_NONE:{ return "NONE"; }
    case MRS_CONNECTION_TYPE_TCP:{ return "TCP"; }
    case MRS_CONNECTION_TYPE_UDP:{ return "UDP"; }
    case MRS_CONNECTION_TYPE_WS:{ return "WS"; }
    case MRS_CONNECTION_TYPE_WSS:{ return "WSS"; }
    case MRS_CONNECTION_TYPE_TCP_SSL:{ return "TCP_SSL"; }
    case MRS_CONNECTION_TYPE_MRU:{ return "MRU"; }
    }
    return "INVALID";
}

void parse_record( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len ){
    MRS_LOG_DEBUG( "parse_record seqnum=%u options=0x%02X payload=0x%02X/%u", seqnum, options, payload_type, payload_len );
    switch ( payload_type ){
    // MRS_PAYLOAD_TYPE_BEGIN - MRS_PAYLOAD_TYPE_ENDの範囲内で任意のIDを定義し、対応するアプリケーションコードを記述する
    case 0x01:{
        mrs_write_record( connection, options, payload_type, payload, payload_len );
    }break;
    
    default:{}break;
    }
}

// ソケット接続時に呼ばれる
void on_connect( MrsConnection connection ){
    MRS_LOG_DEBUG( "on_connect %s local_mrs_version=0x%x remote_mrs_version=0x%x",
        connection_type_to_string( connection ).c_str(), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
    
    mrs_set_cipher( connection, mrs_cipher_create( MRS_CIPHER_TYPE_ECDH ) );
}

// ソケット切断時に呼ばれる
void on_disconnect( MrsConnection connection, void* connection_data ){
    MRS_LOG_DEBUG( "on_disconnect %s local_mrs_version=0x%x remote_mrs_version=0x%x",
        connection_type_to_string( connection ).c_str(), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
}

// ソケットにエラーが発生した時に呼ばれる
void on_error( MrsConnection connection, void* connection_data, MrsConnectionError status ){
    MRS_LOG_DEBUG( "on_error %s local_mrs_version=0x%x remote_mrs_version=0x%x status=%s",
        connection_type_to_string( connection ).c_str(), mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ), mrs_get_connection_error_string( status ) );
}

// レコード受信時に呼ばれる
void on_read_record( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len ){
    parse_record( connection, connection_data, seqnum, options, payload_type, payload, payload_len );
}

// バイナリデータ受信時に呼ばれる
void on_read( MrsConnection connection, void* connection_data, const void* data, uint32 data_len ){
    mrs_write( connection, data, data_len );
}

// ソケットが新しい接続を生成した時に呼ばれる
void on_new_connection( MrsServer server, void* server_data, MrsConnection client ){
    MRS_LOG_DEBUG( "on_new_connection %s", connection_type_to_string( client ).c_str() );
    
    mrs_set_disconnect_callback( client, on_disconnect );
    mrs_set_error_callback( client, on_error );
    mrs_set_read_record_callback( client, on_read_record );
    on_connect( client );
}

int main( int argc, char** argv ){
    int argi = 1;
    MRS_LOG_DEBUG( "arg %02d: sleep_msec [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: server_addr [0.0.0.0]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: server_port [22222]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: backlog [10]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: is_valid_record(OFF:0 ON:1) [1]", argi++ );
    
    argi = 1;
    const char* arg_sleep_msec      = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_server_addr     = ( ++argi <= argc ) ? argv[ argi - 1 ] : "0.0.0.0";
    const char* arg_server_port     = ( ++argi <= argc ) ? argv[ argi - 1 ] : "22222";
    const char* arg_backlog         = ( ++argi <= argc ) ? argv[ argi - 1 ] : "10";
    const char* arg_is_valid_record = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    MRS_LOG_DEBUG( "sleep_msec=%s server_addr=%s server_port=%s backlog=%s arg_is_valid_record=%s",
        arg_sleep_msec, arg_server_addr, arg_server_port, arg_backlog,
        arg_is_valid_record );
    
    uint32 sleep_msec = atoi( arg_sleep_msec );
    uint16 server_port = atoi( arg_server_port );
    int32 backlog = atoi( arg_backlog );
    g_IsValidRecord = ( 0 != atoi( arg_is_valid_record ) );
    mrs_initialize();
    mrs_set_ssl_certificate_data( g_SslCertificateData );
    mrs_set_ssl_private_key_data( g_SslPrivateKeyData );
    MrsServer tcp_server = NULL;
    MrsServer udp_server = NULL;
    MrsServer ws_server = NULL;
    MrsServer wss_server = NULL;
    MrsServer tcp_ssl_server = NULL;
    MrsServer mru_server = NULL;
    do{
        tcp_server = mrs_server_create( MRS_CONNECTION_TYPE_TCP, arg_server_addr, server_port, backlog );
        if ( NULL == tcp_server ){
            MRS_LOG_ERR( "TCP mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        udp_server = mrs_server_create( MRS_CONNECTION_TYPE_UDP, arg_server_addr, server_port, backlog );
        if ( NULL == udp_server ){
            MRS_LOG_ERR( "UDP mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        ws_server = mrs_server_create( MRS_CONNECTION_TYPE_WS, arg_server_addr, server_port + 1, backlog );
        if ( NULL == ws_server ){
            MRS_LOG_ERR( "WS mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        wss_server = mrs_server_create( MRS_CONNECTION_TYPE_WSS, arg_server_addr, server_port + 2, backlog );
        if ( NULL == wss_server ){
            MRS_LOG_ERR( "WSS mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        tcp_ssl_server = mrs_server_create( MRS_CONNECTION_TYPE_TCP_SSL, arg_server_addr, server_port + 3, backlog );
        if ( NULL == tcp_ssl_server ){
            MRS_LOG_ERR( "TCP_SSL mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        mru_server = mrs_server_create( MRS_CONNECTION_TYPE_MRU, arg_server_addr, server_port + 4, backlog );
        if ( NULL == mru_server ){
            MRS_LOG_ERR( "MRU mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
            break;
        }
        
        mrs_server_set_new_connection_callback( tcp_server, on_new_connection );
        mrs_server_set_new_connection_callback( udp_server, on_new_connection );
        mrs_server_set_new_connection_callback( ws_server, on_new_connection );
        mrs_server_set_new_connection_callback( wss_server, on_new_connection );
        mrs_server_set_new_connection_callback( tcp_ssl_server, on_new_connection );
        mrs_server_set_new_connection_callback( mru_server, on_new_connection );
        
        mrs_set_error_callback( tcp_server, on_error );
        mrs_set_error_callback( udp_server, on_error );
        mrs_set_error_callback( ws_server, on_error );
        mrs_set_error_callback( wss_server, on_error );
        mrs_set_error_callback( tcp_ssl_server, on_error );
        mrs_set_error_callback( mru_server, on_error );
        
        if ( ! g_IsValidRecord ){
            mrs_set_read_callback( tcp_server, on_read );
            mrs_set_read_callback( udp_server, on_read );
            mrs_set_read_callback( ws_server, on_read );
            mrs_set_read_callback( wss_server, on_read );
            mrs_set_read_callback( tcp_ssl_server, on_read );
            mrs_set_read_callback( mru_server, on_read );
        }
        
        MRS_LOG_DEBUG( "%s listening on %s %u", connection_type_to_string( tcp_server ).c_str(), arg_server_addr, server_port );
        MRS_LOG_DEBUG( "%s waiting on %s %u", connection_type_to_string( udp_server ).c_str(), arg_server_addr, server_port );
        MRS_LOG_DEBUG( "%s listening on %s %u", connection_type_to_string( ws_server ).c_str(), arg_server_addr, server_port + 1 );
        MRS_LOG_DEBUG( "%s listening on %s %u", connection_type_to_string( wss_server ).c_str(), arg_server_addr, server_port + 2 );
        MRS_LOG_DEBUG( "%s listening on %s %u", connection_type_to_string( tcp_ssl_server ).c_str(), arg_server_addr, server_port + 3 );
        MRS_LOG_DEBUG( "%s waiting on %s %u", connection_type_to_string( mru_server ).c_str(), arg_server_addr, server_port + 4 );
        
        while ( true ){
            mrs_update();
            
            mrs_sleep( sleep_msec );
        }
    }while ( false );
    mrs_close( tcp_server );
    mrs_close( udp_server );
    mrs_close( ws_server );
    mrs_close( wss_server );
    mrs_close( tcp_ssl_server );
    mrs_close( mru_server );
    mrs_finalize();
    return 0;
}
