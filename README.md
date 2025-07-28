#Установка серверного приложения
Для начала, необходимо установить нужное программное обеспечение, такое как git, OpenVPN, dotnet-sdk. Установка на системе Linux (Ubuntu). Данная платформа выбрана как наиболее подходящая и легкая для установки простого сервера и подробно рассказывается в различных ресурсах[16] [18]. Установка выполняется командами:
-	sudo apt install git
-	sudo apt install openvpn
-	sudo apt install dotnet-sdk-9.0
Далее следует установка, конфигурация и запуск программы:
-	iptables -t nat -A POSTROUTING -s 10.8.0.0/24 -o ens3 -j MASQUERADE
-	git clone https://github.com/JustEngineerLGTM/ShadowVpnApi.git
-	cd ShadowVpnApi/ShadowVPNApi
-	nano ~/Documents/shadowvpn/settings.toml #(необходимо установить пароль)
-	dotnet build
-	dotnet run
После этого приложение будет работать на порте 5000, текущего публичного ip сервера.

#Установка клиентского приложения
Необходимо установить OpenVPN, скачав с официального сайта, не меняя оригинальный путь установки. Затем запустить приложение через “ShadowVPN.exe” файл от имени администратора.
