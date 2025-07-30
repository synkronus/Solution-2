#!/bin/bash

# PoliMarket CBSE System Stop Script
# This script stops all components of the PoliMarket system
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
PID_DIR="pids"
LOG_DIR="logs"

# Ports
BACKEND_PORT=5001
ANGULAR_PORT=4200
REACT_PORT=3001

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

# Function to stop a service by PID file
stop_service() {
    local service_name=$1
    local pid_file="$PID_DIR/$2.pid"
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        if kill -0 "$pid" 2>/dev/null; then
            print_status "Stopping $service_name (PID: $pid)..."
            kill "$pid" 2>/dev/null || true
            
            # Wait for process to stop
            local attempts=0
            while kill -0 "$pid" 2>/dev/null && [ $attempts -lt 10 ]; do
                sleep 1
                attempts=$((attempts + 1))
            done
            
            # Force kill if still running
            if kill -0 "$pid" 2>/dev/null; then
                print_warning "Force killing $service_name..."
                kill -9 "$pid" 2>/dev/null || true
            fi
            
            print_success "$service_name stopped successfully"
        else
            print_info "$service_name was not running"
        fi
        rm -f "$pid_file"
    else
        print_info "No PID file found for $service_name"
    fi
}

# Function to stop processes by port
stop_by_port() {
    local port=$1
    local service_name=$2
    
    print_status "Checking for processes on port $port..."
    local pids=$(lsof -ti:$port 2>/dev/null || true)
    
    if [ -n "$pids" ]; then
        print_status "Stopping $service_name processes on port $port..."
        echo "$pids" | xargs kill -TERM 2>/dev/null || true
        sleep 2
        
        # Force kill if still running
        local remaining_pids=$(lsof -ti:$port 2>/dev/null || true)
        if [ -n "$remaining_pids" ]; then
            print_warning "Force killing remaining $service_name processes..."
            echo "$remaining_pids" | xargs kill -9 2>/dev/null || true
        fi
        
        print_success "$service_name processes on port $port stopped"
    else
        print_info "No processes found on port $port"
    fi
}

# Function to stop all Node.js processes related to our projects
stop_node_processes() {
    print_status "Stopping Node.js development servers..."
    
    # Stop Angular processes
    pkill -f "ng serve" 2>/dev/null || true
    pkill -f "angular" 2>/dev/null || true
    
    # Stop React processes
    pkill -f "react-scripts start" 2>/dev/null || true
    pkill -f "react-dev-server" 2>/dev/null || true
    
    # Stop any npm start processes
    pkill -f "npm start" 2>/dev/null || true
    
    print_success "Node.js processes stopped"
}

# Function to stop .NET processes
stop_dotnet_processes() {
    print_status "Stopping .NET processes..."
    
    # Stop dotnet run processes
    pkill -f "dotnet run" 2>/dev/null || true
    pkill -f "dotnet.*PoliMarket.API" 2>/dev/null || true
    pkill -f "PoliMarket.API" 2>/dev/null || true
    
    print_success ".NET processes stopped"
}

# Function to clean up resources
cleanup_resources() {
    print_status "Cleaning up resources..."
    
    # Remove PID files
    if [ -d "$PID_DIR" ]; then
        rm -f "$PID_DIR"/*.pid
        print_success "PID files cleaned up"
    fi
    
    # Optionally clean up log files (commented out to preserve logs)
    # if [ -d "$LOG_DIR" ]; then
    #     rm -f "$LOG_DIR"/*.log
    #     print_success "Log files cleaned up"
    # fi
}

# Function to show final status
show_final_status() {
    echo ""
    echo -e "${PURPLE}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${PURPLE}║                    🏪 PoliMarket CBSE System                 ║${NC}"
    echo -e "${PURPLE}║                        System Stopped                        ║${NC}"
    echo -e "${PURPLE}╚══════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${GREEN}🛑 All PoliMarket services have been stopped successfully!${NC}"
    echo ""
    echo -e "${CYAN}📊 Stopped Services:${NC}"
    echo -e "  🔧 .NET Backend API (Port $BACKEND_PORT)"
    echo -e "  🅰️  Angular Client (Port $ANGULAR_PORT)"
    echo -e "  ⚛️  React Client (Port $REACT_PORT)"
    echo ""
    echo -e "${YELLOW}📝 Logs are preserved in the 'logs' directory${NC}"
    echo -e "${GREEN}🚀 To start the system again, run: ./start-polimarket.sh${NC}"
    echo ""
}

# Main execution
main() {
    echo -e "${PURPLE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║                    🏪 PoliMarket CBSE System                 ║"
    echo "║                         Stop Script                          ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
    
    print_status "Stopping PoliMarket CBSE System..."
    
    # Stop services by PID files first
    stop_service ".NET Backend API" "backend"
    stop_service "Angular Client" "angular"
    stop_service "React Client" "react"
    
    # Stop by ports as backup
    stop_by_port $BACKEND_PORT ".NET Backend"
    stop_by_port $ANGULAR_PORT "Angular Client"
    stop_by_port $REACT_PORT "React Client"
    
    # Stop processes by name as final cleanup
    stop_dotnet_processes
    stop_node_processes
    
    # Clean up resources
    cleanup_resources
    
    # Wait a moment for everything to settle
    sleep 2
    
    # Show final status
    show_final_status
}

# Handle command line arguments
case "${1:-}" in
    --force|-f)
        print_warning "Force stop mode enabled"
        # Kill all processes immediately
        pkill -f "dotnet.*PoliMarket.API" 2>/dev/null || true
        pkill -f "PoliMarket.API" 2>/dev/null || true
        pkill -f "ng serve" 2>/dev/null || true
        pkill -f "react-scripts start" 2>/dev/null || true
        pkill -f "npm start" 2>/dev/null || true
        
        # Clean up
        cleanup_resources
        print_success "Force stop completed"
        ;;
    --help|-h)
        echo "PoliMarket CBSE System Stop Script"
        echo ""
        echo "Usage: $0 [OPTIONS]"
        echo ""
        echo "Options:"
        echo "  --force, -f    Force stop all processes immediately"
        echo "  --help, -h     Show this help message"
        echo ""
        echo "Examples:"
        echo "  $0             Normal stop (graceful shutdown)"
        echo "  $0 --force     Force stop all processes"
        ;;
    *)
        main "$@"
        ;;
esac
