#!/bin/bash

timezone=$1
if [ -z "${timezone}" ]; then
    echo "Invalid timezone: ${timezone}"
    exit 1
fi

if [ "$(uname)" = "Darwin" ]; then
    echo "sudo rm /etc/localtime; sudo ln -s /usr/share/zoneinfo/${timezone} /etc/localtime"
elif [ "$(which timedatectl 2>/dev/null)" != "" ]; then
    sudo timedatectl set-timezone ${timezone}
else
    sudo cp /usr/share/zoneinfo/${timezone} /etc/localtime
fi

