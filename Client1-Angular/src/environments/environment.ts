// PoliMarket Angular App Environment Configuration
// Development Environment

export const environment = {
  production: false,
  
  // API Configuration
  apiBaseUrl: 'http://localhost:5001/api',
  apiTimeout: 10000,
  
  // Application Configuration
  appName: 'PoliMarket Business Management',
  appVersion: '1.0.0',
  environment: 'development',
  
  // Feature Flags
  enableDebugLogs: true,
  enableMockData: false,
  enableNotifications: true,
  enablePerformanceMonitoring: false,
  
  // UI Configuration
  defaultPageSize: 10,
  maxUploadSize: 5242880, // 5MB
  theme: 'light',
  
  // Business Rules
  maxDiscountPercentage: 20,
  taxRate: 0.19,
  currency: 'COP',
  currencySymbol: '$',
  
  // Authorization Configuration
  maxLoginAttempts: 3,
  tokenExpirationMinutes: 480,
  requireEmailVerification: true,
  
  // Inventory Configuration
  lowStockThreshold: 10,
  autoReorderEnabled: true,
  reorderQuantity: 100,
  
  // Pagination
  defaultHRPageSize: 15,
  defaultSellersPageSize: 20,
  defaultInventoryPageSize: 25,
  
  // Timeouts and Intervals
  healthCheckInterval: 30000,
  autoRefreshInterval: 60000,
  sessionTimeout: 1800000, // 30 minutes
  
  // Development Tools
  showAngularDevTools: true,
  enableStrictMode: true
};
