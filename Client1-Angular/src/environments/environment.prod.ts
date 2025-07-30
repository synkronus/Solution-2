// PoliMarket Angular App Environment Configuration
// Production Environment

export const environment = {
  production: true,
  
  // API Configuration
  apiBaseUrl: 'https://api.polimarket.com/api',
  apiTimeout: 15000,
  
  // Application Configuration
  appName: 'PoliMarket Business Management',
  appVersion: '1.0.0',
  environment: 'production',
  
  // Feature Flags
  enableDebugLogs: false,
  enableMockData: false,
  enableNotifications: true,
  enablePerformanceMonitoring: true,
  
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
  healthCheckInterval: 60000,
  autoRefreshInterval: 120000,
  sessionTimeout: 1800000, // 30 minutes
  
  // Development Tools (disabled in production)
  showAngularDevTools: false,
  enableStrictMode: false
};
