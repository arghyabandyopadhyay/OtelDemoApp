# OtelDemoApp

This project is an ASP.NET Core Web API application that provides weather forecast data and demonstrates distributed tracing, metrics, and logging using the OpenTelemetry stack (Otel Collector, Loki, Tempo, Mimir, and Grafana) via Docker Compose.

## Project Structure

- **Program.cs**: Main entry point for the application, sets up the web server and API endpoints.
- **OtelDemoApp.csproj**: Project file with dependencies and configuration.
- **Properties/launchSettings.json**: Launch profiles for different environments.
- **Dockerfile**: Instructions for building the application Docker image.
- **docker-compose.yml**: Orchestrates the full observability stack (Otel Collector, Loki, Tempo, Mimir, Grafana, and the demo app).
- **loki-config.yaml, tempo.yaml, mimir.yaml**: Configuration files for observability services.

## Getting Started

### Prerequisites

- .NET SDK (version 6.0 or later)
- Docker & Docker Compose

### Building and Running the Application

#### 1. Clone the repository:
```sh
git clone <repository-url>
cd OtelDemoApp
```

#### 2. Build and run the full stack with Docker Compose:
```sh
docker compose up --build
```

This will start:
- **otel-demo-app** (your ASP.NET Core API)
- **otel-collector** (OpenTelemetry Collector)
- **loki** (logs)
- **tempo** (traces)
- **mimir** (metrics)
- **grafana** (visualization)

#### 3. Access the services:
- **API**: [http://localhost:5213/weatherforecast](http://localhost:5213/weatherforecast)
- **Grafana**: [http://localhost:3000](http://localhost:3000)  
  Default login: `admin` / `admin`
- **Loki**: [http://localhost:3100/ready](http://localhost:3100/ready) (API only)
- **Tempo**: [http://localhost:3200/ready](http://localhost:3200/ready) (API only)
- **Mimir**: [http://localhost:9009/prometheus](http://localhost:9009/prometheus) (API only)

#### 4. Add Data Sources in Grafana:
- **Loki**: `http://loki:3100`
- **Tempo**: `http://tempo:3200`
- **Prometheus/Mimir**: `http://mimir:9009/prometheus`

> If Grafana is running outside Docker, use `localhost` instead of the service name.

## API Endpoints

- **GET /weatherforecast**: Returns a list of weather forecasts for the next five days.

## Observability

- **Traces**: Collected via OpenTelemetry Collector and sent to Tempo.
- **Logs**: Collected and sent to Loki.
- **Metrics**: Collected and sent to Mimir (Prometheus-compatible).
- **Visualization**: All data can be explored in Grafana.

## License

This project is licensed under the MIT License.