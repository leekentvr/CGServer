#!/bin/bash

start()
{
    dotnet bin/$1/netcoreapp2.1/publish/${PROJ_NAME}.dll &
}

stop()
{
    MRS_MMO_PID=`ps aux | grep -e dotnet | grep -v 'grep' | awk '{ print $2 }'`
    kill -15 ${MRS_MMO_PID}
}

hup()
{
    MRS_MMO_PID=`ps aux | grep -e dotnet | grep -v 'grep' | awk '{ print $2 }'`
    kill -1 ${MRS_MMO_PID}
}

build()
{
    dotnet restore ${PROJ_NAME}.csproj
    dotnet publish ${PROJ_NAME}.csproj --configuration $1
    cp ../../../../../library/mrs/linux/centos7_4.8.5/enet_uv_openssl_1.1.1/libmrs.so bin/$1/netcoreapp2.1
    cp ../../../../../library/mrs/linux/centos7_4.8.5/enet_uv_openssl_1.1.1/libmrs.so bin/$1/netcoreapp2.1/publish
}

clean()
{
    printf "password: "
    read password
    echo "$password" | sudo -S rm -rf ./obj
    echo "$password" | sudo -S rm -rf ./bin
}

# プロジェクト名
PROJ_NAME="echo_client_linux"

# コマンド
CONNECTION_TYPE="--connectionType"
IS_KEY_EXCHANGE="--is_key_exchange"
IS_ENCRYPT_RECORDS="--is_encrypt_records"
WRITE_DATA_LEN="--write_data_len"
WRITE_COUNT="--write_count"
SLEEP_MSEC="--sleep_msec"
SERVER_ADDR="--server_addr"
SERVER_PORT="--server_port"
TIMEOUT_MSEC="--timeout_msec"
CONNECTIONS="--connections"
IS_VALID_RECORD="--is_valid_record"
CONNECTION_PATH="--connection_path"

# コマンド値
CONNECTION_TYPE_VAL=1
IS_KEY_EXCHANGE_VAL=1
IS_ENCRYPT_RECORDS_VAL=1
WRITE_DATA_LEN_VAL=1024
WRITE_COUNT_VAL=10
SLEEP_MSEC_VAL=1
SERVER_ADDR_VAL="127.0.0.1"
SERVER_PORT_VAL=22222
TIMEOUT_MSEC_VAL=5000
CONNECTIONS_VAL=1
IS_VALID_RECORD_VAL=1
CONNECTION_PATH_VAL="/"

# コマンドの実行
case "$1" in
    start)
        start Debug
        ;;
    startd)
        start Debug
        ;;
    startr)
        start Release
        ;;
    stop)
        stop
        ;;
    hup)
        hup
        ;;
    restart)
        stop
        sleep 3
        start
        ;;
    debug)
        build Debug
        ;;
    release)
        build Release
        ;;
    clean)
        clean
        ;;
    *)
        echo "Unknown type: [$1]"
        ;;
esac

