docker volume create --driver local \
--opt type=none \
--opt device=/home/ubuntu/fileserver_volume \
--opt o=bind \
fileserver_volume