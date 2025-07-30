import React, { createContext, useContext, useReducer, useCallback, ReactNode } from 'react';
import { Entrega, Proveedor, Cliente, Producto, SystemHealthResponse, ComponentHealth } from '../services/apiService';

// User Interface
export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  nombre?: string;
  apellido?: string;
  vendedorId?: string;
  empleadoRHId?: string;
  token?: string;
  expiresAt?: string;
  isAuthenticated: boolean;
}

// State Interface
export interface AppState {
  // User/Auth
  user: User | null;
  isAuthenticated: boolean;

  // System
  systemHealth: SystemHealthResponse | null;
  isLoadingHealth: boolean;

  // Deliveries (RF4)
  deliveries: Entrega[];
  selectedDelivery: Entrega | null;
  isLoadingDeliveries: boolean;

  // Suppliers (RF5)
  suppliers: Proveedor[];
  selectedSupplier: Proveedor | null;
  isLoadingSuppliers: boolean;

  // Customers (RF2)
  customers: Cliente[];
  selectedCustomer: Cliente | null;
  isLoadingCustomers: boolean;

  // Products (RF2)
  products: Producto[];
  selectedProduct: Producto | null;
  isLoadingProducts: boolean;

  // Sales (RF2)
  sales: any[];
  selectedSale: any | null;
  isLoadingSales: boolean;

  // UI State
  currentView: 'dashboard' | 'sales' | 'deliveries' | 'suppliers';
  notifications: Notification[];
  isLoading: boolean;
}

// Notification Interface
export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  autoHide?: boolean;
}

// Action Types
export type AppAction =
  // Auth Actions
  | { type: 'SET_USER'; payload: User | null }
  | { type: 'SET_AUTHENTICATED'; payload: boolean }
  | { type: 'LOGIN'; payload: User }
  | { type: 'LOGOUT' }

  // System Actions
  | { type: 'SET_SYSTEM_HEALTH'; payload: SystemHealthResponse }
  | { type: 'SET_LOADING_HEALTH'; payload: boolean }
  
  // Delivery Actions
  | { type: 'SET_DELIVERIES'; payload: Entrega[] }
  | { type: 'ADD_DELIVERY'; payload: Entrega }
  | { type: 'UPDATE_DELIVERY'; payload: Entrega }
  | { type: 'SET_SELECTED_DELIVERY'; payload: Entrega | null }
  | { type: 'SET_LOADING_DELIVERIES'; payload: boolean }
  
  // Supplier Actions
  | { type: 'SET_SUPPLIERS'; payload: Proveedor[] }
  | { type: 'ADD_SUPPLIER'; payload: Proveedor }
  | { type: 'UPDATE_SUPPLIER'; payload: Proveedor }
  | { type: 'SET_SELECTED_SUPPLIER'; payload: Proveedor | null }
  | { type: 'SET_LOADING_SUPPLIERS'; payload: boolean }

  // Customer Actions
  | { type: 'SET_CUSTOMERS'; payload: Cliente[] }
  | { type: 'ADD_CUSTOMER'; payload: Cliente }
  | { type: 'UPDATE_CUSTOMER'; payload: Cliente }
  | { type: 'DELETE_CUSTOMER'; payload: string }
  | { type: 'SET_SELECTED_CUSTOMER'; payload: Cliente | null }
  | { type: 'SET_LOADING_CUSTOMERS'; payload: boolean }

  // Product Actions
  | { type: 'SET_PRODUCTS'; payload: Producto[] }
  | { type: 'ADD_PRODUCT'; payload: Producto }
  | { type: 'UPDATE_PRODUCT'; payload: Producto }
  | { type: 'DELETE_PRODUCT'; payload: string }
  | { type: 'SET_SELECTED_PRODUCT'; payload: Producto | null }
  | { type: 'SET_LOADING_PRODUCTS'; payload: boolean }

  // Sales Actions
  | { type: 'SET_SALES'; payload: any[] }
  | { type: 'ADD_SALE'; payload: any }
  | { type: 'UPDATE_SALE'; payload: any }
  | { type: 'DELETE_SALE'; payload: string }
  | { type: 'SET_SELECTED_SALE'; payload: any | null }
  | { type: 'SET_LOADING_SALES'; payload: boolean }

  // UI Actions
  | { type: 'SET_CURRENT_VIEW'; payload: 'dashboard' | 'sales' | 'deliveries' | 'suppliers' }
  | { type: 'ADD_NOTIFICATION'; payload: Notification }
  | { type: 'REMOVE_NOTIFICATION'; payload: string }
  | { type: 'CLEAR_NOTIFICATIONS' }
  | { type: 'SET_LOADING'; payload: boolean };

// Initial State
const initialState: AppState = {
  // User/Auth
  user: null,
  isAuthenticated: false,

  // System
  systemHealth: null,
  isLoadingHealth: false,

  // Deliveries
  deliveries: [],
  selectedDelivery: null,
  isLoadingDeliveries: false,

  // Suppliers
  suppliers: [],
  selectedSupplier: null,
  isLoadingSuppliers: false,

  // Customers
  customers: [],
  selectedCustomer: null,
  isLoadingCustomers: false,

  // Products
  products: [],
  selectedProduct: null,
  isLoadingProducts: false,

  // Sales
  sales: [],
  selectedSale: null,
  isLoadingSales: false,

  // UI
  currentView: 'dashboard',
  notifications: [],
  isLoading: false,
};

// Reducer Function
function appReducer(state: AppState, action: AppAction): AppState {
  switch (action.type) {
    // Auth Cases
    case 'SET_USER':
      return { ...state, user: action.payload };
    case 'SET_AUTHENTICATED':
      return { ...state, isAuthenticated: action.payload };
    case 'LOGIN':
      return { ...state, user: action.payload, isAuthenticated: true };
    case 'LOGOUT':
      return { ...state, user: null, isAuthenticated: false };

    // System Cases
    case 'SET_SYSTEM_HEALTH':
      return { ...state, systemHealth: action.payload };
    case 'SET_LOADING_HEALTH':
      return { ...state, isLoadingHealth: action.payload };
    
    // Delivery Cases
    case 'SET_DELIVERIES':
      return { ...state, deliveries: action.payload };
    case 'ADD_DELIVERY':
      return { ...state, deliveries: [...state.deliveries, action.payload] };
    case 'UPDATE_DELIVERY':
      return {
        ...state,
        deliveries: state.deliveries.map(delivery =>
          delivery.id === action.payload.id ? action.payload : delivery
        ),
        selectedDelivery: state.selectedDelivery?.id === action.payload.id ? action.payload : state.selectedDelivery
      };
    case 'SET_SELECTED_DELIVERY':
      return { ...state, selectedDelivery: action.payload };
    case 'SET_LOADING_DELIVERIES':
      return { ...state, isLoadingDeliveries: action.payload };
    
    // Supplier Cases
    case 'SET_SUPPLIERS':
      return { ...state, suppliers: action.payload };
    case 'ADD_SUPPLIER':
      return { ...state, suppliers: [...state.suppliers, action.payload] };
    case 'UPDATE_SUPPLIER':
      return {
        ...state,
        suppliers: state.suppliers.map(supplier =>
          supplier.id === action.payload.id ? action.payload : supplier
        ),
        selectedSupplier: state.selectedSupplier?.id === action.payload.id ? action.payload : state.selectedSupplier
      };
    case 'SET_SELECTED_SUPPLIER':
      return { ...state, selectedSupplier: action.payload };
    case 'SET_LOADING_SUPPLIERS':
      return { ...state, isLoadingSuppliers: action.payload };

    // Customer Cases
    case 'SET_CUSTOMERS':
      return { ...state, customers: action.payload };
    case 'ADD_CUSTOMER':
      return { ...state, customers: [...state.customers, action.payload] };
    case 'UPDATE_CUSTOMER':
      return {
        ...state,
        customers: state.customers.map(customer =>
          customer.id === action.payload.id ? action.payload : customer
        ),
        selectedCustomer: state.selectedCustomer?.id === action.payload.id ? action.payload : state.selectedCustomer
      };
    case 'DELETE_CUSTOMER':
      return {
        ...state,
        customers: state.customers.filter(customer => customer.id !== action.payload),
        selectedCustomer: state.selectedCustomer?.id === action.payload ? null : state.selectedCustomer
      };
    case 'SET_SELECTED_CUSTOMER':
      return { ...state, selectedCustomer: action.payload };
    case 'SET_LOADING_CUSTOMERS':
      return { ...state, isLoadingCustomers: action.payload };

    // Product Cases
    case 'SET_PRODUCTS':
      return { ...state, products: action.payload };
    case 'ADD_PRODUCT':
      return { ...state, products: [...state.products, action.payload] };
    case 'UPDATE_PRODUCT':
      return {
        ...state,
        products: state.products.map(product =>
          product.id === action.payload.id ? action.payload : product
        ),
        selectedProduct: state.selectedProduct?.id === action.payload.id ? action.payload : state.selectedProduct
      };
    case 'DELETE_PRODUCT':
      return {
        ...state,
        products: state.products.filter(product => product.id !== action.payload),
        selectedProduct: state.selectedProduct?.id === action.payload ? null : state.selectedProduct
      };
    case 'SET_SELECTED_PRODUCT':
      return { ...state, selectedProduct: action.payload };
    case 'SET_LOADING_PRODUCTS':
      return { ...state, isLoadingProducts: action.payload };

    // Sales Cases
    case 'SET_SALES':
      return { ...state, sales: action.payload };
    case 'ADD_SALE':
      return { ...state, sales: [...state.sales, action.payload] };
    case 'UPDATE_SALE':
      return {
        ...state,
        sales: state.sales.map(sale =>
          sale.id === action.payload.id ? action.payload : sale
        ),
        selectedSale: state.selectedSale?.id === action.payload.id ? action.payload : state.selectedSale
      };
    case 'DELETE_SALE':
      return {
        ...state,
        sales: state.sales.filter(sale => sale.id !== action.payload),
        selectedSale: state.selectedSale?.id === action.payload ? null : state.selectedSale
      };
    case 'SET_SELECTED_SALE':
      return { ...state, selectedSale: action.payload };
    case 'SET_LOADING_SALES':
      return { ...state, isLoadingSales: action.payload };

    // UI Cases
    case 'SET_CURRENT_VIEW':
      return { ...state, currentView: action.payload };
    case 'ADD_NOTIFICATION':
      return { ...state, notifications: [...state.notifications, action.payload] };
    case 'REMOVE_NOTIFICATION':
      return {
        ...state,
        notifications: state.notifications.filter(n => n.id !== action.payload)
      };
    case 'CLEAR_NOTIFICATIONS':
      return { ...state, notifications: [] };
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    
    default:
      return state;
  }
}

// Context Creation
const AppContext = createContext<{
  state: AppState;
  dispatch: React.Dispatch<AppAction>;
} | undefined>(undefined);

// Provider Component
interface AppProviderProps {
  children: ReactNode;
}

export const AppProvider: React.FC<AppProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(appReducer, initialState);

  // Initialize user from localStorage on app start
  React.useEffect(() => {
    const storedUser = localStorage.getItem('user');
    const storedToken = localStorage.getItem('authToken');

    if (storedUser && storedToken) {
      try {
        const user = JSON.parse(storedUser);
        // Check if token is still valid (basic check)
        if (user.expiresAt && new Date(user.expiresAt) > new Date()) {
          dispatch({ type: 'LOGIN', payload: user });
        } else {
          // Token expired, clear storage
          localStorage.removeItem('user');
          localStorage.removeItem('authToken');
        }
      } catch (error) {
        console.error('Error parsing stored user:', error);
        localStorage.removeItem('user');
        localStorage.removeItem('authToken');
      }
    }
    // No default user - require proper login
  }, []);

  return (
    <AppContext.Provider value={{ state, dispatch }}>
      {children}
    </AppContext.Provider>
  );
};

// Custom Hook
export const useAppContext = () => {
  const context = useContext(AppContext);
  if (context === undefined) {
    throw new Error('useAppContext must be used within an AppProvider');
  }
  return context;
};

// Action Creators (Helper Functions)
export const createNotification = (
  type: Notification['type'],
  title: string,
  message: string,
  autoHide: boolean = true
): Notification => ({
  id: Date.now().toString() + Math.random().toString(36).substr(2, 9),
  type,
  title,
  message,
  timestamp: new Date(),
  autoHide,
});

// React App Component Filter - Only show components used in React app
const REACT_APP_COMPONENTS = [
  { key: 'ventas', names: ['Ventas', 'Sales', 'RF2'] },
  { key: 'entregas', names: ['Entregas', 'Deliveries', 'RF4'] },
  { key: 'proveedores', names: ['Proveedores', 'Suppliers', 'RF5'] }
];

// Selectors (Helper Functions)
export const getHealthyComponentsCount = (systemHealth: SystemHealthResponse | null): { healthy: number; total: number } => {
  if (!systemHealth?.componentsHealth) {
    return { healthy: 0, total: 0 };
  }

  // Filter to only show React app components
  const reactComponents = Object.values(systemHealth.componentsHealth).filter(component =>
    REACT_APP_COMPONENTS.some(reactComp =>
      reactComp.names.some(name =>
        component.componentName.toLowerCase().includes(name.toLowerCase()) ||
        name.toLowerCase().includes(component.componentName.toLowerCase())
      )
    )
  );

  return {
    healthy: reactComponents.filter(c => c.isHealthy).length,
    total: reactComponents.length
  };
};

// Get filtered components for React app
export const getReactAppComponents = (systemHealth: SystemHealthResponse | null): Record<string, ComponentHealth> => {
  if (!systemHealth?.componentsHealth) {
    console.log('No system health data available');
    return {};
  }

  const filteredComponents: Record<string, ComponentHealth> = {};

  Object.entries(systemHealth.componentsHealth).forEach(([key, component]) => {
    if (REACT_APP_COMPONENTS.some(reactComp =>
      reactComp.names.some(name =>
        component.componentName.toLowerCase().includes(name.toLowerCase()) ||
        name.toLowerCase().includes(component.componentName.toLowerCase())
      )
    )) {
      filteredComponents[key] = component;
      console.log(`Added React component: ${component.componentName} (${key})`);
    }
  });

  console.log('Filtered React components:', Object.keys(filteredComponents));
  return filteredComponents;
};

export const getPendingDeliveries = (deliveries: Entrega[]): Entrega[] => {
  return deliveries.filter(delivery => 
    delivery.estado === 'Programada' || delivery.estado === 'En Transito'
  );
};

export const getActiveSuppliers = (suppliers: Proveedor[]): Proveedor[] => {
  return suppliers.filter(supplier => supplier.activo);
};

export const getSuppliersByCategory = (suppliers: Proveedor[], category: string): Proveedor[] => {
  return suppliers.filter(supplier => supplier.categoria === category);
};

// Custom hook for authentication
export const useAuth = () => {
  const { state, dispatch } = useAppContext();

  const login = useCallback((userData: Omit<User, 'isAuthenticated'>) => {
    const user: User = { ...userData, isAuthenticated: true };
    dispatch({ type: 'LOGIN', payload: user });
    // Store in localStorage for persistence
    localStorage.setItem('user', JSON.stringify(user));
    if (userData.token) {
      localStorage.setItem('authToken', userData.token);
    }
  }, [dispatch]);

  const logout = useCallback(async () => {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        // Call logout API to invalidate token on server
        await import('../services/apiService').then(({ apiService }) =>
          apiService.logout(token)
        );
      } catch (error) {
        console.error('Error during logout:', error);
      }
    }

    dispatch({ type: 'LOGOUT' });
    localStorage.removeItem('user');
    localStorage.removeItem('authToken');
  }, [dispatch]);

  const getCurrentUser = useCallback((): User | null => {
    return state.user;
  }, [state.user]);

  const getCurrentUserId = useCallback((): string | null => {
    return state.user?.id || null;
  }, [state.user]);

  return {
    user: state.user,
    isAuthenticated: state.isAuthenticated,
    login,
    logout,
    getCurrentUser,
    getCurrentUserId
  };
};
