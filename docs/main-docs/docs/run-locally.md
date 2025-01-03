## Prerequisites
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

## Deployment Steps
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