
<p align="center">
  <h1 align="center">
    <img src="https://github.com/user-attachments/assets/0a5286e0-dd10-4f93-a449-328910609bff" width="50%" alt="ServerPulse-logo">
  </h1>
</p>

<p align="center">
	<img src="https://img.shields.io/github/issues-pr-closed/TEGTO/ServerPulse" alt="pull-requests-amount">
	<img src="https://img.shields.io/github/last-commit/TEGTO/ServerPulse?style=flat&logo=git&logoColor=white&color=0080ff" alt="last-commit">
	<img src="https://img.shields.io/github/languages/top/TEGTO/ServerPulse?style=flat&color=0080ff" alt="repo-top-language">
	<img src="https://img.shields.io/github/languages/count/TEGTO/ServerPulse?style=flat&color=0080ff" alt="repo-language-count">
	<img src="https://img.shields.io/sonar/coverage/TEGTO_ServerPulse?server=https%3A%2F%2Fsonarcloud.io" alt="sonar-cloud-codecoverage">
	<img src="https://sonarcloud.io/api/project_badges/measure?project=TEGTO_ServerPulse&metric=ncloc" alt="sonar-cloud-lineofcode">
</p>
<p align="center">
		<em>Built with the tools and technologies:</em>
</p>
<p align="center">
	<img src="https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff" alt=".NET">
	<img src="https://img.shields.io/badge/Angular-%23DD0031.svg?logo=angular&logoColor=white" alt="Angular">
	<img src="https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white" alt="C#">
	<img src="https://img.shields.io/badge/TypeScript-3178C6.svg?style=flat&logo=TypeScript&logoColor=white" alt="TypeScript">
	<img src="https://img.shields.io/badge/JavaScript-F7DF1E.svg?style=flat&logo=JavaScript&logoColor=black" alt="JavaScript">
	<img src="https://img.shields.io/badge/HTML5-E34F26.svg?style=flat&logo=HTML5&logoColor=white" alt="HTML5">
 	<img src="https://img.shields.io/badge/Sass-C69?logo=sass&logoColor=fff" alt="SASS">
	<img src="https://img.shields.io/badge/Postgres-%23316192.svg?logo=postgresql&logoColor=white" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Apache_Kafka-231F20?logo=apache-kafka&logoColor=white" alt="Apache Kafka">
  <img src="https://img.shields.io/badge/redis-CC0000.svg?&logo=redis&logoColor=white" alt="Redis">
	<br>
	<img src="https://img.shields.io/badge/Tailwind%20CSS-%2338B2AC.svg?logo=tailwind-css&logoColor=white" alt="Tailwind">
	<img src="https://custom-icon-badges.demolab.com/badge/Microsoft%20Azure-0089D6?logo=msazure&logoColor=white" alt="Azure">
  <img src="https://img.shields.io/badge/Google_Cloud-4285F4?logo=google-cloud&logoColor=white" alt="Google Cloud">
	<img src="https://img.shields.io/badge/Docker-2496ED.svg?style=flat&logo=Docker&logoColor=white" alt="Docker">
	<img src="https://img.shields.io/badge/Kubernetes-326CE5?logo=kubernetes&logoColor=fff" alt="Kubernetes">
	<img src="https://img.shields.io/badge/ESLint-4B32C3.svg?style=flat&logo=ESLint&logoColor=white" alt="ESLint">
 	<img src="https://img.shields.io/badge/SonarCloud-F3702A?logo=sonarcloud&logoColor=fff" alt="ESLint">
	<img src="https://img.shields.io/badge/GitHub%20Actions-2088FF.svg?style=flat&logo=GitHub-Actions&logoColor=white" alt="GitHub%20Actions">
	<img src="https://img.shields.io/badge/JSON-000000.svg?style=flat&logo=JSON&logoColor=white" alt="JSON">
	<img src="https://img.shields.io/badge/YAML-CB171E.svg?style=flat&logo=YAML&logoColor=white" alt="YAML">
</p>

## Table of Contents
- [Description](#description)
- [Links](#links)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Getting Started](#getting-started)
- [Run Locally](#run-locally)
- [Contributors](#contributors)
- [Screenshots](#screenshots)

### Description
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

### Why Server Pulse?
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

# Getting Started
Integrate Server Pulse into your application to monitor metrics and events.
## Create a Server Slot and Retrieve the Key
To start sending metrics to Server Pulse, you first need to create a server slot and obtain the corresponding key. The key uniquely identifies your server slot and is required to connect your service to Server Pulse.

![image](https://github.com/user-attachments/assets/e4f1c683-3aa8-495a-ae07-47afdc2eb864)

![image](https://github.com/user-attachments/assets/9be696f1-b24d-4267-aa2e-714aa04a3e1b)

## Install the Library
Add the [Server Pulse](https://www.nuget.org/packages/TEGTO.ServerPulse.Client) NuGet package to your project:
```
<PackageReference Include="TEGTO.ServerPulse.Client" Version="1.0.0" />
```
Alternatively, you can install it using the .NET CLI:
```
dotnet add package TEGTO.ServerPulse.Client --version 1.0.0
```

## Configure Server Pulse
### Setup Configuration Settings
Update your application settings to include the EventServer URL and Key:
```
var settings = new SendingSettings
{
    EventServer = builder.Configuration["ServerPulse:EventServer"]!, // Server Pulse API Gateway URL
    Key = builder.Configuration["ServerPulse:Key"]!, // Server Slot Key
};

builder.Services.AddServerPulseClient(settings);
```
> EventServer: The Server Pulse API Gateway URL, e.g., https://localhost:7129.

> Key: Obtain the key from the Server Pulse frontend or the Server Slot API.

### Add Middleware
Enable HTTP metrics collection by adding the following middleware:
```
app.UseLoadMonitor();
```
## Go to frontend to see the metrics. 
Head over to the Server Pulse frontend to monitor your metrics in real time, thanks to SignalR's live updates.

For additional insights, navigate to the Info Page, where you can access detailed metrics and analytics.

![image](https://github.com/user-attachments/assets/40b102ca-2432-4f24-a21d-66152a969b60)

![image](https://github.com/user-attachments/assets/ecf25246-3b1a-4823-bac6-d7e10a13e0e5)

## Advanced Configuration (Optional)
You can override the default sending settings:
```
   public class SendingSettings
{
    /// The Server Slot key. Required to link metrics to Server Pulse.
    public required string Key { get; set; }

    /// The Server Pulse API Gateway URL or Server Monitor API URL for local setups.
    public required string EventServer { get; set; } = default!;

    /// The frequency (in seconds) for sending pulse events. 
    /// Backend default is one message every 3 seconds.
    public double ServerKeepAliveInterval { get; set; } = 10d;

    /// Maximum number of events per message (excluding pulse events). 
    /// Backend default is 99 messages.
    public int MaxEventSendingAmount { get; set; } = 10;

    /// Frequency (in seconds) for sending event data messages (excluding pulse events). 
    /// Backend default is one message every 3 seconds.
    public double SendingInterval { get; set; } = 15d;
}
```

## Run Locally

### Kubernetes & Minikube Setup
#### Prerequisites
Before you begin, ensure you have the following installed on your machine:
1. **Docker**: For building and running containers. [Install Docker](https://docs.docker.com/get-docker/).
2. **Minikube**: A lightweight Kubernetes implementation for local testing. [Install Minikube](https://minikube.sigs.k8s.io/docs/start/).
3. **kubectl**: Kubernetes command-line tool to manage clusters. [Install kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/).
   
----

**1. Clone the repository**:
```
git clone https://github.com/TEGTO/ServerPulse.git
```

**2. Navigate into the Kubernetes folder**:
```
cd ServerPulse/k8/dev
```

**3. Start Minikube**:
   
Open a terminal in the folder and start Minikube:
```
minikube start
```

If Minikube is already running, ensure you’re in the correct context:
```
kubectl config use-context minikube
```

**4. (Optional) Enable OAuth or Email Confirmation Features:**

- Open the `backend-conf.yml` file.
- Set the `FeatureManagement__X` environment feature variable to true.
- Provide your keys in the secret for a feature.

---

#### Deployment Steps
Follow these steps in order:

**1. Create the Namespace:**
```
kubectl create namespace server-pulse
```

**2. Configure ConfigMaps and Secrets:**
```
kubectl apply -f backend-conf.yml
kubectl apply -f infrastructure-conf.yml
```

**3. Deploy the Infrastructure:**
Deploy the database, redis and kafka and wait for them to be fully initialized:
```
kubectl apply -f kafka.yml
kubectl apply -f infrastructure.yml
kubectl get pods # Verify that the pods are running.
```

**4. Deploy the Backend:**
Deploy the backend services:
```
kubectl apply -f backend.yml
```

**5. (Optional) Swagger Open API:**
Expose and forward the apigateway service using Minikube:
```
minikube service --namespace=server-pulse apigateway
```
Go to the swagger documentation page:
```
<exposed-apigateway-url>/swagger/index.html
```

**6. Deploy the Frontend**
Deploy the frontend application:
```
kubectl apply -f frontend.yml
```

**7. Access the Frontend**
Expose and forward the frontend service using Minikube:
```
minikube service --namespace=server-pulse frontend
```

This command will expose and open the frontend in your default web browser or go manually to the exposed url.

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