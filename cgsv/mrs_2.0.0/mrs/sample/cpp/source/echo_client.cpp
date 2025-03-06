#include <mrs.hpp>
#include <mrs/connect.hpp>

static bool         g_IsRun            = true;
static bool         g_IsKeyExchange    = true;
static bool         g_IsEncryptRecords = true;
static uint16       g_RecordOptions    = 0;
static uint32       g_WriteDataLen     = 0;
static uint32       g_WriteCount       = 0;
static uint32       g_Connections      = 0;
static uint32       g_ReadCount        = 0;
static char         g_Header[ 128 ];
static mrs::Connect g_Connect;
static bool         g_IsValidRecord    = true;
static const char*  g_ConnectionPath   = "";

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

void read_echo( MrsConnection connection, const void* payload, uint32 payload_len ){
    mrs::Time read_time;
    read_time.Set();
    mrs::Buffer buffer;
    buffer.Write( payload, payload_len );
    while ( 0 < buffer.GetDataLen() ){
        mrs::Time write_time = buffer.ReadTime();
        MRS_LOG_DEBUG( "read_echo data=%s data_len=%u diff_time=%s(%s - %s)",
            buffer.GetData(), g_WriteDataLen,
            ( read_time - write_time ).ToString().c_str(), read_time.ToString().c_str(), write_time.ToString().c_str() );
        if ( ! buffer.Read( NULL, g_WriteDataLen ) ){
            MRS_LOG_ERR( "Lost data. len=%u %s", buffer.GetDataLen(), mrs::Utility::ToHex( buffer.GetData(), buffer.GetDataLen() ).c_str() );
            g_IsRun = false;
            break;
        }
        
        ++g_ReadCount;
        if ( g_WriteCount * g_Connections <= g_ReadCount ){
            MRS_LOG_DEBUG( "Since all records have been received, it is finished." );
            g_IsRun = false;
        }
    }
}

void parse_record( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len ){
    MRS_LOG_DEBUG( "parse_record seqnum=%u options=0x%02X payload=0x%02X/%u", seqnum, options, payload_type, payload_len );
    // MRS_PAYLOAD_TYPE_BEGIN - MRS_PAYLOAD_TYPE_ENDの範囲内で任意のIDを定義し、対応するアプリケーションコードを記述する
    switch ( payload_type ){
    case 0x01:{
        read_echo( connection, payload, payload_len );
    }break;
    
    default:{}break;
    }
}

void write_echo( MrsConnection connection, char* data, uint32 data_len ){
    mrs::Time write_time;
    write_time.Set();
    mrs::Buffer buffer;
    buffer.WriteTime( &write_time );
    buffer.Write( data, data_len );
    
    if ( g_IsValidRecord ){
        mrs_write_record( connection, g_RecordOptions, 0x01, buffer.GetData(), buffer.GetDataLen() );
    }else{
        mrs_write( connection, buffer.GetData(), buffer.GetDataLen() );
    }
}

void write_echo_all( MrsConnection connection ){
    uint32 header_len = snprintf( g_Header, sizeof( g_Header ), "%s ", connection_type_to_string( connection ).c_str() );
    header_len += snprintf( &g_Header[ header_len ], sizeof( g_Header ) - header_len, "%s", ( g_IsKeyExchange && g_IsEncryptRecords ) ? "  CRYPT" : "NOCRYPT" );
    
    char* data = (char*)MRS_MALLOC( g_WriteDataLen );
    if ( NULL == data ){
        MRS_LOG_ERR( "write_echo_all malloc error: size=%u", g_WriteDataLen );
        return;
    }
    
    memset( data, 0xFF, g_WriteDataLen );
    for ( uint32 i = 0; i < g_WriteCount; ++i ){
        snprintf( data, g_WriteDataLen, "%s %llu: %u", g_Header, (uint64)connection, i + 1 );
        write_echo( connection, data, g_WriteDataLen );
    }
    MRS_FREE( data );
}

// 鍵交換した時に呼ばれる
void on_key_exchange( MrsConnection connection, void* connection_data ){
    MRS_LOG_DEBUG( "on_key_exchange" );
    
    write_echo_all( connection );
}

// ソケット接続時に呼ばれる
void on_connect( MrsConnection connection, void* connection_data ){
    MRS_LOG_DEBUG( "on_connect local_mrs_version=0x%x remote_mrs_version=0x%x",
        mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
    
    if ( g_IsKeyExchange ){
        mrs_set_cipher( connection, mrs_cipher_create( MRS_CIPHER_TYPE_ECDH ) );
        mrs_key_exchange( connection, on_key_exchange );
    }else{
        write_echo_all( connection );
    }
}

// ソケット切断時に呼ばれる
void on_disconnect( MrsConnection connection, void* connection_data ){
    MRS_LOG_DEBUG( "on_disconnect local_mrs_version=0x%x remote_mrs_version=0x%x",
        mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ) );
}

// ソケットにエラーが発生した時に呼ばれる
void on_error( MrsConnection connection, void* connection_data, MrsConnectionError status ){
    switch ( status ){
    case MRS_CONNECT_ERROR:
    case MRS_CONNECT_TIMEOUT:{
        MrsConnection client = g_Connect.FallbackConnect( connection );
        if ( NULL != client ) return;
    }break;
    
    default: break;
    }
    
    MRS_LOG_DEBUG( "on_error local_mrs_version=0x%x remote_mrs_version=0x%x status=%s",
        mrs_get_version( MRS_VERSION_KEY ), mrs_connection_get_remote_version( connection, MRS_VERSION_KEY ), mrs_get_connection_error_string( status ) );
}

// レコード受信時に呼ばれる
void on_read_record( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len ){
    parse_record( connection, connection_data, seqnum, options, payload_type, payload, payload_len );
}

// バイナリデータ受信時に呼ばれる
void on_read( MrsConnection connection, void* connection_data, const void* data, uint32 data_len ){
    read_echo( connection, data, data_len );
}

// フォールバック接続時に呼ばれる
void on_fallback_connect( MrsConnection connection, const mrs::Connect::Request* request ){
    MRS_LOG_DEBUG( "on_fallback_connect connection_type=%d addr=%s port=%u timeout_msec=%u", (int)request->connection_type, request->addr, request->port, request->timeout_msec );
    mrs_set_connect_callback( connection, on_connect );
    mrs_set_disconnect_callback( connection, on_disconnect );
    mrs_set_error_callback( connection, on_error );
    
    if ( g_IsValidRecord ){
        mrs_set_read_record_callback( connection, on_read_record );
    }else{
        mrs_set_read_callback( connection, on_read );
    }
    mrs_connection_set_path( connection, g_ConnectionPath );
}

int main( int argc, char** argv ){
    int argi = 1;
    MRS_LOG_DEBUG( "arg %02d: connection_type(TCP:%d UDP:%d WS:%d WSS:%d TCP_SSL:%d MRU:%d TCP -> WSS -> WS:*) [1]", argi++, MRS_CONNECTION_TYPE_TCP, MRS_CONNECTION_TYPE_UDP, MRS_CONNECTION_TYPE_WS, MRS_CONNECTION_TYPE_WSS, MRS_CONNECTION_TYPE_TCP_SSL, MRS_CONNECTION_TYPE_MRU );
    MRS_LOG_DEBUG( "arg %02d: is_key_exchange(OFF:0 ON:1) [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: is_encrypt_records(OFF:0 ON:1) [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: write_data_len [1024]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: write_count [10]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: connections [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: sleep_msec [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: server_addr [127.0.0.1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: server_port [22222]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: timeout_msec [5000]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: is_valid_record(OFF:0 ON:1) [1]", argi++ );
    MRS_LOG_DEBUG( "arg %02d: connection_path [/]", argi++ );
    
    argi = 1;
    const char* arg_connection_type    = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_is_key_exchange    = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_is_encrypt_records = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_write_data_len     = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1024";
    const char* arg_write_count        = ( ++argi <= argc ) ? argv[ argi - 1 ] : "10";
    const char* arg_connections        = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_sleep_msec         = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_server_addr        = ( ++argi <= argc ) ? argv[ argi - 1 ] : "127.0.0.1";
    const char* arg_server_port        = ( ++argi <= argc ) ? argv[ argi - 1 ] : "22222";
    const char* arg_timeout_msec       = ( ++argi <= argc ) ? argv[ argi - 1 ] : "5000";
    const char* arg_is_valid_record    = ( ++argi <= argc ) ? argv[ argi - 1 ] : "1";
    const char* arg_connection_path    = ( ++argi <= argc ) ? argv[ argi - 1 ] : "/";
    MRS_LOG_DEBUG( "connection_type=%s is_key_exchange=%s is_encrypt_records=%s write_data_len=%s write_count=%s connections=%s sleep_msec=%s server_addr=%s server_port=%s timeout_msec=%s is_valid_record=%s connection_path=%s",
        arg_connection_type, arg_is_key_exchange, arg_is_encrypt_records, arg_write_data_len, arg_write_count, arg_connections,
        arg_sleep_msec, arg_server_addr, arg_server_port, arg_timeout_msec,
        arg_is_valid_record, arg_connection_path );
    
    g_IsKeyExchange = ( 0 != atoi( arg_is_key_exchange ) );
    g_IsEncryptRecords = ( 0 != atoi( arg_is_encrypt_records ) );
    g_RecordOptions = g_IsEncryptRecords ? MRS_RECORD_OPTION_ON_CRYPT : MRS_RECORD_OPTION_NONE;
//    g_RecordOptions |= MRS_RECORD_OPTION_UDP_UNRELIABLE;
//    g_RecordOptions |= MRS_RECORD_OPTION_UDP_UNSEQUENCED;
    g_WriteDataLen = strtoul( arg_write_data_len, NULL, 0 );
    g_WriteCount = strtoul( arg_write_count, NULL, 0 );
    g_Connections = strtoul( arg_connections, NULL, 0 );
    g_IsValidRecord = ( 0 != atoi( arg_is_valid_record ) );
    g_ConnectionPath = arg_connection_path;
    
    uint32 sleep_msec = strtoul( arg_sleep_msec, NULL, 0 );
    mrs::Connect::Request connect_request;
    connect_request.connection_type = MRS_CONNECTION_TYPE_NONE;
    connect_request.addr            = arg_server_addr;
    connect_request.port            = (uint16)strtoul( arg_server_port, NULL, 0 );
    connect_request.timeout_msec    = (uint32)strtoul( arg_timeout_msec, NULL, 0 );
    int32 connection_type = atoi( arg_connection_type );
    switch ( connection_type ){
    case MRS_CONNECTION_TYPE_TCP:
    case MRS_CONNECTION_TYPE_UDP:
    case MRS_CONNECTION_TYPE_WS:
    case MRS_CONNECTION_TYPE_WSS:
    case MRS_CONNECTION_TYPE_TCP_SSL:
    case MRS_CONNECTION_TYPE_MRU:{
        connect_request.connection_type = (MrsConnectionType)connection_type;
        g_Connect.AddRequest( &connect_request );
    }break;
    
    default:{
        connect_request.connection_type = MRS_CONNECTION_TYPE_TCP;
        g_Connect.AddRequest( &connect_request );
        connect_request.connection_type = MRS_CONNECTION_TYPE_WSS;
        connect_request.port += 2;
        g_Connect.AddRequest( &connect_request );
        connect_request.connection_type = MRS_CONNECTION_TYPE_WS;
        connect_request.port -= 1;
        g_Connect.AddRequest( &connect_request );
    }break;
    }
    g_Connect.SetFallbackConnectCallback( on_fallback_connect );
    mrs_initialize();
    do{
        uint32 connections = 0;
        for ( uint32 i = 0; i < g_Connections; ++i ){
            MrsConnection client = g_Connect.FallbackConnect();
            if ( NULL == client ){
                MRS_LOG_ERR( "mrs_connect[%u]: %s", i, mrs_get_error_string( mrs_get_last_error() ) );
                break;
            }
            
            ++connections;
        }
        if ( connections != g_Connections ) break;
        
        while ( g_IsRun ){
            mrs_update();
            
            mrs_sleep( sleep_msec );
        }
    }while ( false );
    mrs_finalize();
    return 0;
}
