#!/bin/bash

# PoliMarket CBSE API Quick Test Script
# This script performs basic connectivity tests for all API endpoints

BASE_URL="http://localhost:5001"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "🚀 PoliMarket CBSE API Quick Test Suite"
echo "========================================"
echo ""

# Function to test endpoint
test_endpoint() {
    local method=$1
    local endpoint=$2
    local description=$3
    local data=$4
    
    echo -n "Testing: $description... "
    
    if [ "$method" = "GET" ]; then
        response=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL$endpoint")
    else
        response=$(curl -s -w "%{http_code}" -o /dev/null -X "$method" -H "Content-Type: application/json" -d "$data" "$BASE_URL$endpoint")
    fi
    
    if [ "$response" = "200" ] || [ "$response" = "201" ]; then
        echo -e "${GREEN}✅ PASS${NC} (HTTP $response)"
    else
        echo -e "${RED}❌ FAIL${NC} (HTTP $response)"
    fi
}

# Test 1: API Information
test_endpoint "GET" "/api" "API Information"

# Test 2: System Health Check
test_endpoint "GET" "/api/Integracion/health" "System Health Check"

# Test 3: Swagger Documentation
echo -n "Testing: Swagger Documentation... "
swagger_response=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/swagger/index.html")
if [ "$swagger_response" = "200" ]; then
    echo -e "${GREEN}✅ PASS${NC} (HTTP $swagger_response)"
else
    echo -e "${RED}❌ FAIL${NC} (HTTP $swagger_response)"
fi

echo ""
echo "🔐 Authorization Component Tests"
echo "--------------------------------"

# Test 4: Authorize Seller
auth_data='{
  "codigoVendedor": "V001",
  "nombreCompleto": "Juan Pérez Test",
  "email": "juan.test@polimarket.com",
  "telefono": "+57 300 123 4567",
  "departamento": "Ventas",
  "nivelAutorizacion": "Vendedor"
}'
test_endpoint "POST" "/api/Autorizacion/authorize" "Authorize New Seller" "$auth_data"

# Test 5: Validate Seller
test_endpoint "GET" "/api/Autorizacion/validate/V001" "Validate Seller Authorization"

# Test 6: Get All Sellers
test_endpoint "GET" "/api/Autorizacion/vendedores" "Get All Authorized Sellers"

echo ""
echo "💰 Sales Component Tests"
echo "------------------------"

# Test 7: Calculate Sale Total
calc_data='{
  "productos": [
    {
      "codigoProducto": "P001",
      "cantidad": 1,
      "precioUnitario": 1500000,
      "descuento": 10.0
    }
  ],
  "aplicarImpuestos": true
}'
test_endpoint "POST" "/api/Ventas/calcular-total" "Calculate Sale Total" "$calc_data"

# Test 8: Create Sale
sale_data='{
  "codigoVendedor": "V001",
  "clienteId": "C001",
  "productos": [
    {
      "codigoProducto": "P001",
      "nombreProducto": "Test Product",
      "cantidad": 1,
      "precioUnitario": 1500000,
      "descuento": 5.0
    }
  ],
  "metodoPago": "Efectivo"
}'
test_endpoint "POST" "/api/Ventas/crear" "Create New Sale" "$sale_data"

# Test 9: Get Sales History
test_endpoint "GET" "/api/Ventas/historial?limite=10" "Get Sales History"

echo ""
echo "📦 Inventory Component Tests"
echo "----------------------------"

# Test 10: Check Stock
test_endpoint "GET" "/api/Inventario/disponibilidad/P001" "Check Stock Availability"

# Test 11: Reserve Stock
reserve_data='{
  "codigoProducto": "P001",
  "cantidad": 5,
  "ventaId": "TEST-SALE-001",
  "tiempoReserva": 30
}'
test_endpoint "POST" "/api/Inventario/reservar" "Reserve Stock" "$reserve_data"

# Test 12: Update Stock
update_data='{
  "codigoProducto": "P001",
  "tipoMovimiento": "Entrada",
  "cantidad": 50,
  "motivo": "Test restock",
  "proveedor": "TEST-PROV"
}'
test_endpoint "PUT" "/api/Inventario/actualizar" "Update Stock" "$update_data"

# Test 13: Get Low Stock Alerts
test_endpoint "GET" "/api/Inventario/alertas-stock-bajo" "Get Low Stock Alerts"

echo ""
echo "🔗 Integration Component Tests"
echo "------------------------------"

# Test 14: Execute Complete Sale Transaction
transaction_data='{
  "transactionId": "TXN-TEST-001",
  "transactionType": "complete_sale",
  "data": {
    "codigoVendedor": "V001",
    "clienteId": "C001",
    "productos": [
      {
        "codigoProducto": "P001",
        "cantidad": 1,
        "precioUnitario": 1500000
      }
    ]
  },
  "requestedBy": "API_TEST"
}'
test_endpoint "POST" "/api/Integracion/execute-transaction" "Execute Complete Sale Transaction" "$transaction_data"

# Test 15: Synchronize Data
sync_data='{
  "sourceComponent": "Sales",
  "targetComponent": "Inventory",
  "syncType": "ProductMovements"
}'
test_endpoint "POST" "/api/Integracion/synchronize" "Synchronize Component Data" "$sync_data"

# Test 16: Get Component Configuration
test_endpoint "GET" "/api/Integracion/configuration/Sales" "Get Component Configuration"

echo ""
echo "📊 Test Summary"
echo "==============="
echo -e "${YELLOW}API Base URL:${NC} $BASE_URL"
echo -e "${YELLOW}Swagger Docs:${NC} $BASE_URL/swagger/index.html"
echo -e "${YELLOW}Health Check:${NC} $BASE_URL/api/Integracion/health"
echo ""
echo "✅ Green = Test Passed (HTTP 200/201)"
echo "❌ Red = Test Failed (HTTP 4xx/5xx)"
echo ""
echo "For detailed testing, use the Postman collection:"
echo "- PoliMarket-API-Tests.postman_collection.json"
echo "- PoliMarket-API-Environment.postman_environment.json"
echo ""
echo "🎉 Quick test completed!"
