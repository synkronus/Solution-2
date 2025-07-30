#!/bin/bash

# PoliMarket CBSE System Setup Script
# This script sets up the development environment for PoliMarket
# Author: PoliMarket Development Team
# Version: 1.0

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration
BACKEND_DIR="Backend"
ANGULAR_CLIENT_DIR="Client1-Angular"
REACT_CLIENT_DIR="Client2-React"
LOG_DIR="logs"
PID_DIR="pids"

# Function to print colored output
print_status() {
    echo -e "${BLUE}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[$(date '+%Y-%m-%d %H:%M:%S')] ✅ $1${NC}"
}

print_error() {
    echo -e "${RED}[$(date '+%Y-%m-%d %H:%M:%S')] ❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}[$(date '+%Y-%m-%d %H:%M:%S')] ⚠️  $1${NC}"
}

print_info() {
    echo -e "${CYAN}[$(date '+%Y-%m-%d %H:%M:%S')] ℹ️  $1${NC}"
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check system requirements
check_requirements() {
    print_status "Checking system requirements..."
    
    local requirements_met=true
    
    # Check .NET SDK
    if command_exists dotnet; then
        local dotnet_version=$(dotnet --version 2>/dev/null || echo "unknown")
        print_success ".NET SDK found (version: $dotnet_version)"
    else
        print_error ".NET SDK not found. Please install .NET 8.0 SDK"
        print_info "Download from: https://dotnet.microsoft.com/download"
        requirements_met=false
    fi
    
    # Check Node.js
    if command_exists node; then
        local node_version=$(node --version 2>/dev/null || echo "unknown")
        print_success "Node.js found (version: $node_version)"
    else
        print_error "Node.js not found. Please install Node.js 18+"
        print_info "Download from: https://nodejs.org/"
        requirements_met=false
    fi
    
    # Check npm
    if command_exists npm; then
        local npm_version=$(npm --version 2>/dev/null || echo "unknown")
        print_success "npm found (version: $npm_version)"
    else
        print_error "npm not found. Please install npm"
        requirements_met=false
    fi
    
    # Check Angular CLI (optional)
    if command_exists ng; then
        local ng_version=$(ng version --skip-git 2>/dev/null | grep "Angular CLI" | cut -d' ' -f3 || echo "unknown")
        print_success "Angular CLI found (version: $ng_version)"
    else
        print_warning "Angular CLI not found. Installing compatible version..."
        npm install -g @angular/cli@17 --force
        print_success "Angular CLI installed"
    fi
    
    # Check useful tools
    if command_exists curl; then
        print_success "curl found"
    else
        print_warning "curl not found. Some features may not work properly"
    fi
    
    if command_exists jq; then
        print_success "jq found"
    else
        print_warning "jq not found. JSON parsing in status script may not work"
        print_info "Install with: brew install jq (macOS) or apt-get install jq (Ubuntu)"
    fi
    
    if ! $requirements_met; then
        print_error "Some requirements are not met. Please install missing dependencies."
        exit 1
    fi
    
    print_success "All requirements are satisfied"
}

# Function to create directory structure
create_directories() {
    print_status "Creating directory structure..."
    
    mkdir -p "$LOG_DIR"
    mkdir -p "$PID_DIR"
    
    print_success "Directory structure created"
}

# Function to setup .NET backend
setup_backend() {
    print_status "Setting up .NET Backend..."
    
    if [ ! -d "$BACKEND_DIR" ]; then
        print_error "Backend directory '$BACKEND_DIR' not found!"
        return 1
    fi
    
    cd "$BACKEND_DIR"

    # Restore dependencies for the solution
    print_status "Restoring .NET dependencies..."
    dotnet restore

    # Build the entire solution
    print_status "Building .NET solution..."
    dotnet build

    cd ..
    
    print_success ".NET Backend setup completed"
}

# Function to setup Angular client
setup_angular() {
    print_status "Setting up Angular Client..."
    
    if [ ! -d "$ANGULAR_CLIENT_DIR" ]; then
        print_error "Angular client directory '$ANGULAR_CLIENT_DIR' not found!"
        return 1
    fi
    
    cd "$ANGULAR_CLIENT_DIR"
    
    # Install dependencies
    print_status "Installing Angular dependencies..."
    npm install
    
    # Check if PrimeNG is installed
    if npm list primeng >/dev/null 2>&1; then
        print_success "PrimeNG is already installed"
    else
        print_status "Installing PrimeNG..."
        npm install primeng@17.18.11 primeicons --legacy-peer-deps
    fi
    
    cd ..
    
    print_success "Angular Client setup completed"
}

# Function to setup React client
setup_react() {
    print_status "Setting up React Client..."
    
    if [ ! -d "$REACT_CLIENT_DIR" ]; then
        print_error "React client directory '$REACT_CLIENT_DIR' not found!"
        return 1
    fi
    
    cd "$REACT_CLIENT_DIR"
    
    # Install dependencies
    print_status "Installing React dependencies..."
    npm install
    
    # Check if PrimeReact is installed
    if npm list primereact >/dev/null 2>&1; then
        print_success "PrimeReact is already installed"
    else
        print_status "Installing PrimeReact..."
        npm install primereact primeicons
    fi
    
    cd ..
    
    print_success "React Client setup completed"
}

# Function to make scripts executable
setup_scripts() {
    print_status "Setting up scripts..."
    
    # Make all scripts executable
    chmod +x start-polimarket.sh
    chmod +x stop-polimarket.sh
    chmod +x status-polimarket.sh
    chmod +x setup-polimarket.sh
    
    print_success "Scripts are now executable"
}

# Function to create VS Code workspace
create_vscode_workspace() {
    print_status "Creating VS Code workspace..."
    
    cat > polimarket-cbse.code-workspace << 'EOF'
{
    "folders": [
        {
            "name": "🔧 Backend (.NET)",
            "path": "./Backend-CBSE"
        },
        {
            "name": "🅰️ Angular Client",
            "path": "./Client1-Angular"
        },
        {
            "name": "⚛️ React Client",
            "path": "./Client2-React"
        },
        {
            "name": "📜 Scripts",
            "path": "."
        }
    ],
    "settings": {
        "typescript.preferences.includePackageJsonAutoImports": "auto",
        "editor.formatOnSave": true,
        "editor.codeActionsOnSave": {
            "source.fixAll": true
        },
        "files.exclude": {
            "**/node_modules": true,
            "**/bin": true,
            "**/obj": true,
            "**/logs": true,
            "**/pids": true
        }
    },
    "extensions": {
        "recommendations": [
            "ms-dotnettools.csharp",
            "angular.ng-template",
            "ms-vscode.vscode-typescript-next",
            "esbenp.prettier-vscode",
            "bradlc.vscode-tailwindcss"
        ]
    },
    "tasks": {
        "version": "2.0.0",
        "tasks": [
            {
                "label": "🚀 Start All Services",
                "type": "shell",
                "command": "./start-polimarket.sh",
                "group": "build",
                "presentation": {
                    "echo": true,
                    "reveal": "always",
                    "focus": false,
                    "panel": "new"
                }
            },
            {
                "label": "🛑 Stop All Services",
                "type": "shell",
                "command": "./stop-polimarket.sh",
                "group": "build"
            },
            {
                "label": "📊 Check Status",
                "type": "shell",
                "command": "./status-polimarket.sh --detailed",
                "group": "test"
            },
            {
                "label": "🔧 Build Backend",
                "type": "shell",
                "command": "dotnet build",
                "options": {
                    "cwd": "${workspaceFolder}/Backend-CBSE"
                },
                "group": "build"
            },
            {
                "label": "🅰️ Build Angular",
                "type": "shell",
                "command": "npm run build",
                "options": {
                    "cwd": "${workspaceFolder}/Client1-Angular"
                },
                "group": "build"
            },
            {
                "label": "⚛️ Build React",
                "type": "shell",
                "command": "npm run build",
                "options": {
                    "cwd": "${workspaceFolder}/Client2-React"
                },
                "group": "build"
            }
        ]
    }
}
EOF
    
    print_success "VS Code workspace created: polimarket-cbse.code-workspace"
}

# Function to create README
create_readme() {
    print_status "Creating README file..."
    
    cat > README.md << 'EOF'
# 🏪 PoliMarket CBSE System

Component-Based Software Engineering implementation for PoliMarket e-commerce platform.

## 🏗️ Architecture Overview

This system demonstrates CBSE principles through a distributed architecture with the following components:

- **🔧 Backend (.NET)**: Core API with component-based architecture
- **🅰️ Angular Client**: Consumes RF1 (Authorization) and RF3 (Inventory)
- **⚛️ React Client**: Consumes RF4 (Deliveries) and RF5 (Suppliers)

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- npm

### Setup
```bash
# Make scripts executable and setup environment
./setup-polimarket.sh

# Start all services
./start-polimarket.sh

# Check status
./status-polimarket.sh

# Stop all services
./stop-polimarket.sh
```

## 📊 Service URLs

- **Backend API**: http://localhost:5001
- **Swagger UI**: http://localhost:5001
- **Angular Client**: http://localhost:4200
- **React Client**: http://localhost:3001
- **Health Check**: http://localhost:5001/api/Integracion/health

## 📋 Component Coverage

| Component | Description | Client |
|-----------|-------------|---------|
| RF1 | Authorization | Angular |
| RF3 | Inventory | Angular |
| RF4 | Deliveries | React |
| RF5 | Suppliers | React |

## 🛠️ Development

### Scripts
- `./setup-polimarket.sh` - Initial setup and dependency installation
- `./start-polimarket.sh` - Start all services
- `./stop-polimarket.sh` - Stop all services
- `./status-polimarket.sh` - Check system status

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
EOF
    
    print_success "README.md created"
}

# Function to show final setup status
show_setup_complete() {
    echo ""
    echo -e "${PURPLE}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${PURPLE}║                    🏪 PoliMarket CBSE System                 ║${NC}"
    echo -e "${PURPLE}║                      Setup Complete!                         ║${NC}"
    echo -e "${PURPLE}╚══════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${GREEN}🎉 PoliMarket CBSE System setup completed successfully!${NC}"
    echo ""
    echo -e "${CYAN}📊 What's been set up:${NC}"
    echo -e "  ✅ System requirements verified"
    echo -e "  ✅ Directory structure created"
    echo -e "  ✅ .NET Backend dependencies restored"
    echo -e "  ✅ Angular Client dependencies installed"
    echo -e "  ✅ React Client dependencies installed"
    echo -e "  ✅ Scripts made executable"
    echo -e "  ✅ VS Code workspace created"
    echo -e "  ✅ README documentation created"
    echo ""
    echo -e "${YELLOW}🚀 Next Steps:${NC}"
    echo -e "  1. Start all services: ${GREEN}./start-polimarket.sh${NC}"
    echo -e "  2. Check status: ${GREEN}./status-polimarket.sh${NC}"
    echo -e "  3. Open VS Code: ${GREEN}code polimarket-cbse.code-workspace${NC}"
    echo ""
    echo -e "${CYAN}📚 Available Commands:${NC}"
    echo -e "  🚀 ./start-polimarket.sh    - Start all services"
    echo -e "  🛑 ./stop-polimarket.sh     - Stop all services"
    echo -e "  📊 ./status-polimarket.sh   - Check system status"
    echo -e "  🔧 ./setup-polimarket.sh    - Re-run setup (this script)"
    echo ""
}

# Main execution
main() {
    echo -e "${PURPLE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║                    🏪 PoliMarket CBSE System                 ║"
    echo "║                        Setup Script                          ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
    
    print_status "Initializing PoliMarket CBSE System setup..."
    
    # Run setup steps
    check_requirements
    create_directories
    setup_backend
    setup_angular
    setup_react
    setup_scripts
    create_vscode_workspace
    create_readme
    
    # Show completion status
    show_setup_complete
}

# Handle command line arguments
case "${1:-}" in
    --help|-h)
        echo "PoliMarket CBSE System Setup Script"
        echo ""
        echo "Usage: $0 [OPTIONS]"
        echo ""
        echo "Options:"
        echo "  --help, -h     Show this help message"
        echo ""
        echo "This script will:"
        echo "  - Check system requirements"
        echo "  - Create necessary directories"
        echo "  - Install dependencies for all components"
        echo "  - Set up development environment"
        echo "  - Create VS Code workspace"
        echo "  - Generate documentation"
        ;;
    *)
        main "$@"
        ;;
esac
