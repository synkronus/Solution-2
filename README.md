# 🏪 PoliMarket CBSE System

**Component-Based Software Engineering Implementation**
*Politécnico Grancolombiano - Software Design Module*

## 📋 Overview

PoliMarket is a comprehensive marketplace management system built using Component-Based Software Engineering (CBSE) principles. The system demonstrates modular architecture, loose coupling, and high reusability through independent, interchangeable components.

## 🏗️ Architecture

### CBSE Principles Applied

- **Separation of Concerns**: Each component handles specific business functionality
- **Interface-Based Design**: Components communicate through well-defined APIs
- **Loose Coupling**: Components are independent and can be developed/deployed separately
- **High Reusability**: Components can be reused across different contexts
- **Modularity**: System is composed of discrete, manageable modules

### System Components

| Component                | Technology | Port | Status      | Functionality                        |
| ------------------------ | ---------- | ---- | ----------- | ------------------------------------ |
| **Backend API**    | .NET 8     | 5001 | ✅ Active   | Core business logic, data management |
| **React Client**   | React 18   | 3000 | ✅ Active   | RF4 (Deliveries), RF5 (Suppliers)    |
| **Angular Client** | Angular 17 | 4200 | ⚠️ Issues | RF1 (Products), RF3 (Reports)        |

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+npm

### Setup

```bash
# Make scripts executable and setup environment
./setup-polimarket.sh

# Start all services
./start-polimarket.sh

# Stop all services
./stop-polimarket.sh
```

## 📊 Service URLs

- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger/index.html
- **Angular Client**: http://localhost:4200
- **React Client**: http://localhost:3001
- **Health Check**: http://localhost:5001/api/Integracion/health

## 📋 Component Coverage

| Component | Description   | Client  |
| --------- | ------------- | ------- |
| RF1       | Authorization | Angular |
| RF5       | Suppliers     | Angular |
| RF3       | Inventory     | Angular |
| RF4       | Deliveries    | React   |
| RF2       | Sales         | React   |

## 🛠️ Development

### Scripts

- `./setup-polimarket.sh` - Initial setup and dependency installation
- `./start-polimarket.sh` - Start all services
- `./stop-polimarket.sh` - Stop all services

### VS Code

Open `polimarket-cbse.code-workspace` for a configured development environment.

## 📝 Logs

All service logs are stored in the `logs/` directory:

- `backend.log` - .NET API logs
- `angular.log` - Angular client logs
- `react.log` - React client logs

## 🔧 Troubleshooting

### Port Conflicts

If ports are in use, the scripts will attempt to stop existing processes.

### Force Stop

```bash
./stop-polimarket.sh --force
```

### Detailed Status

```bash
./status-polimarket.sh --detailed
```

### Monitor Services

```bash
./status-polimarket.sh --monitor
```

## 🏛️ CBSE Principles Demonstrated

1. **Separation of Concerns**: Each component handles specific business logic
2. **Interface-Based Design**: Well-defined APIs between components
3. **Loose Coupling**: Components communicate through HTTP APIs
4. **High Reusability**: Components can be used by different clients
5. **Scalability**: Components can be scaled independently

## 📚 Documentation

- API documentation available at Swagger UI
- Component interfaces defined in service files
- Architecture diagrams in the docs/ folder

---

**PoliMarket CBSE System** - Demonstrating Component-Based Software Engineering principles
