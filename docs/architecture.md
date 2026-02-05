# Microshop Architecture

Client
 → API Gateway (YARP)
   → Order Service
     → Inventory Service
     → Payment Service

Observability:
- Prometheus
- Grafana

Resilience:
- Chaos Mesh
- SLO / Error Budget
