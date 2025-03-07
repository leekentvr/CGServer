#include "mrs.hpp"
#include <stdio.h>


void on_connect( MrsConnection connection ){
    fprintf(stderr,"on_connect\n");
}

void on_disconnect( MrsConnection connection, void* connection_data ){
    fprintf(stderr,"on_disconnect\n");    
}

void on_error( MrsConnection connection, void* connection_data, MrsConnectionError status ){
    fprintf(stderr,"on_error: %s\n", mrs_get_error_string(mrs_get_last_error()));
    fprintf(stderr,"quitting\n");
    exit(1);
}

// Callback function when receiving a record from client. Many records can be contained in a TCP packet,
// A single record can also be divided into many TCP packets.
void on_read_record( MrsConnection connection, void* connection_data, uint32 seqnum, uint16 options, uint16 payload_type, const void* payload, uint32 payload_len ){
    fprintf(stderr, "on_read_record: received record with seqnum=%u, options=0x%02X, payload_type=0x%02X, payload_len=%u\n", seqnum, options, payload_type, payload_len);

    switch ( payload_type ){
    case 0x01:
        fprintf(stderr, "Received record type 0x01: payload= '%.*s' (length: %u)\n", payload_len, (const char*)payload, payload_len);
        mrs_write_record( connection, options, payload_type, payload, payload_len );
        break;    
    default:
        break;
    }
}

// Callback function when accepting new connection .
void on_new_connection( MrsServer server, void* server_data, MrsConnection client ){
    fprintf(stderr,"on_new_connection\n");
    
    mrs_set_disconnect_callback( client, on_disconnect );
    mrs_set_error_callback( client, on_error );
    mrs_set_read_record_callback( client, on_read_record );
    on_connect( client );
}

int main( int argc, char** argv ){

    mrs_initialize();

    MrsServer tcp_server=NULL;
    tcp_server = mrs_server_create( MRS_CONNECTION_TYPE_TCP, "0.0.0.0", 5333, 10 ); // listen addr and port.
    if(!tcp_server){
        fprintf(stderr,"TCP mrs_server_create: %s", mrs_get_error_string( mrs_get_last_error() ) );
        return 0;
    }

    // Configure callback functions.
    mrs_server_set_new_connection_callback( tcp_server, on_new_connection );
    mrs_set_error_callback( tcp_server, on_error );
    fprintf(stderr,"server starting\n");

    // main loop
    bool done=false;
    while(!done){
        mrs_update();
        mrs_sleep(1); // 1ms sleep (Loop about 1000 times per second)
    };
    
    mrs_close( tcp_server );
    mrs_finalize();
    return 0;
}
