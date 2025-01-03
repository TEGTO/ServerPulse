## Description
Server Pulse streamlines the monitoring and visualization of system performance, enabling real-time API metrics collection and display. Designed with simplicity and flexibility in mind, it offers a straightforward alternative to traditional monitoring tools like Prometheus and Grafana, without simple functionality. Server Pulse empowers users with a low-effort setup and intuitive dashboard, ideal for single developers.

### Planned Use Cases
1. **API Monitoring**: Track API performance, latency, and error rates.
2. **Real-Time Insights**: Provide actionable metrics through an interactive dashboard.
3. **Microservice Coordination**: Simplify operations in complex, distributed systems.
4. **Quick Deployment**: Offer an out-of-the-box monitoring solution with minimal configuration.

### How It Works
1. **Data Collection**: Metrics are collected from APIs via middleware or agents.
2. **Processing**: Metrics are processed in the backend using Kafka for event-driven architecture and Redis for caching.
3. **Service Discovery**: Consul manages service registrations and discovery within the microservices architecture.
4. **Security**: JWT authentication ensures secure access to API and frontend.

### Why Server Pulse
Server Pulse offers a lightweight, user-friendly alternative to traditional solutions like Prometheus:
- **Ease of Setup**: A low-friction onboarding process with minimal configuration.
- **Modern Frontend**: Angular-powered UI for seamless navigation and real-time insights.
- **Integrated Architecture**: Microservices-ready, with tools like Kafka, Redis, and SignalR for robust performance.
- **All-in-One Solution**: Combines monitoring, visualization, and API security in one platform.

## Links
- **Client nuget package**: [Nuget Package](https://www.nuget.org/packages/TEGTO.ServerPulse.Client)  
  Get started with client SDK.
- **Main Documentation**: [Main Docs](https://mango-plant-0330b6603.4.azurestaticapps.net)  
  Main documentation generated with DocFX.
- **Frontend Documentation**: [Frontend Docs](https://thankful-grass-038e4f603.4.azurestaticapps.net)  
  Main documentation generated with DocFX.
- **GitHub**: [Project](https://github.com/TEGTO/ServerPulse)  
  Project source code.

## Tech Stack
- **Frontend:** Angular with NgRx for state management.
- **Backend:** ASP.NET Core Web API.
- **Database:** PostgreSQL with Entity Framework Core.
- **Caching:** Redis for distributed caching.
- **Messaging:** Apache Kafka for event-driven messaging.
- **Authentication:** JWT with Azure Communication Services for email confirmation and OAuth 2.0.
- **API Gateway:** Ocelot for microservice routing.
- **Resilience:** Polly for retry policies and fault tolerance.
- **Containerization:** Docker and Kubernetes for deployment.
- **CI/CD:** GitHub Actions for automated workflows.
- **Testing:** NUnit and Jasmine for unit testing; Testcontainers for integration testing.
- **Cloud:** Azure for cloud architecture.
- **Documentation:** Swagger, DocFX, and Postman for comprehensive documentation.
- **Code Quality:** SonarCloud for static code analysis.
 
## Features
- **Metric Collection:** Gather API metrics efficiently and transmit them to the server.
- **Data Visualization:** Displays metrics in an intuitive, user-friendly dashboard.
- **Microservices Architecture:** Modular and scalable architecture design.
- **Real-Time Updates:** Powered by SignalR for live metric updates.
- **Event Processing:** Integrates with Apache Kafka for distributed messaging.
- **Secure Access:** Implements robust JWT authentication, email confirmation, and OAuth 2.0.
- **Scalable Deployment:** Uses Docker containers and Kubernetes orchestration.
- **API Gateway:** Employs Ocelot for seamless service endpoint routing and protection.

## Contributors
<a href="https://github.com/TEGTO/ServerPulse/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=TEGTO/ServerPulse"/>
</a>

## Screenshots 
![image](https://github.com/user-attachments/assets/554449c4-72b6-4d63-9afc-d6dba98a128c)

----

![image](https://github.com/user-attachments/assets/bb9136c8-586f-4dfd-ad72-c4ff0535fabb)

----

![image](https://github.com/user-attachments/assets/277fdf68-b74a-4401-9df3-7f183aec7199)

----

![image](https://github.com/user-attachments/assets/344447a1-a570-43ac-a365-9f766852b3d5)