# VidSync - Backend Services

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Build](https://github.com/EnesEfeTokta/VidSync-Backend/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/EnesEfeTokta/VidSync-Backend/actions/workflows/dotnet-ci.yml)

**VidSync** is a real-time communication platform designed to provide seamless 1-on-1 video and audio calls directly in the browser. This repository contains the backend services powering the application, built with .NET 8, ASP.NET Core, and SignalR, following Clean Architecture principles.

The backend is responsible for user authentication, room management, and signaling for WebRTC peer-to-peer connections.

## ✨ Features (Roadmap)

-   **User Authentication:** Secure user registration and login using JWT.
-   **Room Management:** Create and join private communication rooms.
-   **WebRTC Signaling:** Real-time signaling server using SignalR to facilitate peer connections.
-   **1-on-1 Video/Audio Calls:** Direct, low-latency P2P media streams.
-   **In-Room Chat:** Real-time text messaging during calls.
-   **Screen Sharing:** Share your screen with the other participant.
-   **Virtual Whiteboard:** Collaborate with a shared drawing canvas.

## 🏛️ Architecture

The backend follows the principles of **Clean Architecture** to ensure a separation of concerns, maintainability, and testability. The solution is structured into the following layers:

-   **`Domain`:** Contains the core business logic, entities, and interfaces. It has no external dependencies.
-   **`Infrastructure`:** Implements the interfaces defined in the Domain layer. This layer handles data access (using Entity Framework Core), and other external services.
-   **`Core`:** The application's entry points. This layer contains the ASP.NET Core projects:
    -   **`VidSync.API`:** Manages RESTful endpoints for authentication, user management, and room creation.
    -   **`VidSync.Signaling`:** Manages real-time communication via a SignalR Hub for WebRTC signaling, chat, and whiteboard synchronization.

## 🛠️ Tech Stack

-   **.NET 8**
-   **ASP.NET Core** (for Web API & SignalR)
-   **Entity Framework Core 8**
-   **PostgreSQL** (as the primary database)
-   **SignalR** (for real-time WebSocket communication)
-   **JWT** (for authentication)
-   **Redis** (for SignalR backplane and caching)
-   **Docker** (for containerization and local development)

## 🚀 Getting Started

Follow these instructions to get the backend services up and running on your local machine for development and testing.

### Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Installation & Running Locally

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/EnesEfeTokta/VidSync-Backend.git
    cd VidSync-Backend
    ```

2.  **Configure Environment Variables:**
    The application uses a `docker-compose.yml` file for local development. You may need to review the environment variables within this file, such as database credentials or JWT secrets, although the defaults should work out-of-the-box.

3.  **Launch the Services with Docker Compose:**
    This is the recommended way to run the entire backend stack, including the database and cache.
    ```bash
    docker-compose up --build
    ```
    This command will:
    -   Pull or build the required Docker images.
    -   Start the PostgreSQL database container.
    -   Start the Redis container.
    -   Build and start the `VidSync.API` service (available at `http://localhost:8080`).
    -   Build and start the `VidSync.Signaling` service (available at `http://localhost:8081`).

4.  **Apply Database Migrations:**
    Once the containers are running, you need to apply the initial database schema. Open a new terminal and run:
    ```bash
    dotnet ef database update --project src/Core/VidSync.API
    ```
    *Note: This command needs to be run from the root of the repository.*

5.  **You are ready to go!**
    The backend is now running. You can interact with the API using a tool like Postman or by running the [VidSync Frontend](https://github.com/EnesEfeTokta/VidSync-Frontend) application.

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

Please refer to the `CONTRIBUTING.md` file for details on our code of conduct and the process for submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Next Steps for You:**

1.  Replace `YOUR_USERNAME` with your actual GitHub username in the links.
2.  If you have a separate frontend repo, add the link to it.
3.  As you add features, update the "Features" section to reflect the current state.
4.  Consider adding a `CONTRIBUTING.md` file if you plan to accept contributions.
5.  Create a `LICENSE` file (the MIT license is a great, permissive choice for most projects).