#!/bin/bash

# jqコマンドの確認
if type jq > /dev/null 2>&1; then
    :
else
    if [ "$(uname)" == 'Darwin' ]; then
        # macOSの場合
        brew install jq
    elif [ "$(uname)" == 'Linux' ]; then
        if [ "$(which yum 2>/dev/null)" != "" ]; then
            # OS名の取得
            OS_NAME=`cat /etc/system-release`

            # OS毎にインストール方法を分ける
            if [[ "${OS_NAME}" =~ Amazon ]]; then
                # Amazon Linux2の場合
                sudo yum install -y jq
            else
                # epel のインストール
                sudo yum install -y epel-release

                # jq のインストール
                sudo yum install -y jq
            fi
        elif [ "$(which apt 2>/dev/null)" != "" ]; then
            # jq のインストール
            sudo apt install -y jq
        else
            echo "Invalid package manager"
        fi
    fi
fi
