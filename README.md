# VidSync - Backend Services

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Build](https://github.com/EnesEfeTokta/VidSync-Backend/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/EnesEfeTokta/VidSync-Backend/actions/workflows/dotnet-ci.yml)

**VidSync** is a real-time communication platform designed for seamless 1-on-1 video/audio calls, chat, screen sharing, and more, directly in the browser. This repository contains the backend services powering the application, built with .NET, ASP.NET Core, and SignalR, following Clean Architecture principles.

The backend is responsible for user authentication, room management, and signaling for WebRTC peer-to-peer connections.

## 🌐 The VidSync Ecosystem

VidSync is a modular project composed of multiple repositories working together:

-   **➡️ VidSync-Backend (You are here):** The core API, real-time signaling server, and database logic.
-   [**VidSync-Frontend**](https://github.com/EnesEfeTokta/VidSync-Frontend): The client-side application built with a modern web framework for users to interact with.
-   [**VidSync-AI**](https://github.com/EnesEfeTokta/VidSync-AI): A separate service providing AI-powered features for the platform.

## ✨ Features (Roadmap)

-   ✅ **User Authentication:** Secure user registration and login using JWT.
-   ✅ **Room Management:** Create and join private communication rooms.
-   ✅ **WebRTC Signaling:** Real-time signaling server using SignalR to facilitate peer connections.
-   🔄 **1-on-1 Video/Audio Calls:** Direct, low-latency P2P media streams.
-   🔄 **In-Room Chat:** Real-time text messaging during calls.
-   🔄 **Screen Sharing:** Share your screen with the other participant.
-   🔄 **Email Integration:** User verification and notification emails via SMTP.
-   💡 **AI-Powered Features:** Integration with a dedicated AI service.
-   💡 **Virtual Whiteboard:** Collaborate with a shared drawing canvas.

## 🏛️ Architecture

The backend follows the principles of **Clean Architecture** to ensure a separation of concerns, maintainability, and testability. The solution is structured into the following layers:

-   **`Domain`:** Contains the core business logic, entities, and interfaces. It has no external dependencies.
-   **`Infrastructure`:** Implements the interfaces defined in the Domain layer. This layer handles data access (using Entity Framework Core), and other external services.
-   **`Core`:** The application's entry points. This layer contains the ASP.NET Core projects:
    -   **`VidSync.API`:** Manages RESTful endpoints for authentication, user management, and room creation.
    -   **`VidSync.Signaling`:** Manages real-time communication via a SignalR Hub for WebRTC signaling, chat, and whiteboard synchronization. (Note: Can be merged into the API or kept separate).

## 🛠️ Tech Stack

-   **.NET 9**
-   **ASP.NET Core** (for Web API & SignalR)
-   **Entity Framework Core**
-   **PostgreSQL** (as the primary database)
-   **SignalR** (for real-time WebSocket communication)
-   **JWT** (for authentication)
-   **Redis** (for SignalR backplane and caching)
-   **Docker** & **Docker Compose** (for containerization and local development)
-   **Cloudflare Tunnel** (for easy and secure exposing of local services)

## 🚀 Getting Started

Follow these instructions to get the backend services up and running on your local machine for development and testing.

### Prerequisites

-   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or the version specified in `global.json`)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installation & Running Locally

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/EnesEfeTokta/VidSync-Backend.git
    cd VidSync-Backend
    ```

2.  **Set Up Environment Variables:**
    The project uses an `.env` file to manage secrets and environment-specific configuration.
    -   Copy the example file:
        ```bash
        # On Windows (Command Prompt)
        copy .env.example .env
        
        # On macOS/Linux
        cp .env.example .env
        ```
    -   Open the newly created `.env` file and fill in the placeholder values. You must provide your own database password, JWT secret, etc.

3.  **Generate a Local HTTPS Certificate:**
    The `docker-compose` setup is configured to use a local HTTPS certificate for the API service.
    -   Run the following .NET CLI command to generate a certificate:
        ```bash
        dotnet dev-certs https -ep ./src/Core/VidSync.API/localhost.pfx -p YOUR_STRONG_PASSWORD_HERE
        ```
    -   Make sure the password you use here matches the `ASPNETCORE_HTTPS_PWD` variable in your `.env` file. The command above places the certificate in the correct location (`src/Core/VidSync.API/`) with the default name `localhost.pfx`. If you change the name, update the `docker-compose.yml` file accordingly.

        ***Note:** You will also need to copy this certificate to your React frontend project, otherwise HTTPS requests will fail.*

4.  **Launch the Services with Docker Compose:**
    This is the recommended way to run the entire backend stack, including the database and cache.
    ```bash
    docker-compose up --build
    ```
    This command will:
    -   Build the required Docker images for the API.
    -   Start the PostgreSQL database container.
    -   Start the Redis container.
    -   Build and start the `VidSync.API` service.
    -   (Optional) Start the `cloudflared` service if you've provided a token.

5.  **Apply Database Migrations:**
    Once the containers are running (especially the `db` container), you need to apply the initial database schema.
    -   Open a **new terminal window** in the repository root.
    -   Run the Entity Framework Core update command:
        ```bash
        dotnet ef database update --project src/Infrastructure/VidSync.Persistence --startup-project src/Core/VidSync.API
        ```
    *Note: This command connects to the PostgreSQL database running inside Docker, so the `docker-compose up` command must be active.*

6.  **You are ready to go!**
    The backend is now running and available at:
    -   **API (HTTP):** `http://localhost:5123`
    -   **API (HTTPS):** `https://localhost:7123`

### Accessing API Documentation

The API includes Swagger/OpenAPI documentation for easy testing. Once the services are running, navigate to:
-   [**https://localhost:7123/swagger**](https://localhost:7123/swagger)

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

Please refer to the `CONTRIBUTING.md` file for details on our code of conduct and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

I've also created the content for the `.env.example` file you should add to your repository.

**File: `.env.example`**
```dotenv
# --- Database Configuration ---
# Used by the Postgres container to initialize and by the API to connect.
DB_USER=postgres
DB_PASSWORD=YOUR_STRONG_DATABASE_PASSWORD
DB_NAME=vidsync_db

# --- ASP.NET Core ---
# Password for the local HTTPS certificate (.pfx file).
# Generate the cert with: dotnet dev-certs https -ep ./src/Core/VidSync.API/localhost.pfx -p YOUR_PASSWORD
ASPNETCORE_HTTPS_PWD=YOUR_STRONG_CERTIFICATE_PASSWORD

# --- JWT Configuration ---
# A long, random, and secret string for signing JWT tokens.
JWT__Key=REPLACE_WITH_A_VERY_LONG_AND_SECRET_RANDOM_STRING_FOR_JWT
JWT__Issuer=https://localhost:7123
JWT__Audience=https://localhost:7123

# --- Cloudflare Tunnel (Optional) ---
# If you want to expose your local instance to the internet via Cloudflare Tunnel,
# paste your tunnel token here. Otherwise, you can leave it blank or comment out the 'cloudflared' service in docker-compose.yml.
CLOUDFLARE_TUNNEL_TOKEN=YOUR_CLOUDFLARE_TUNNEL_TOKEN_HERE