#!/bin/bash
#downloading input.txt file
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/code/input.txt
#downloading deploy1.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/deploy1.sh
#downloading configupdate2.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/configupdate2.sh
#downloading commonupdate3.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/commonupdate3.sh
#downloading edisonwebenvupdate4.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/edisonwebenvupdate4.sh
#downloading updateappsettings5.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/updateappsettings5.sh
#downloading imagesupdate6.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/imagesupdate6.sh
#downloading clusterconnect7.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/clusterconnect7.sh
#downloading set-kubernetes-config8.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/set-kubernetes-config8.sh
#downloading helminstall9.sh
sudo wget -P /var/lib/waagent/custom-script/download/0 https://raw.githubusercontent.com/sysgain/Iot-ProjectEdison/dev/scripts/helminstall9.sh

cd /var/lib/waagent/custom-script/download/0
chmod +x deploy1.sh configupdate2.sh commonupdate3.sh edisonwebenvupdate4.sh updateappsettings5.sh imagesupdate6.sh clusterconnect7.sh set-kubernetes-config8.sh helminstall9.sh


