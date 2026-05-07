# Docker & Kubernetes Guide

## Development with Docker Compose

### Prerequisites
- Docker Desktop installed and running
- Docker Compose v2+

### Run the full stack locally

```bash
docker-compose up -d
```

This starts:
- **PostgreSQL 16** on `localhost:5432` (credentials: postgres/postgres)
- **Feedy API** on `localhost:5000`

### View logs

```bash
docker-compose logs -f api
docker-compose logs -f postgres
```

### Stop the stack

```bash
docker-compose down
```

### Rebuild the image after code changes

```bash
docker-compose up -d --build api
```

## Kubernetes Deployment

### Prerequisites
- kubectl configured to point to your cluster
- Docker image pushed to registry (e.g., Docker Hub, ECR, GCR)

### 1. Create secrets

Replace values with your actual credentials:

```bash
kubectl create secret generic feedy-db-secret \
  --from-literal=connection-string='Host=feedy-postgres;Port=5432;Database=feedy_prod;Username=postgres;Password=YOUR_PASSWORD' \
  --from-literal=postgres-password='YOUR_PASSWORD'

kubectl create secret generic feedy-jwt-secret \
  --from-literal=secret='your-super-secret-jwt-key-min-32-chars'
```

### 2. Deploy PostgreSQL

```bash
kubectl apply -f k8s/postgres-statefulset.yaml
```

Wait for the pod to be ready:

```bash
kubectl wait --for=condition=ready pod -l app=feedy-postgres --timeout=300s
```

### 3. Deploy the API

```bash
kubectl apply -f k8s/deployment.yaml
```

Check deployment status:

```bash
kubectl get deployments
kubectl get pods -l app=feedy-api
```

### 4. Access the API

Get the external IP:

```bash
kubectl get svc feedy-api-service
```

The API will be available at `http://<EXTERNAL-IP>`

### Verify health

```bash
curl http://<EXTERNAL-IP>/health
```

### View logs

```bash
kubectl logs -l app=feedy-api -f
```

### Clean up

```bash
kubectl delete -f k8s/
kubectl delete secrets feedy-db-secret feedy-jwt-secret
```

## Image Configuration

### Build image manually

```bash
docker build -t feedy-api:latest .
```

### Push to registry

```bash
docker tag feedy-api:latest YOUR_REGISTRY/feedy-api:latest
docker push YOUR_REGISTRY/feedy-api:latest
```

Then update `k8s/deployment.yaml` to use `YOUR_REGISTRY/feedy-api:latest`.

## Environment Variables

Refer to `appsettings.json` for all available configuration options. All environment variables are prefixed with corresponding JSON paths:

- `ConnectionStrings__DefaultConnection` — database connection string
- `JwtSettings__Secret` — JWT signing key
- `JwtSettings__Issuer` — JWT issuer
- `JwtSettings__Audience` — JWT audience
- `ASPNETCORE_ENVIRONMENT` — `Development` or `Production`
