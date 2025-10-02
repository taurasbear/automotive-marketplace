#!/bin/bash

set -euo pipefail

log_info() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] INFO: $1"
}

log_error() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ERROR: $1" >&2
}

log_warning() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] WARNING: $1" >&2
}

check_env_vars() {
    local missing_vars=()
    local required_vars=(
        "POSTGRES_USER"
        "POSTGRES_PASSWORD" 
        "POSTGRES_DB"
        "JWT_KEY"
        "JWT_ISSUER"
        "JWT_AUDIENCE"
        "JWT_ACCESSTOKENEXPIRATIONMINUTES"
        "JWT_REFRESHTOKENEXPIRATIONDAYS"
        "VITE_APP_API_URL"
        "DOCKER_GITHUB_TOKEN"
        "GITHUB_REPOSITORY_OWNER"
        "MINIO_CONSOLE_URL"
        "MINIO_BUCKETNAME"
        "MINIO_PRESIGNEDURLEXPIRATIONHOURS"
        "MINIO_SERVERURL"
        "MINIO_ACCESSKEY"
        "MINIO_SECRETKEY"
    )
    
    log_info "Checking required environment variables..."
    
    for var in "${required_vars[@]}"; do
        if [[ -z "${!var:-}" ]]; then
            missing_vars+=("$var")
            log_error "Missing required environment variable: $var"
        else
            log_info "✓ $var is set"
        fi
    done
    
    if [[ ${#missing_vars[@]} -gt 0 ]]; then
        log_error "Deployment cannot continue. Missing environment variables: ${missing_vars[*]}"
        exit 1
    fi
    
    log_info "All required environment variables are present"
}

log_info "Starting deployment process..."

check_env_vars

# Export all variables
export POSTGRES_USER POSTGRES_PASSWORD POSTGRES_DB JWT_KEY JWT_ISSUER JWT_AUDIENCE JWT_ACCESSTOKENEXPIRATIONMINUTES JWT_REFRESHTOKENEXPIRATIONDAYS VITE_APP_API_URL DOCKER_GITHUB_TOKEN GITHUB_REPOSITORY_OWNER MINIO_CONSOLE_URL MINIO_BUCKETNAME MINIO_PRESIGNEDURLEXPIRATIONHOURS MINIO_SERVERURL MINIO_ACCESSKEY MINIO_SECRETKEY

log_info "Creating application directory..."
mkdir -p ~/automotive-marketplace

log_info "Generating .env file..."
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
MINIO_CONSOLE_URL=${MINIO_CONSOLE_URL}
MINIO_BUCKETNAME=${MINIO_BUCKETNAME}
MINIO_PRESIGNEDURLEXPIRATIONHOURS=${MINIO_PRESIGNEDURLEXPIRATIONHOURS}
MINIO_SERVERURL=${MINIO_SERVERURL}
MINIO_ACCESSKEY=${MINIO_ACCESSKEY}
MINIO_SECRETKEY=${MINIO_SECRETKEY}
DOCKER_REGISTRY=ghcr.io/${GITHUB_REPOSITORY_OWNER}/
EOL
log_info "✓ .env file created successfully"

log_info "Creating nginx directories..."
mkdir -p ~/automotive-marketplace/nginx/conf
mkdir -p ~/automotive-marketplace/nginx/logs
log_info "✓ Nginx directories created"

log_info "Logging into Docker registry..."
if echo ${DOCKER_GITHUB_TOKEN} | docker login ghcr.io -u ${GITHUB_REPOSITORY_OWNER} --password-stdin; then
    log_info "✓ Successfully logged into Docker registry"
else
    log_error "Failed to login to Docker registry"
    exit 1
fi

log_info "Changing to application directory..."
cd ~/automotive-marketplace

log_info "Stopping existing containers..."
if docker-compose down; then
    log_info "✓ Containers stopped successfully"
else
    log_warning "Failed to stop containers (they might not be running)"
fi

log_info "Cleaning up Docker system..."
if docker system prune -f; then
    log_info "✓ Docker system cleaned up"
else
    log_warning "Docker system cleanup had some issues"
fi

log_info "Pulling latest images..."
if docker-compose pull; then
    log_info "✓ Images pulled successfully"
else
    log_error "Failed to pull images"
    exit 1
fi

log_info "Starting containers..."
if docker-compose up -d; then
    log_info "✓ Containers started successfully"
    log_info "Deployment completed successfully!"
else
    log_error "Failed to start containers"
    exit 1
fi

log_info "Deployment process finished"