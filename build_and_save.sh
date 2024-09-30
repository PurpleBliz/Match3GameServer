#!/bin/bash

#   qYJdF56i*Fy_V3

IMAGE_NAME="match3server"
TAR_FILE="match3server.tar"
REMOTE_USER="root"
REMOTE_HOST="91.200.150.35"
REMOTE_PATH="/dockerfiles/"
CONTAINER_NAME="dev"

clear

echo "Start cleaning"
dotnet clean

echo "Start building"
dotnet build -c Release

if [ $? -ne 0 ]; then
  echo "Ошибка сборки. Прерывание."
  exit 1
fi

echo "Start publishing"
dotnet publish -c Release --no-restore

if [ $? -ne 0 ]; then
  echo "Ошибка публикации. Прерывание."
  exit 1
fi

echo "Start docker-compose building"
docker-compose build --no-cache

if [ $? -ne 0 ]; then
  echo "Ошибка построения Docker-образа. Прерывание."
  exit 1
fi

echo "Start saving image"
docker save -o ${TAR_FILE} ${IMAGE_NAME}

if [ $? -ne 0 ]; then
  echo "Ошибка сохранения Docker-образа. Прерывание."
  exit 1
fi

echo "Start copy file to server"
scp ${TAR_FILE} ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_PATH}

if [ $? -ne 0 ]; then
  echo "Ошибка копирования tar-файла. Прерывание."
  exit 1
fi

echo "Copying tar-file completed successfully."

echo "Start removing ${TAR_FILE}"
rm ./${TAR_FILE}

if [ -f "./${TAR_FILE}" ]; then
  echo "File ${TAR_FILE} has not deleted."
else
  echo "File ${TAR_FILE} successful deleted."
fi

echo "Connecting to remote server and executing commands..."

ssh ${REMOTE_USER}@${REMOTE_HOST} << EOF
  set -e

  if docker ps | grep -q ${CONTAINER_NAME}; then
    echo "Stopping container '${CONTAINER_NAME}'..."
    docker stop ${CONTAINER_NAME}
  else
    echo "Container 'dev' is not running."
  fi

  if docker ps -a | grep -q ${CONTAINER_NAME}; then
    echo "Removing container '${CONTAINER_NAME}'..."
    docker rm ${CONTAINER_NAME}
  else
    echo "Container '${CONTAINER_NAME}' does not exist."
  fi

  if [ -n "\$(docker images -q)" ]; then
    echo "Removing all Docker images..."
    docker rmi \$(docker images -q)
  else
    echo "No Docker images to remove."
  fi

  echo "Loading Docker image 'fn-backend'..."
  docker load -i ${REMOTE_PATH}/${TAR_FILE}

  if ! docker ps -a | grep -q ${CONTAINER_NAME}; then
    echo "Running Docker container '${CONTAINER_NAME}'..."
    docker run -d -p 443:7287 --name ${CONTAINER_NAME} ${IMAGE_NAME}
  else
    echo "Container '${CONTAINER_NAME}' is already running."
  fi
EOF

echo "All steps completed successfully."