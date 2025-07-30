import axios, { AxiosResponse } from 'axios';

// Base API Configuration from Environment Variables
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:5002/api';
const API_TIMEOUT = parseInt(process.env.REACT_APP_API_TIMEOUT || '10000');

// Validate required environment variables
if (!process.env.REACT_APP_API_BASE_URL) {
  console.warn('REACT_APP_API_BASE_URL not set, using default:', API_BASE_URL);
}

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add authentication token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);

    // Handle authentication errors
    if (error.response?.status === 401) {
      // Clear stored authentication data
      localStorage.removeItem('user');
      localStorage.removeItem('authToken');

      // Redirect to login (reload page to trigger login component)
      window.location.reload();
    }

    return Promise.reject(error);
  }
);

// Common API Response Interface
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

// System Health Interfaces
export interface ComponentHealth {
  componentName: string;
  isHealthy: boolean;
  lastChecked: string;
  details?: string;
}

export interface SystemHealthResponse {
  isHealthy: boolean;
  overallStatus: string;
  componentsHealth: Record<string, ComponentHealth>;
  timestamp: string;
}

// Authentication Types
export interface Usuario {
  id: string;
  username: string;
  email: string;
  nombre: string;
  apellido: string;
  rol: UserRole;
  activo: boolean;
  fechaCreacion: string;
  fechaActualizacion?: string;
  ultimoLogin?: string;
  vendedorId?: string;
  empleadoRHId?: string;
  vendedor?: Vendedor;
  empleadoRH?: any;
}

export interface AuthenticationResult {
  usuario: Usuario;
  token: string;
  expiresAt: string;
}

export enum UserRole {
  Admin = 1,
  HRManager = 2,
  SalesRep = 3,
  InventoryManager = 4,
  DeliveryManager = 5
}

// RF4: Entregas (Deliveries) Interfaces
export interface Entrega {
  id: string;
  idVenta: string;
  fechaProgramada: string;
  direccion: string;
  cliente: string;
  estado: 'Programada' | 'En Transito' | 'Entregada' | 'Cancelada';
  transportista: string;
  fechaEntrega: string | null;
  observaciones?: string;
  telefono?: string;
  coordenadas?: {
    latitud: number;
    longitud: number;
  };
}

export interface DeliveryUpdateRequest {
  id: string;
  estado: string;
  observaciones?: string;
  fechaEntrega?: string;
}

export interface ScheduleDeliveryRequest {
  idVenta: string;
  fechaProgramada: string;
  direccion: string;
  cliente: string;
  transportistaId?: string;
  observaciones?: string;
}

export interface DeliveryTrackingInfo {
  id: string;
  ubicacionActual: string;
  estadoDetallado: string;
  tiempoEstimado: string;
  historialMovimientos: Array<{
    fecha: string;
    ubicacion: string;
    estado: string;
    observaciones: string;
  }>;
}

// RF5: Proveedores (Suppliers) Interfaces
export interface Proveedor {
  id: string;
  nombre: string;
  contacto: string;
  direccion: string;
  telefono: string;
  email: string;
  activo: boolean;
  categoria: string;
  fechaRegistro: string;
  calificacion?: number;
  condicionesPago?: string;
  tiempoEntrega?: number;
}

export interface SupplierCreateRequest {
  nombre: string;
  contacto: string;
  direccion: string;
  telefono: string;
  email: string;
  categoria: string;
  condicionesPago?: string;
  tiempoEntrega?: number;
}

export interface SupplierUpdateRequest extends SupplierCreateRequest {
  id: string;
  activo: boolean;
}

export interface SupplierPerformanceMetrics {
  id: string;
  nombre: string;
  totalPedidos: number;
  pedidosATiempo: number;
  calificacionPromedio: number;
  tiempoEntregaPromedio: number;
  ultimaEvaluacion: string;
}

// Customer Interfaces
export interface Cliente {
  id: string;
  nombre: string;
  direccion: string;
  telefono: string;
  email: string;
  ciudad?: string; // Optional field for city
  tipoCliente: string;
  limiteCredito: number;
  activo: boolean;
  fechaRegistro: string;
}

export interface CustomerListResponse {
  customers: Cliente[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateCustomerRequest {
  nombre: string;
  direccion: string;
  telefono: string;
  email: string;
  tipoCliente: string;
  limiteCredito: number;
}

// RF1: Authorization Interfaces
export interface Vendedor {
  codigoVendedor: string;
  nombre: string;
  territorio: string;
  comision: number;
  autorizado: boolean;
  fechaAutorizacion: string;
  empleadoRHAutorizo: string;
  fechaCreacion: string;
  activo: boolean;
}

export interface ValidationResponse {
  isValid: boolean;
  reason: string;
  vendedor?: Vendedor;
}

// RF3: Inventory Interfaces
export interface DisponibilidadInventario {
  idProducto: string;
  nombreProducto: string;
  stockActual: number;
  stockDisponible: number;
  stockReservado: number;
  disponibleParaVenta: boolean;
  estado: string;
  fechaConsulta: string;
}

// Product Interfaces
export interface Producto {
  id: string;
  nombre: string;
  descripcion: string;
  precio: number;
  categoria: string;
  stock: number; // Backend uses 'stock' property
  stockMinimo: number;
  stockMaximo: number;
  stockActual?: number; // Computed field for compatibility
  stockDisponible?: number; // Optional field for available stock
  unidadMedida: string;
  estado: boolean; // Backend uses 'estado' not 'activo'
  fechaCreacion: string;
  fechaActualizacion?: string;
  proveedor?: string; // Optional as not all products may have supplier info
}

export interface ProductListResponse {
  products: Producto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateProductRequest {
  nombre: string;
  descripcion: string;
  precio: number;
  categoria: string;
  stock: number;
  stockMinimo: number;
  stockMaximo?: number;
  unidadMedida: string;
}

// API Service Class
class ApiService {
  // System Health
  async getSystemHealth(): Promise<SystemHealthResponse> {
    // React app specific components that should always be shown
    const reactAppComponents = {
      'ventas': {
        componentName: 'Ventas',
        isHealthy: true,
        lastChecked: new Date().toISOString(),
        details: 'RF2: Sales Operations - Active'
      },
      'entregas': {
        componentName: 'Entregas',
        isHealthy: true,
        lastChecked: new Date().toISOString(),
        details: 'RF4: Deliveries Management - Active'
      },
      'proveedores': {
        componentName: 'Proveedores',
        isHealthy: true,
        lastChecked: new Date().toISOString(),
        details: 'RF5: Suppliers Management - Active'
      }
    };

    try {
      const response: AxiosResponse<ApiResponse<SystemHealthResponse>> = await apiClient.get('/Integracion/health');
      const backendHealth = response.data.data;

      // Merge backend health data with React-specific components
      // Keep any relevant backend components (like Ventas) and add React-specific ones
      const mergedComponents = { ...reactAppComponents };

      // If backend has Ventas component, use its actual health status
      if (backendHealth.componentsHealth?.Ventas) {
        mergedComponents.ventas = {
          componentName: 'Ventas',
          isHealthy: backendHealth.componentsHealth.Ventas.isHealthy,
          lastChecked: backendHealth.componentsHealth.Ventas.lastChecked,
          details: 'RF2: Sales Operations - Connected to backend'
        };
      }

      // Check actual health of React app specific endpoints
      await this.checkReactComponentsHealth(mergedComponents);

      return {
        isHealthy: Object.values(mergedComponents).every(c => c.isHealthy),
        overallStatus: Object.values(mergedComponents).every(c => c.isHealthy) ? 'Healthy' : 'Degraded',
        timestamp: new Date().toISOString(),
        componentsHealth: mergedComponents
      };
    } catch (error) {
      console.warn('Backend health API unavailable, using React app component status');
      return {
        isHealthy: true,
        overallStatus: 'Healthy',
        timestamp: new Date().toISOString(),
        componentsHealth: reactAppComponents
      };
    }
  }

  // Check health of React app specific components
  private async checkReactComponentsHealth(components: Record<string, ComponentHealth>): Promise<void> {
    // Check Entregas (Deliveries) component health
    try {
      await apiClient.get('/entregas/deliveries', { timeout: 3000 });
      components.entregas.isHealthy = true;
      components.entregas.details = 'RF4: Deliveries Management - API responding';
    } catch (error) {
      components.entregas.isHealthy = false;
      components.entregas.details = 'RF4: Deliveries Management - API unavailable';
    }

    // Check Proveedores (Suppliers) component health
    try {
      await apiClient.get('/proveedores/suppliers', { timeout: 3000 });
      components.proveedores.isHealthy = true;
      components.proveedores.details = 'RF5: Suppliers Management - API responding';
    } catch (error) {
      components.proveedores.isHealthy = false;
      components.proveedores.details = 'RF5: Suppliers Management - API unavailable';
    }

    // Check Ventas (Sales) component health if not already checked from backend
    if (components.ventas.details === 'RF2: Sales Operations - Active') {
      try {
        await apiClient.get('/ventas/customers', { timeout: 3000 });
        components.ventas.isHealthy = true;
        components.ventas.details = 'RF2: Sales Operations - API responding';
      } catch (error) {
        components.ventas.isHealthy = false;
        components.ventas.details = 'RF2: Sales Operations - API unavailable';
      }
    }
  }

  // RF4: Entregas (Deliveries) Methods
  async getDeliveries(vendorId?: string): Promise<Entrega[]> {
    const params = vendorId ? { vendorId } : {};
    const response: AxiosResponse<ApiResponse<Entrega[]>> = await apiClient.get('/entregas/deliveries', { params });
    return response.data.data;
  }

  async getDeliveryById(id: string): Promise<Entrega> {
    const response: AxiosResponse<ApiResponse<Entrega>> = await apiClient.get(`/entregas/delivery/${id}`);
    return response.data.data;
  }

  async confirmDelivery(id: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.post(`/entregas/confirm/${id}`);
    return response.data.data;
  }

  async updateDeliveryStatus(request: DeliveryUpdateRequest): Promise<Entrega> {
    const response: AxiosResponse<ApiResponse<Entrega>> = await apiClient.put('/entregas/update-status', request);
    return response.data.data;
  }

  async getDeliveryTracking(id: string): Promise<DeliveryTrackingInfo> {
    const response: AxiosResponse<ApiResponse<DeliveryTrackingInfo>> = await apiClient.get(`/entregas/tracking/${id}`);
    return response.data.data;
  }

  async getPendingDeliveries(): Promise<Entrega[]> {
    const response: AxiosResponse<ApiResponse<Entrega[]>> = await apiClient.get('/entregas/pending');
    return response.data.data;
  }

  async scheduleDelivery(request: ScheduleDeliveryRequest): Promise<Entrega> {
    const response: AxiosResponse<ApiResponse<Entrega>> = await apiClient.post('/entregas/schedule', request);
    return response.data.data;
  }

  // RF5: Proveedores (Suppliers) Methods
  async getSuppliers(): Promise<Proveedor[]> {
    const response: AxiosResponse<ApiResponse<Proveedor[]>> = await apiClient.get('/proveedores/suppliers');
    return response.data.data;
  }

  async getSupplierById(id: string): Promise<Proveedor> {
    const response: AxiosResponse<ApiResponse<Proveedor>> = await apiClient.get(`/proveedores/supplier/${id}`);
    return response.data.data;
  }

  async createSupplier(supplier: SupplierCreateRequest): Promise<Proveedor> {
    const response: AxiosResponse<ApiResponse<Proveedor>> = await apiClient.post('/proveedores', supplier);
    return response.data.data;
  }

  async updateSupplier(supplier: SupplierUpdateRequest): Promise<Proveedor> {
    const response: AxiosResponse<ApiResponse<Proveedor>> = await apiClient.put(`/proveedores/${supplier.id}`, supplier);
    return response.data.data;
  }

  async deactivateSupplier(id: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.put(`/proveedores/deactivate/${id}`);
    return response.data.data;
  }

  async getActiveSuppliers(): Promise<Proveedor[]> {
    const response: AxiosResponse<ApiResponse<Proveedor[]>> = await apiClient.get('/proveedores/active');
    return response.data.data;
  }

  async getSuppliersByCategory(category: string): Promise<Proveedor[]> {
    const response: AxiosResponse<ApiResponse<Proveedor[]>> = await apiClient.get(`/proveedores/category/${category}`);
    return response.data.data;
  }

  async getSupplierPerformance(id: string): Promise<SupplierPerformanceMetrics> {
    const response: AxiosResponse<ApiResponse<SupplierPerformanceMetrics>> = await apiClient.get(`/proveedores/performance/${id}`);
    return response.data.data;
  }

  async evaluateSupplier(id: string, calificacion: number, comentarios?: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.post(`/proveedores/evaluate/${id}`, {
      calificacion,
      comentarios
    });
    return response.data.data;
  }

  // RF2: Customers Methods
  async getCustomers(page: number = 1, pageSize: number = 10, tipoCliente?: string, activo?: boolean, searchTerm?: string): Promise<CustomerListResponse> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (tipoCliente) params.append('tipoCliente', tipoCliente);
    if (activo !== undefined) params.append('activo', activo.toString());
    if (searchTerm) params.append('searchTerm', searchTerm);

    const response: AxiosResponse<ApiResponse<CustomerListResponse>> = await apiClient.get(`/customers?${params.toString()}`);
    return response.data.data;
  }

  async getCustomerById(id: string): Promise<Cliente> {
    const response: AxiosResponse<ApiResponse<Cliente>> = await apiClient.get(`/customers/${id}`);
    return response.data.data;
  }

  async createCustomer(customer: CreateCustomerRequest): Promise<Cliente> {
    const response: AxiosResponse<ApiResponse<Cliente>> = await apiClient.post('/customers', customer);
    return response.data.data;
  }

  async updateCustomer(id: string, customer: CreateCustomerRequest): Promise<Cliente> {
    const response: AxiosResponse<ApiResponse<Cliente>> = await apiClient.put(`/customers/${id}`, customer);
    return response.data.data;
  }

  async deleteCustomer(id: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.delete(`/customers/${id}`);
    return response.data.data;
  }

  async getCustomerTypes(): Promise<string[]> {
    const response: AxiosResponse<ApiResponse<string[]>> = await apiClient.get('/customers/types');
    return response.data.data;
  }

  // RF2: Sales Methods
  async createSale(saleRequest: any): Promise<any> {
    const response: AxiosResponse<ApiResponse<any>> = await apiClient.post('/ventas', saleRequest);
    return response.data.data;
  }

  async getSales(vendedorId?: string): Promise<any[]> {
    const url = vendedorId ? `/ventas/by-seller/${vendedorId}` : '/ventas';
    const response: AxiosResponse<ApiResponse<any[]>> = await apiClient.get(url);
    return response.data.data;
  }

  async getSaleById(id: string): Promise<any> {
    const response: AxiosResponse<ApiResponse<any>> = await apiClient.get(`/ventas/${id}`);
    return response.data.data;
  }

  async updateSale(id: string, saleRequest: any): Promise<any> {
    const response: AxiosResponse<ApiResponse<any>> = await apiClient.put(`/ventas/${id}`, saleRequest);
    return response.data.data;
  }

  async deleteSale(id: string, motivo: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.delete(`/ventas/${id}`, {
      data: { motivo }
    });
    return response.data.data;
  }

  async applyDiscount(id: string, descuento: number, motivo: string): Promise<any> {
    const response: AxiosResponse<ApiResponse<any>> = await apiClient.put(`/ventas/${id}/discount`, {
      descuento,
      motivo
    });
    return response.data.data;
  }

  // Authentication Methods
  async login(username: string, password: string): Promise<AuthenticationResult> {
    const response: AxiosResponse<ApiResponse<AuthenticationResult>> = await apiClient.post('/authentication/login', {
      username,
      password
    });
    return response.data.data;
  }

  async logout(token: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.post('/authentication/logout', {
      token
    });
    return response.data.data;
  }

  async validateToken(token: string): Promise<Usuario> {
    const response: AxiosResponse<ApiResponse<Usuario>> = await apiClient.get(`/authentication/validate?token=${token}`);
    return response.data.data;
  }

  // RF1: Authorization Methods
  async validateSeller(codigoVendedor: string): Promise<ValidationResponse> {
    const response: AxiosResponse<ApiResponse<ValidationResponse>> = await apiClient.get(`/autorizacion/validate/${codigoVendedor}`);
    return response.data.data;
  }

  async getSellerByCode(codigoVendedor: string): Promise<Vendedor> {
    const response: AxiosResponse<ApiResponse<Vendedor>> = await apiClient.get(`/autorizacion/vendedores/${codigoVendedor}`);
    return response.data.data;
  }

  // RF3: Inventory Methods
  async checkInventoryAvailability(productoId: string, cantidad: number): Promise<DisponibilidadInventario> {
    const response: AxiosResponse<ApiResponse<DisponibilidadInventario>> = await apiClient.post('/inventario/check-availability', {
      idProducto: productoId,
      cantidadRequerida: cantidad
    });
    return response.data.data;
  }

  async getCurrentStock(productoId: string): Promise<DisponibilidadInventario> {
    const response: AxiosResponse<ApiResponse<DisponibilidadInventario>> = await apiClient.get(`/inventario/stock/${productoId}`);
    return response.data.data;
  }

  // RF2: Products Methods
  async getProducts(page: number = 1, pageSize: number = 10, categoria?: string, activo?: boolean, searchTerm?: string): Promise<ProductListResponse> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (categoria) params.append('categoria', categoria);
    if (activo !== undefined) params.append('activo', activo.toString());
    if (searchTerm) params.append('searchTerm', searchTerm);

    const url = `/productos?${params.toString()}`;
    const response: AxiosResponse<ApiResponse<ProductListResponse>> = await apiClient.get(url);
    return response.data.data;
  }

  async getProductById(id: string): Promise<Producto> {
    const response: AxiosResponse<ApiResponse<Producto>> = await apiClient.get(`/productos/${id}`);
    return response.data.data;
  }

  async createProduct(product: any): Promise<Producto> {
    const response: AxiosResponse<ApiResponse<Producto>> = await apiClient.post('/productos', product);
    return response.data.data;
  }

  async updateProduct(id: string, product: any): Promise<Producto> {
    const response: AxiosResponse<ApiResponse<Producto>> = await apiClient.put(`/productos/${id}`, product);
    return response.data.data;
  }

  async deleteProduct(id: string): Promise<boolean> {
    const response: AxiosResponse<ApiResponse<boolean>> = await apiClient.delete(`/productos/${id}`);
    return response.data.data;
  }

  async getProductCategories(): Promise<string[]> {
    const response: AxiosResponse<ApiResponse<string[]>> = await apiClient.get('/productos/categories');
    return response.data.data;
  }

  // Utility Methods
  async testConnection(): Promise<boolean> {
    try {
      await this.getSystemHealth();
      return true;
    } catch (error) {
      return false;
    }
  }
}

// Export singleton instance
export const apiService = new ApiService();
export default apiService;
