# MicroServicesProject

MicroServicesProject is a reference implementation of a containerized microservices architecture built with C# (.NET 8) exposing both HTTP and gRPC endpoints, backed by Kafka for messaging, and instrumented for observability with OpenTelemetry, Prometheus, and Grafana. It includes load- and stress-testing scripts using k6 for both HTTP and gRPC workloads.

---

## Overview

MicroServicesProject demonstrates a simple yet complete microservices setup using .NET, Kafka messaging, and modern observability practices. It provides:

- An **HTTP API** (`APIService`) exposing REST endpoints with Swagger support.
    
- A **gRPC service** (`GRPCService`) offering an RPC endpoint for summing two numbers, with an outbox pattern implementation and SQLite persistence.
    
- Kafka-based asynchronous messaging between services, leveraging MassTransit and Confluent.Kafka.
    
- Container orchestration via **Docker Compose**, orchestrating Zookeeper, Kafka, both services, OpenTelemetry Collector, Prometheus, Grafana, and a Prometheus Node Exporter.
    

---

## Architecture

The solution follows a decoupled, message-driven architecture:

1. **APIService** receives HTTP requests, publishes messages to a Kafka topic, and returns immediate acknowledgments.
    
2. **GRPCService** consumes those messages, processes them (e.g., adds numbers), persists results in SQLite using EF Core migrations, and optionally emits further events.
    
3. **Kafka Cluster** (Zookeeper + Kafka broker) acts as a durable message bus between services.
    
4. **OpenTelemetry Collector** captures metrics via OTLP and forwards them to Prometheus.
    
5. **Prometheus** scrapes metrics from the Collector and Node Exporter, visualized in **Grafana**.
    

---

## Services

### APIService

- **Tech Stack:** ASP.NET Core Web API, C#, MassTransit, OpenTelemetry
    
- **Port:** `5270`
    
- **Features:**
    
    - REST endpoint `GET /api/Agent` returning a health check/status.
        
    - Swagger UI for API exploration in Development mode.
        
    - Publishes `NumberMessage` to Kafka topic `number`.
        

### GRPCService

- **Tech Stack:** ASP.NET Core gRPC, C#, EF Core (SQLite), MassTransit, OpenTelemetry
    
- **Port:** `7168` (HTTP/2)
    
- **Features:**
    
    - gRPC service `SumService/AddNumbers` defined in `sum.proto`.
        
    - Applies EF Core migrations on startup to maintain `outbox.db`.
        
    - Produces metrics via OTLP exporter to the OpenTelemetry Collector.
        

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
    
- [Docker & Docker Compose (v3.8+)](https://docs.docker.com/compose/)
    
- [k6 CLI](https://k6.io/) (for load/stress tests)
    

---

## Installation & Running

1. **Clone the repository**
    
    ```bash
    git clone https://github.com/youssef-hilaly/MicroServicesProject.git
    cd MicroServicesProject
    ```
    
2. **Build & start all containers**
    
    ```bash
    docker-compose up --build
    ```
    
    This spins up Zookeeper, Kafka, both services, OTEL Collector, Prometheus, Grafana, and Node Exporter.
    
3. **Access Services**
    
    - APIService Swagger UI: [http://localhost:5270/swagger/index.html](http://localhost:5270/swagger/index.html)
        
    - gRPC endpoint (via a gRPC client) at `localhost:7168`
        
    - Prometheus dashboard: [http://localhost:9090](http://localhost:9090/)
        
    - Grafana dashboard: [http://localhost:3000](http://localhost:3000/)
        

---

## Load & Stress Testing

Use the k6 scripts in the `K6/` folder to validate performance:

- **Load Test** (`load_test.js`):
    
    - 50 constant VUs for HTTP over 60s
        
    - 30 constant VUs for gRPC over 45s
        
    
    ```bash
    k6 run K6/load_test.js
    ```
    
- **Stress Test** (`stress_test.js`):
    
    - Ramping-VUs up to 200 for HTTP
        
    - Ramping-VUs up to 100 for gRPC
        
    
    ```bash
    k6 run K6/stress_test.js
    ```
    

---

## Monitoring & Observability

- **OpenTelemetry Collector** configured via `config/otel-collector-config.yml` to receive OTLP metrics and export to Prometheus.
    
- **Prometheus** scrapes its own metrics, Node Exporter, and the Collector as per `config/prometheus.yml`.
    
- **Grafana** preconfigured on `3000` for dashboards (add Prometheus as a data source manually).
    

---

## Configuration

- **OTEL Endpoint:** set via environment variable `Otel__Endpoint` in `docker-compose.yml` for both services.
    
- **Kafka Settings:** configured in MassTransit setup in each service’s `Program.cs`.
    
- **EF Core Connection:** `DefaultConnection` using SQLite in `appsettings.json` for GRPCService.
    

---

## Project Structure

```
.
├── APIService/               # HTTP API Service
├── GRPCService/              # gRPC Service
├── K6/                       # k6 load & stress test scripts
├── config/                   # OTEL & Prometheus configs
├── docker-compose.yml        # Entire stack composition
└── README.md
```

---

_Happy microservicing!_
