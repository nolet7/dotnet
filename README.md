# dotnet-microshop (Microshop) — End-to-End Runbook

This repo contains a .NET microservices demo:

User → Frontend (optional) → API Gateway (YARP) → OrderService → (InventoryService, PaymentService)

It also includes:
- Kubernetes manifests + Ingress
- Helm chart
- Argo CD GitOps app
- cert-manager TLS
- Prometheus/Grafana observability
- Chaos Mesh + k6 load test
- Dev/Prod environment folders

---

## 0) Prerequisites

### Required (Local Docker)
- Docker + Docker Compose (v2)
- curl
- Git

### Required (Kubernetes path)
- kubectl
- Helm
- NGINX Ingress Controller installed (if using Ingress)
- Optional: Argo CD
- Optional: cert-manager
- Optional: kube-prometheus-stack

---

## 1) Repo layout (high level)

- `frontend/` (Blazor UI)
- `api-gateway/ApiGateway/` (YARP gateway)
- `order-service/OrderService/`
- `inventory-service/InventoryService/`
- `payment-service/PaymentService/`
- `docker-compose.yml`
- `k8s/` (raw manifests)
- `helm/microshop/` (Helm chart)
- `argocd-app.yaml` (Argo CD Application)
- `chaos/` (Chaos Mesh + k6)
- `slo/` (Prometheus rules + Grafana dashboard)
- `envs/` (dev/prod namespaces + values)
- `docs/` (architecture + runbook + resume bullets)

---

## 2) Start the application (Docker Compose)

### 2.1 Stop anything occupying ports (if needed)
If you previously ran `dotnet run` locally, kill the listeners:
```bash
sudo fuser -k 5000/tcp 5001/tcp 5002/tcp 8080/tcp 2>/dev/null || true
````

### 2.2 Start

From repo root:

```bash
cd ~/dotnet-microshop
docker compose down
docker compose up -d
```

### 2.3 Verify containers are up

```bash
docker ps
```

### 2.4 Test the gateway end-to-end (must return 200)

```bash
curl -v -X POST http://localhost:8080/orders
```

Expected:

```text
HTTP/1.1 200 OK
"Order created successfully"
```

### 2.5 Test services directly (optional)

```bash
curl -v -X POST http://localhost:5000/orders
curl -v -X POST http://localhost:5001/check
curl -v -X POST http://localhost:5002/pay
```

---

## 3) Build & Push images to Docker Hub (manual)

> Replace `noletengine` if your DockerHub username is different.

```bash
cd ~/dotnet-microshop

docker build -t noletengine/orderservice:latest ./order-service/OrderService
docker push noletengine/orderservice:latest

docker build -t noletengine/inventoryservice:latest ./inventory-service/InventoryService
docker push noletengine/inventoryservice:latest

docker build -t noletengine/paymentservice:latest ./payment-service/PaymentService
docker push noletengine/paymentservice:latest

docker build -t noletengine/apigateway:latest ./api-gateway/ApiGateway
docker push noletengine/apigateway:latest
```

---

## 4) Kubernetes (raw manifests)

### 4.1 Apply everything

```bash
kubectl apply -f k8s/
```

### 4.2 Check status

```bash
kubectl get pods
kubectl get svc
kubectl get ingress
```

### 4.3 Test via ingress (example host)

If your Ingress uses `microshop.local`, add to `/etc/hosts` pointing to your ingress IP:

```bash
kubectl get ingress
```

Then:

```bash
curl -v -X POST http://microshop.local/orders
```

---

## 5) Helm (install all services)

### 5.1 Install

```bash
helm install microshop helm/microshop
```

### 5.2 Upgrade after changes

```bash
helm upgrade microshop helm/microshop
```

### 5.3 Uninstall

```bash
helm uninstall microshop
```

---

## 6) Multi-environment (dev/prod)

### 6.1 Create namespaces

```bash
kubectl apply -f envs/dev/namespace.yaml
kubectl apply -f envs/prod/namespace.yaml
```

> If you want Helm per env, install into the namespace:

```bash
helm install microshop-dev helm/microshop -n microshop-dev --create-namespace
helm install microshop-prod helm/microshop -n microshop-prod --create-namespace
```

---

## 7) Argo CD GitOps

### 7.1 Install Argo CD

```bash
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

### 7.2 Access Argo CD UI locally

```bash
kubectl port-forward svc/argocd-server -n argocd 8081:443
```

### 7.3 Deploy the GitOps application

Make sure `argocd-app.yaml` points to the right repoURL/path.

```bash
kubectl apply -f argocd-app.yaml
```

---

## 8) TLS with cert-manager

### 8.1 Install cert-manager

```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/latest/download/cert-manager.yaml
```

### 8.2 Create ClusterIssuer (edit email + domain)

```bash
kubectl apply -f cluster-issuer.yaml
```

### 8.3 Apply TLS Ingress

```bash
kubectl apply -f tls-ingress.yaml
```

Check:

```bash
kubectl get certificate
kubectl describe certificate microshop-tls 2>/dev/null || true
kubectl get secret microshop-tls
```

---

## 9) Observability (Prometheus + Grafana)

### 9.1 Install kube-prometheus-stack

```bash
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm install monitoring prometheus-community/kube-prometheus-stack
```

### 9.2 Access Grafana

```bash
kubectl port-forward svc/monitoring-grafana 3000:80
```

Login (default chart values):

* user: `admin`
* password: `prom-operator`

---

## 10) Chaos Engineering (Chaos Mesh + k6)

### 10.1 Chaos Mesh install (quick)

> If you already run Chaos Mesh, skip this.
> Follow your cluster’s Chaos Mesh install method; once installed:

Apply chaos experiments:

```bash
kubectl apply -f chaos/chaos-mesh/pod-kill-orderservice.yaml
kubectl apply -f chaos/chaos-mesh/network-delay.yaml
```

Delete chaos experiments:

```bash
kubectl delete -f chaos/chaos-mesh/pod-kill-orderservice.yaml
kubectl delete -f chaos/chaos-mesh/network-delay.yaml
```

### 10.2 Run k6 load test (Docker)

```bash
docker run --rm --network host -i grafana/k6 run - < chaos/k6/orders-load.js
```

> If your gateway is not on localhost, update the URL in `chaos/k6/orders-load.js`.

---

## 11) SLO / Error Budget files

Prometheus rules live in:

* `slo/prometheus/slo-rules.yaml`
* `slo/prometheus/error-budget.yaml`

Grafana dashboard JSON:

* `slo/grafana/slo-dashboard.json`

(How you import depends on your monitoring stack setup.)

---

## 12) Common troubleshooting

### A) Port already in use

```bash
sudo fuser -k 8080/tcp 5000/tcp 5001/tcp 5002/tcp 2>/dev/null || true
docker compose down
docker compose up -d
```

### B) Gateway returns 404

Check logs:

```bash
docker logs dotnet-microshop-apigateway-1 --tail=200
```

### C) Service-to-service DNS not working (Docker)

Make sure containers are on the same compose network:

```bash
docker network ls | grep dotnet-microshop
```

---

## 13) Quick “start everything” (Docker)

Copy/paste:

```bash
cd ~/dotnet-microshop
sudo fuser -k 8080/tcp 5000/tcp 5001/tcp 5002/tcp 2>/dev/null || true
docker compose down
docker compose up -d
curl -v -X POST http://localhost:8080/orders
```

---

## Maintainer Notes

* Keep containers minimal (no curl/wget inside images).
* Use disposable test containers (`curlimages/curl`) on the network when needed.

