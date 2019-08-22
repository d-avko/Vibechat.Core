docker run -v /home/ubuntu/fileserver_volume/_data/wwwroot:/app/wwwroot \
-p 5000:5000 -e "ASPNETCORE_URLS=http://*:5000" vibechat.fileserver
