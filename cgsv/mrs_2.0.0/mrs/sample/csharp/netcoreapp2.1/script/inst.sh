#!/bin/bash

if [ "$(which yum 2>/dev/null)" != "" ]; then
    # dotnet-sdk 用リポジトリの追加
    sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
    sudo sh -c 'echo -e "[packages-microsoft-com-prod]\nname=packages-microsoft-com-prod \nbaseurl=https://packages.microsoft.com/yumrepos/microsoft-rhel7.3-prod\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/dotnetdev.repo'
    
    # yum update
    sudo yum -y update
    
    # libunwind, libicu のインストール
    sudo yum install -y libunwind libicu
    
    # .NET Core 2.0のインストール
    sudo yum install -y dotnet-sdk-2.1.200
    
    # .NET Core 2.1のインストール
    sudo yum install -y dotnet-sdk-2.1
    
    # シンボリックリンクが消える事があるので作成しておく
    if [ ! -e /usr/bin/dotnet ]; then
        sudo ln -s /usr/share/dotnet/dotnet /usr/bin
    fi
elif [ "$(which apt 2>/dev/null)" != "" ]; then
    echo TODO
else
    echo "Invalid package manager"
fi
