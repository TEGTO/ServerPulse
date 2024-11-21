# Server Pulse

Project aims to streamline monitoring and visualization of system performance. The system collects, processes, and visualizes API metrics in real time. It is designed with simplicity in mind, making it easier for users to set up and maintain, without compromising on performance or flexibility.

### **Key Features**
- **Metric Collection:** Efficiently gathers API metrics and sends them to a processing server.
- **Data Visualization:** Displays metrics in an intuitive, user-friendly dashboard.
- **Microservices Architecture:** Ensures modularity and scalability.
- **Real-Time Updates:** Uses **SignalR** to provide live updates on metrics.
- **Distributed System Support:** Integrates with **Apache Kafka** for messaging and **Consul** for service discovery.
- **Secure Access:** Implements **JWT authentication** to protect data and endpoints.
- **Scalability:** Employs **Docker** for containerization and **Redis** for caching.

---

## **Tech Stack**
Server Pulse utilizes modern, robust technologies for both backend and frontend development:

### **Backend**
- **ASP.NET Core**: For building high-performance APIs and microservices.
- **Entity Framework (EF)**: For database interactions with **PostgreSQL**.
- **Apache Kafka**: For distributed messaging and data processing.
- **Redis**: For caching frequently used data.
- **Ocelot API Gateway**: For managing API requests in a microservices environment.
- **Consul**: For service discovery and configuration.

### **Frontend**
- **Angular**: For building a dynamic and interactive user interface.
- **NgRx**: For state management and ensuring predictable application behavior.

### **DevOps**
- **Docker**: For containerization, ensuring consistent environments across development and production.
- **SignalR**: For enabling real-time communication between server and client.

---

## **Planned Use Cases**
1. **API Monitoring**: Track API performance, latency, and error rates.
2. **Real-Time Insights**: Provide actionable metrics through an interactive dashboard.
3. **Microservice Coordination**: Simplify operations in complex, distributed systems.
4. **Quick Deployment**: Offer an out-of-the-box monitoring solution with minimal configuration.

---

## **How It Works**
1. **Data Collection**: Metrics are collected from APIs via middleware or agents.
2. **Processing**: Data is processed on the backend using Kafka for messaging and Redis for caching.
3. **Visualization**: Processed data is sent to the Angular-based frontend and displayed in real-time.
4. **Service Discovery**: Consul manages service registrations and discovery within the microservices architecture.
5. **Security**: JWT authentication ensures secure access to API and frontend.

---

## **Why Server Pulse?**
Unlike traditional solutions such as **Prometheus** and **Grafana**, Server Pulse offers:
- Simplicity in setup and configuration.
- A user-friendly interface built with Angular.
- Seamless integration with microservices architecture.
- A complete, all-in-one monitoring solution.

