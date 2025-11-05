# CHANGELOG

All notable changes to this project will be documented in this file.

## [Sprint 1] - 2025-09

### Added
- Implemented user login and registration functionality.
- Set up PostgreSQL database for persistent data storage.
- Developed basic API endpoints for core backend operations.

## [Sprint 2] - 2025-09

### Added
- Real-time communication features using SignalR.
- User presence tracking in video rooms.
- Enhanced error management.

## [Sprint 3] - 2025-10

### Added
- The necessary signaling mechanisms for establishing WebRTC connections via SignalR Hub were set up.
- At the same time, the foundation for the chat feature was laid by developing the database schema, API endpoints, and SignalR events for persistent and instant messaging.
- Functionality enabling users to join and leave video rooms was added.
- By the end of this sprint, the backend was able to provide all the necessary infrastructure for two clients to find each other, start a video chat, and instant message within a room.

## [Sprint 4] - 2025-10

### Added
- A robust encryption mechanism has been implemented for user messages and conversation summaries to ensure maximum data privacy.
- We laid the foundation for future reporting features by collecting detailed room and user activity metrics with Redis for real-time monitoring.
- Finally, we integrated our new Python AI service to automatically summarize completed conversations, and the results were securely stored in the database.