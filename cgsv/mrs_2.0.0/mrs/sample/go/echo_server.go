package main

import (
    "fmt"
    "os"
    "net/http"
    "golang.org/x/net/websocket"
    "io"
    "io/ioutil"
    "strconv"
)

func on_websocket( connection *websocket.Conn ){
    for {
        var data []byte
        err := websocket.Message.Receive( connection, &data )
        if nil != err {
            if io.EOF != err {
                fmt.Printf( "read error: %s\n", err )
            }
            return
        }
        
        err = websocket.Message.Send( connection, data )
        if nil != err {
            fmt.Printf( "write error: %s\n", err )
            return
        }
    }
}

func on_http_websocket( w http.ResponseWriter, r *http.Request ){
    fmt.Printf( "on_http_websocket url=%s host=%s\n", r.URL, r.Host )
    s := websocket.Server{ Handler: on_websocket }
    s.ServeHTTP( w, r )
}

func on_http( w http.ResponseWriter, r *http.Request ){
    fmt.Printf( "on_http url=%s host=%s\n", r.URL, r.Host )
    bytes, err := ioutil.ReadAll( r.Body )
    if nil != err {
        fmt.Printf( "read body error: %s\n", err )
        return
    }
    w.Write( bytes )
}

func http_listen_and_serve( addr string, ssl_cert_file string, ssl_key_file string ){
    if ( "" != ssl_cert_file ) && ( "" != ssl_key_file ) {
        fmt.Printf( "http.ListenAndServeTLS %s %s %s\n", addr, ssl_cert_file, ssl_key_file )
        http.ListenAndServeTLS( addr, ssl_cert_file, ssl_key_file, nil )
    }else{
        fmt.Printf( "http.ListenAndServe %s\n", addr )
        http.ListenAndServe( addr, nil )
    }
}

func main(){
    arg_is_websocket  := "1"
    arg_server_addr   := "0.0.0.0"
    arg_server_port   := "22223"
    arg_server_path   := "/"
    arg_ssl_cert_file := ""
    arg_ssl_key_file  := ""
    
    argc := len( os.Args )
    for argi := 1; argi <= 6; argi++ {
        switch argi {
        case 1:
            fmt.Printf( "arg %02d: is_websocket [%s]\n", argi, arg_is_websocket )
            if argi < argc {
                arg_is_websocket = os.Args[ argi ]
            }
        case 2:
            fmt.Printf( "arg %02d: server_addr [%s]\n", argi, arg_server_addr )
            if argi < argc {
                arg_server_addr = os.Args[ argi ]
            }
        case 3:
            fmt.Printf( "arg %02d: server_port [%s]\n", argi, arg_server_port )
            if argi < argc {
                arg_server_port = os.Args[ argi ]
            }
        case 4:
            fmt.Printf( "arg %02d: server_path [%s]\n", argi, arg_server_path )
            if argi < argc {
                arg_server_path = os.Args[ argi ]
            }
        case 5:
            fmt.Printf( "arg %02d: ssl_cert_file [%s]\n", argi, arg_ssl_cert_file )
            if argi < argc {
                arg_ssl_cert_file = os.Args[ argi ]
            }
        case 6:
            fmt.Printf( "arg %02d: ssl_key_file [%s]\n", argi, arg_ssl_key_file )
            if argi < argc {
                arg_ssl_key_file = os.Args[ argi ]
            }
        }
    }
    fmt.Printf( "is_websocket=%s server_addr=%s server_port=%s server_path=%s ssl_cert_file=%s ssl_key_file=%s\n",
        arg_is_websocket, arg_server_addr, arg_server_port, arg_server_path, arg_ssl_cert_file, arg_ssl_key_file )
    
    is_websocket, _ := strconv.Atoi( arg_is_websocket )
    if 0 != is_websocket {
        http.HandleFunc( arg_server_path, on_http_websocket )
    }else{
        http.HandleFunc( arg_server_path, on_http )
    }
    http_listen_and_serve( arg_server_addr +":"+ arg_server_port, arg_ssl_cert_file, arg_ssl_key_file )
}
