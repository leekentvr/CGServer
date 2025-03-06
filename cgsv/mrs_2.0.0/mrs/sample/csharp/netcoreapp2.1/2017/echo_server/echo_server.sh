#!/bin/bash

start()
{
    dotnet bin/$1/netcoreapp2.1/publish/${PROJ_NAME}.dll ${SLEEP_MSEC}=${SLEEP_MSEC_VAL} ${SERVER_ADDR}=${SERVER_ADDR_VAL} ${SERVER_PORT}=${SERVER_PORT_VAL} ${BACKLOG}=${BACKLOG_VAL} &
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
PROJ_NAME="echo_server_linux"

# コマンド
SLEEP_MSEC="--sleep_msec"
SERVER_ADDR="--server_addr"
SERVER_PORT="--server_port"
BACKLOG="--backlog"
IS_VALID_RECORD="--is_valid_record"

# コマンド値
SLEEP_MSEC_VAL=1
SERVER_ADDR_VAL="0.0.0.0"
SERVER_PORT_VAL=22222
BACKLOG_VAL=10
IS_VALID_RECORD_VAL=1

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

