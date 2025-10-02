<div align="center">
  <h1 style="font-family: 'Montserrat', Arial, Helvetica, sans-serif; font-weight: 700; letter-spacing: 2px;">
    Automotive<br>Marketplace
  </h1>
<br>

[![Deployed App][deployed-app-shield]][deployed-app-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<br>

[![React][React.js]][React-url]
[![TypeScript][TypeScript]][TypeScript-url]
[![Tailwind][tailwindcss]][Tailwind-url]
[![ASP.NET Core][ASP.NET Core]][ASP.NET-Core-url]
[![PostgreSQL][PostgreSQL]][PostgreSQL-url]
[![Docker][Docker]][Docker-url]
[![Vite][Vite]][vite-url]

</div>

<div align="center">

</div>

## About

A web app for users to purchase or sell automobiles. It provides features for **browsing**, **searching** and **filtering** car listings by make, model, etc. Moreover, users who want to sell their vehicles can **create listings** for them.

### Screenshots

<img width="1920" height="964" alt="Web app main page" src="https://github.com/user-attachments/assets/5d3887ee-fc63-4063-9b67-711fb35d2c7f" />

<br><br>

<img width="1920" height="996" alt="Listing search results" src="https://github.com/user-attachments/assets/684b51e2-63b7-43ca-b8d9-20a3159e9ec8" />

### Reason behind project

In the beginning, I set out to build something that would give me hands-on experience in developing with scalability and maintenance in mind while also giving CI/CD a try. To achieve this, I chose to use containerization[^1] - specifically **Docker Compose** in order to have a consistent environment which would make it easy to deploy.

### Achievements

- Got real experience with CI/CD and deployment
- Followed good project management practices
- Got better with containerization using Docker
- Learnt to write more scalable code
- Learnt modern frameworks and libraries like Tailwind, TanStack, EF Core, TestContainers etc.

### Project highlights

- CI/CD implemented with GitHub Actions - [cd-cd.yml](/.github/workflows/ci-cd.yml)
- Deployed on a [DigitalOcean's Droplet](https://www.digitalocean.com/products/droplets)
- Project management using [GitHub Projects](https://github.com/users/taurasbear/projects/1)
- Fully implemented **JWT Auth**
- **BE testing** implemented with real databases
- Use of a **Amazon S3** compatible image storage service
- Fully containerized development environment (including debugging)
- Let's Encrypt for certification generation
- Nginx as a reverse-proxy and web server

<img width="1920" height="964" alt="Image" src="https://github.com/user-attachments/assets/b563c6da-e7f9-4d14-9e2d-afc597e418f1" />

### Design Patterns

- CQRS
- Clean architecture
- Repository
- Unit of Work (via EF Core)

## Architecture

### Overview

The project runs as a monolithic Docker Compose instance. In production, each service - frontend, backend, database[^1], image storage service and reverse-proxy run in their own containers and are orchestrated by [Docker Compose](/docker-compose.yml). This approach brings a lot of advantages such as consistent and isolated service environments and ease of deployment. However, a monolithic architecture is more difficult to set up initially and has issues with individual scaling of services. But since this is a personal project, I felt it was appropriate to choose this approach for its advantages.

### Testing

Tests run in a separate container using `TestContainers` NuGet package. Each test class gets its own database inside the container which means tests can safely run in parallel. Moroever, each test method has a dispose procedure which uses the `Respawn` NuGet package to reset the database state without having to recreate it. [Inspiration](https://www.youtube.com/watch?v=vy1aIT5Ppj8).

### Auth

Auth is implemented using **Refresh** and **Access** JWTs (JSON Web Tokens). Access tokens are short-lived while Refresh tokens have a long lifespan. If a request is rejected because of the lack of an access token, Axios middleware will try to refresh it using the refresh token and hitting the `/auth/refresh/` endpoint.

### Use-case diagram

!["Use-case diagram"](docs/diagrams/images/use-cases.drawio.svg)

### Entity-relationship diagram

!["Entity-relationship diagram"](docs/diagrams/images/entity-relationship-diagram.drawio.svg)

## CI/CD

The project's CI/CD pipeline is implemented using GitHub Actions and consists of 3 stages:

1. **Build and test**

   - The project gets built and runs tests
   - If any tests fail, the workflow stops

2. **Build images and push**

   - Build Docker images for the frontend and backend using their respective Dockerfiles
   - On success, push the images to the GitHub Container Registry (GHCR)

3. **Deploy**
   - SSHs into the production Droplet
   - Pull the latest Docker images from GHCR
   - Retrieve secrets from GitHub Repository secrets and generate the `.env` file.
   - Copy the `docker-compose.yml` from the repository.
   - Start the services using Docker Compose.

## Getting started

### Prerequisites

- Docker and Docker Compose installed on your machine [^2]
- `docker-compose.yml` is for production while `docker-compose.override.yml` is for development

### Setup

1. Create a `.env` file at the project root:

```dotenv
DEV_DOCKER_DB_CONNECTION_STRING=Host=container-service-name;Port=db-port;Database=db-name;Username=your-username;Password=your-password
POSTGRES_USER=your-username
POSTGRES_PASSWORD=your-password
POSTGRES_DB=db-name
JWT_KEY=your-jwt-key
JWT_ISSUER=your-jwt-issuer
JWT_AUDIENCE=-your-jwt-audience
JWT_ACCESSTOKENEXPIRATIONMINUTES=99
JWT_REFRESHTOKENEXPIRATIONDAYS=99
VITE_APP_API_URL=url-to-your-api
VITE_APP_PROXY_API_TARGET=api-container-service-name-and-port-if-using-vite-proxy
MINIO_CONSOLE_URL=https://console.your-domain.me
MINIO_BUCKETNAME=bucket-name-for-storing-images-that-will-need-to-be-created-in-minio-console
MINIO_PRESIGNEDURLEXPIRATIONHOURS=99
MINIO_SERVERURL=http://host.docker.internal:9000
MINIO_ACCESSKEY=admin-username
MINIO_SECRETKEY=admin-password
```

2. To launch up, run execute the following command:

```sh
docker compose --env-file .env up -d
```

3. To get image downloads/uploads to work, you will need to go to `localhost:[minio-port]` and create an image bucket with the name you've chosen in `.env`

### Planned improvements

- Implement better BE validation
- Increase test coverage

## Contacts

Tauras Narvilas
[LinkedIn](https://www.linkedin.com/in/tauras-narvilas/)

[^1]: I would not recommend running your DBVS in a container. For more context, please refer to this [blog post](https://pigsty.io/blog/db/pg-in-docker/)
[^2]: Production project is running on Ubuntu Linux while I personally use Orbstack on MacOS

[deployed-app-shield]: https://img.shields.io/badge/-Deployed_App-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[deployed-app-url]: https://automotive-marketplace.taurasbear.me
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/tauras-narvilas/
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[TypeScript]: https://img.shields.io/badge/TypeScript-20232A?style=for-the-badge&logo=typescript&logoColor=3178C6
[TypeScript-url]: https://www.typescriptlang.org/
[ASP.NET Core]: https://img.shields.io/badge/ASP.NET_Core-20232A?style=for-the-badge&logo=.net&logoColor=512BD4
[ASP.NET-Core-url]: https://dotnet.microsoft.com/en-us/apps/aspnet
[PostgreSQL]: https://img.shields.io/badge/PostgreSQL-20232A?style=for-the-badge&logo=postgresql&logoColor=3178C6
[PostgreSQL-url]: https://www.postgresql.org/
[Docker]: https://img.shields.io/badge/Docker-20232A?style=for-the-badge&logo=docker&logoColor=2496ED
[Docker-url]: https://www.docker.com/
[tailwindcss]: https://img.shields.io/badge/tailwind-20232A?style=for-the-badge&logo=tailwindcss&logoColor=06B6D4
[Tailwind-url]: https://tailwindcss.com/
[Vite]: https://img.shields.io/badge/Vite-20232A?style=for-the-badge&logo=vite&logoColor=646CFF
[vite-url]: https://vite.dev/
