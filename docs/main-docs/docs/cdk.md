## Prerequisites
Before you begin, ensure you have the following installed on your machine:
1. **AWS CLI**: Required for interacting with AWS (configure credentials after installation). [Install CLI](https://aws.amazon.com/cli/).
2. **AWS CDK**: Used to deploy infrastructure as code. [Install CDK](https://aws.amazon.com/cdk/).
   
----

## Deploying with AWS CDK
**1. Clone the repository**:
```
git clone https://github.com/TEGTO/ServerPulse.git
```

**2. Navigate into the CDK folder**:
```
cd ServerPulse/infra/cdk
```

**3. Deploy the CDK Stack**:
   
Run one of the following commands inside the cdk folder:
```
cdk deploy 
```
or, if using a specific AWS profile:
```
cdk deploy --profile=<your-profile>
```

**4. (Optional) Enable OAuth or Email Confirmation Features:**

- Open the `Cdk.sln` solution (found in the infra/cdk folder).
- Set the `FeatureManagement__X` environment feature variable to true.
- Provide your keys in the secret for a feature.

----

##  Accessing the Frontend & API Gateway

**Frontend Access**

1. Go to AWS Console → Elastic Container Service (ECS).
2. In Services, find `frontend`.
3. Click on the latest **running task**.
4. Under the **Networking** tab, copy the public IP.
5. Open your browser and visit
```
http://<public-ip>:80
```

**API Gateway Access**

Follow the same steps as above, but select `apigateway` instead of `frontend`.
```
http://<public-ip>:8080
```