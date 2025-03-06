#!/bin/bash

start()
{
    dotnet bin/$1/netcoreapp2.1/publish/base_server_linux.dll &
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
    dotnet restore base_server_linux.csproj
    dotnet publish base_server_linux.csproj --configuration $1
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

