
## README

### Overview

This project sets up a Kubernetes environment with a microservice that communicates with RabbitMQ and exposes a RESTful API. The RabbitMQ management UI is also exposed for monitoring purposes.

### Prerequisites

- Kubernetes cluster
- `kubectl` configured to interact with your cluster
- Docker to perform docker build command

### Build
To build image with sample 
1. cd ./DotNetSample
2. Run command
   ```sh
   docker build -t dotnetsample:1.0.0 .
   ```
### Deployment

1. **Create Namespace, Services, and Deployments**

   Apply the the YAML configuration from yaml folder to your Kubernetes cluster:

   ```sh
   kubectl apply -f ./yaml
   ```

2. **Access RabbitMQ Management UI**

   To access the RabbitMQ management UI, you can use port forwarding:

   ```sh
   kubectl port-forward service/rabbitmq 15672:15672 -n microservice-namespace
   ```

   Then, open your browser and navigate to `http://localhost:15672`. Use the default credentials (`guest`/`guest`) to log in.

3. **Access the Microservice**

   To access the microservice, you can use port forwarding:

   ```sh
   kubectl port-forward service/microservice 8080:8080 -n microservice-namespace
   ```

   Then, open your browser and navigate to `http://localhost:8080`.

### Environment Variables

- `RABBITMQ_HOST`: The hostname of the RabbitMQ service.
- `RABBITMQ_PORT`: The port of the RabbitMQ service.

### Notes

- Ensure your Kubernetes cluster has sufficient resources to run the deployments.
- Modify the `replicas` field in the deployment configurations to scale the services as needed.

### Cleanup

To delete the resources created by this deployment, run:

```sh
kubectl delete -f ./yaml
```

### License

This project is licensed under the MIT License. See the LICENSE file for details.

---

This README provides an overview of the project, deployment instructions, and additional information to help you get started.