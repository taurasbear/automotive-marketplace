#!/bin/bash
mkdir -p ~/automotive-marketplace

cat > ~/automotive-marketplace/.env << EOL
POSTGRES_USER=${POSTGRES_USER}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_DB=${POSTGRES_DB}
JWT_KEY=${JWT_KEY}
JWT_ISSUER=${JWT_ISSUER}
JWT_AUDIENCE=${JWT_AUDIENCE}
JWT_ACCESSTOKENEXPIRATIONMINUTES=${JWT_ACCESSTOKENEXPIRATIONMINUTES}
JWT_REFRESHTOKENEXPIRATIONDAYS=${JWT_REFRESHTOKENEXPIRATIONDAYS}
VITE_APP_API_URL=${VITE_APP_API_URL}
DOCKER_REGISTRY=ghcr.io/${GITHUB_REPOSITORY_OWNER}/
EOL

mkdir -p ~/automotive-marketplace/nginx/conf
mkdir -p ~/automotive-marketplace/nginx/ssl
mkdir -p ~/automotive-marketplace/nginx/logs

echo ${DOCKER_GITHUB_TOKEN} | docker login ghcr.io -u ${GITHUB_REPOSITORY_OWNER} --password-stdin

cd ~/automotive-marketplace
docker-compose pull
docker-compose down
docker-compose up -d