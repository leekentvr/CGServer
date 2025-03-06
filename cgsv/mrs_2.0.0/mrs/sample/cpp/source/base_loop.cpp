#include <mrs.hpp>

int main( int argc, char** argv ){
    mrs_initialize();
    
    MRS_LOG_DEBUG( "[%s] start", mrs::DateTime::Now().ToString().c_str() );
    for ( int i = 0; i < 3; ++i ){
        mrs_update();
        
        MRS_LOG_DEBUG( "[%s] zzz %d", mrs::DateTime::Now().ToString().c_str(), i + 1 );
        mrs_sleep( 1000 );
    }
    MRS_LOG_DEBUG( "[%s] end", mrs::DateTime::Now().ToString().c_str() );
    
    mrs_finalize();
    return 0;
}
