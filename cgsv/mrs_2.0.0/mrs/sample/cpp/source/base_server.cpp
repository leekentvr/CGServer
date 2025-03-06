#include <mrs.hpp>
#include <mrs/file.hpp>
#include <mrs/signal.hpp>

static bool         g_IsRun     = true;
static uint32       g_SleepMsec = 1;
static mrs::File    g_LogFile;

void on_sigint( int value ){
    MRS_LOG_DEBUG( "SIGINT" );
}

void on_sigterm( int value ){
    MRS_LOG_DEBUG( "SIGTERM" );
    g_IsRun = false;
}

void on_sigquit( int value ){
    MRS_LOG_DEBUG( "SIGQUIT" );
    g_IsRun = false;
}

void on_sighup( int value ){
    g_LogFile.Reopen();
    MRS_LOG_DEBUG( "SIGHUP" );
}

void on_log( MrsLogLevel level, const char* msg ){
    std::string time_stamp = mrs::DateTime::Now().ToString();
    
    char log_msg[ 256 ];
    int log_msg_len = snprintf( log_msg, sizeof( log_msg ), "[%s] %s", time_stamp.c_str(), msg );
    mrs_console_log( level, log_msg );
    g_LogFile.Write( log_msg, log_msg_len );
    g_LogFile.Printf( "\n" );
    g_LogFile.Flush();
}

int main( int argc, char** argv ){
    // ログ設定
    char log_file_path[ 256 ];
    snprintf( log_file_path, sizeof( log_file_path ), "%s.log", argv[ 0 ] );
    if ( ! g_LogFile.Open( log_file_path, "w" ) ){
        MRS_LOG_ERR( "Log file open error: path=%s", log_file_path );
        exit( 1 );
    }
    mrs_set_log_callback( on_log );
    
#ifndef MRS_WINDOWS
    // シグナルの受信設定
    mrs::Signal::SetCallback( SIGINT, on_sigint );
    mrs::Signal::SetCallback( SIGTERM, on_sigterm );
    mrs::Signal::SetCallback( SIGQUIT, on_sigquit );
    mrs::Signal::SetCallback( SIGHUP, on_sighup );
#endif
    
    // メイン処理
    mrs_initialize();
    while ( g_IsRun ){
        mrs_update();
        mrs_sleep( g_SleepMsec );
    }
    mrs_finalize();
    return 0;
}
