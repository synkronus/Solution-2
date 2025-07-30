import React, { useEffect } from 'react';

// PrimeReact Imports
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { Tag } from 'primereact/tag';
import { ProgressSpinner } from 'primereact/progressspinner';
import { Divider } from 'primereact/divider';
import { Toast } from 'primereact/toast';

// Context and Hooks
import { AppProvider, useAppContext, useAuth, getHealthyComponentsCount, getReactAppComponents } from './context/AppContext';
import { useApi } from './hooks/useApi';

// Components
import DeliveryComponent from './components/DeliveryComponent';
import SupplierComponent from './components/SupplierComponent';
import SalesComponent from './components/SalesComponent';
import LoginComponent from './components/LoginComponent';

// Styles
import './App.css';
import 'primereact/resources/themes/lara-light-blue/theme.css';
import 'primereact/resources/primereact.min.css';
import 'primeicons/primeicons.css';

// Main App Content Component
const AppContent: React.FC = () => {
  const { state, dispatch } = useAppContext();
  const { user, isAuthenticated, logout } = useAuth();
  const { loadSystemHealth, testConnection } = useApi();

  useEffect(() => {
    loadSystemHealth();
  }, [loadSystemHealth]);

  const switchView = (view: 'dashboard' | 'sales' | 'deliveries' | 'suppliers') => {
    dispatch({ type: 'SET_CURRENT_VIEW', payload: view });
  };

  const hasAccess = (feature: string): boolean => {
    if (!user) return false;

    switch (feature) {
      case 'dashboard':
        return true; // All authenticated users can see dashboard
      case 'sales':
        return user.role === 'sales_rep' || user.role === 'admin';
      case 'deliveries':
        return user.role === 'delivery_manager' || user.role === 'sales_rep' || user.role === 'admin';
      case 'suppliers':
        return user.role === 'inventory_manager' || user.role === 'admin';
      default:
        return false;
    }
  };

  const getHealthStatusClass = (): string => {
    if (!state.systemHealth) return 'unknown';
    return state.systemHealth.isHealthy ? 'healthy' : 'degraded';
  };

  const handleLogout = async () => {
    await logout();
  };

  // Show login component if user is not authenticated
  if (!isAuthenticated) {
    return <LoginComponent />;
  }

  return (
    <div className="app-container">
      <Toast />

      {/* Header */}
      <header className="app-header">
        <div className="header-content">
          <div className="logo-section">
            <h1 className="app-title">🏪 PoliMarket React Client - CBSE Architecture</h1>
            <p className="app-subtitle">Component-Based Software Engineering Implementation</p>
          </div>

          {/* User Info Section */}
          <div className="user-info-section">
            <div className="user-details">
              <span className="user-name">
                <i className="pi pi-user"></i>
                {user?.nombre && user?.apellido
                  ? `${user.nombre} ${user.apellido}`
                  : user?.username}
              </span>
              <span className="user-role">
                {user?.role === 'sales_rep' ? 'Vendedor' :
                 user?.role === 'delivery_manager' ? 'Gestor de Entregas' :
                 user?.role === 'admin' ? 'Administrador' : user?.role}
              </span>
            </div>
            <Button
              icon="pi pi-sign-out"
              className="p-button-text p-button-plain"
              onClick={handleLogout}
              tooltip="Cerrar Sesión"
              tooltipOptions={{ position: 'bottom' }}
            />
          </div>

          {/* System Health Indicator */}
          <div className={`health-indicator ${getHealthStatusClass()}`}>
            <div className="health-icon">
              {state.isLoadingHealth ? (
                <ProgressSpinner style={{ width: '2rem', height: '2rem' }} strokeWidth="4" />
              ) : (
                <span>
                  {state.systemHealth?.isHealthy ? '✅' :
                   state.systemHealth ? '⚠️' : '❌'}
                </span>
              )}
            </div>
            <div className="health-info">
              <div className="health-status">
                {state.isLoadingHealth ? 'Checking...' : (state.systemHealth?.overallStatus || 'Unknown')}
              </div>
              {state.systemHealth && (
                <div className="health-details">
                  {getHealthyComponentsCount(state.systemHealth).healthy}/
                  {getHealthyComponentsCount(state.systemHealth).total} Components Healthy
                </div>
              )}
            </div>
            <Button
              icon="pi pi-refresh"
              onClick={loadSystemHealth}
              disabled={state.isLoadingHealth}
              loading={state.isLoadingHealth}
              className="p-button-rounded p-button-text p-button-sm"
              tooltip="Refresh System Health"
            />
          </div>
        </div>
      </header>

      {/* Navigation */}
      <nav className="app-navigation">
        <div className="nav-content">
          {hasAccess('dashboard') && (
            <Button
              label="Dashboard"
              icon="pi pi-chart-bar"
              className={`nav-btn ${state.currentView === 'dashboard' ? 'active' : ''}`}
              onClick={() => switchView('dashboard')}
            />
          )}
          {hasAccess('sales') && (
            <Button
              label="RF2: Sales"
              icon="pi pi-shopping-cart"
              className={`nav-btn ${state.currentView === 'sales' ? 'active' : ''}`}
              onClick={() => switchView('sales')}
            />
          )}
          {hasAccess('deliveries') && (
            <Button
              label="RF4: Deliveries"
              icon="pi pi-truck"
              className={`nav-btn ${state.currentView === 'deliveries' ? 'active' : ''}`}
              onClick={() => switchView('deliveries')}
            />
          )}
          {hasAccess('suppliers') && (
            <Button
              label="RF5: Suppliers"
              icon="pi pi-building"
              className={`nav-btn ${state.currentView === 'suppliers' ? 'active' : ''}`}
              onClick={() => switchView('suppliers')}
            />
          )}
        </div>
      </nav>

      {/* Main Content */}
      <main className="app-main">
        {state.currentView === 'dashboard' && (
          <div className="dashboard-view">
            <div className="dashboard-header">
              <h2>🏠 System Dashboard</h2>
              <p>Overview of PoliMarket CBSE System Components</p>
            </div>

            {/* System Overview Cards */}
            <div className="overview-grid">
              {/* System Health Card */}
              <Card className={`health-card fade-in ${getHealthStatusClass()}`}>
                <div className="card-header-content">
                  <h3><i className="pi pi-heart"></i> System Health</h3>
                  <Tag
                    value={state.systemHealth?.overallStatus || 'Unknown'}
                    severity={state.systemHealth?.isHealthy ? 'success' : 'danger'}
                  />
                </div>

                {state.systemHealth && (
                  <>
                    <div className="health-metrics">
                      <div className="metric-grid">
                        <div className="metric">
                          <span className="metric-label">Components</span>
                          <span className="metric-value">{getHealthyComponentsCount(state.systemHealth).total}</span>
                        </div>
                        <div className="metric">
                          <span className="metric-label">Healthy</span>
                          <span className="metric-value healthy">{getHealthyComponentsCount(state.systemHealth).healthy}</span>
                        </div>
                        <div className="metric">
                          <span className="metric-label">Issues</span>
                          <span className="metric-value issues">
                            {getHealthyComponentsCount(state.systemHealth).total - getHealthyComponentsCount(state.systemHealth).healthy}
                          </span>
                        </div>
                      </div>
                    </div>

                    <Divider />

                    <div className="components-list">
                      {state.systemHealth && Object.entries(getReactAppComponents(state.systemHealth)).map(([key, component]) => (
                        <div key={key} className="component-status">
                          <span className="component-name">{component.componentName}</span>
                          <Tag
                            value={component.isHealthy ? 'Healthy' : 'Error'}
                            severity={component.isHealthy ? 'success' : 'danger'}
                          />
                        </div>
                      ))}
                    </div>
                  </>
                )}
              </Card>

              {/* RF4 Deliveries Card */}
              <Card className="rf-card rf4-card fade-in">
                <div className="card-header-content">
                  <h3><i className="pi pi-truck"></i> RF4: Deliveries</h3>
                  <Tag value="Active" severity="success" />
                </div>

                <p>Delivery management and logistics tracking system</p>
                <ul className="feature-list">
                  <li><i className="pi pi-check text-green-500"></i> Delivery Scheduling</li>
                  <li><i className="pi pi-check text-green-500"></i> Status Tracking</li>
                  <li><i className="pi pi-check text-green-500"></i> Route Optimization</li>
                  <li><i className="pi pi-check text-green-500"></i> Confirmation System</li>
                </ul>

                <Button
                  label="Open Deliveries"
                  icon="pi pi-arrow-right"
                  onClick={() => switchView('deliveries')}
                  className="w-full mt-3"
                />
              </Card>

              {/* RF5 Suppliers Card */}
              <Card className="rf-card rf5-card fade-in">
                <div className="card-header-content">
                  <h3><i className="pi pi-building"></i> RF5: Suppliers</h3>
                  <Tag value="Active" severity="success" />
                </div>

                <p>Supplier management and performance tracking system</p>
                <ul className="feature-list">
                  <li><i className="pi pi-check text-green-500"></i> Supplier Registration</li>
                  <li><i className="pi pi-check text-green-500"></i> Performance Metrics</li>
                  <li><i className="pi pi-check text-green-500"></i> Category Management</li>
                  <li><i className="pi pi-check text-green-500"></i> Evaluation System</li>
                </ul>

                <Button
                  label="Open Suppliers"
                  icon="pi pi-arrow-right"
                  onClick={() => switchView('suppliers')}
                  className="w-full mt-3"
                />
              </Card>

              {/* Architecture Info Card */}
              {/* <Card className="architecture-card fade-in">
                <div className="card-header-content">
                  <h3><i className="pi pi-cog"></i> CBSE Architecture</h3>
                  <Tag value="Implemented" severity="success" />
                </div>

                <p>Component-Based Software Engineering principles applied</p>
                <ul className="feature-list">
                  <li><i className="pi pi-check text-green-500"></i> Separation of Concerns</li>
                  <li><i className="pi pi-check text-green-500"></i> Interface-Based Design</li>
                  <li><i className="pi pi-check text-green-500"></i> Loose Coupling</li>
                  <li><i className="pi pi-check text-green-500"></i> High Reusability</li>
                </ul>

                <Divider />

                <div className="architecture-stats">
                  <div className="stat-grid">
                    <div className="stat">
                      <span className="stat-value">6</span>
                      <span className="stat-label">Components</span>
                    </div>
                    <div className="stat">
                      <span className="stat-value">32</span>
                      <span className="stat-label">Endpoints</span>
                    </div>
                  </div>
                </div>
              </Card> */}
            </div>

            {/* API Information */}
            <Card className="api-info-section fade-in">
              <div className="card-header-content">
                <h3><i className="pi pi-link"></i> API Connection Information</h3>
                <Button
                  label="Test Connection"
                  icon="pi pi-wifi"
                  onClick={testConnection}
                  className="p-button-outlined p-button-sm"
                />
              </div>

              <div className="api-details">
                <div className="api-item">
                  <span className="api-label"><i className="pi pi-server"></i> Base URL:</span>
                  <span className="api-value">http://localhost:5001/api</span>
                </div>
                <div className="api-item">
                  <span className="api-label"><i className="pi pi-book"></i> Swagger UI:</span>
                  <span className="api-value">
                    <a href="http://localhost:5001" target="_blank" rel="noopener noreferrer" className="api-link">
                      http://localhost:5001 <i className="pi pi-external-link"></i>
                    </a>
                  </span>
                </div>
                <div className="api-item">
                  <span className="api-label"><i className="pi pi-heart"></i> Health Check:</span>
                  <span className="api-value">
                    <a href="http://localhost:5001/api/Integracion/health" target="_blank" rel="noopener noreferrer" className="api-link">
                      /api/Integracion/health <i className="pi pi-external-link"></i>
                    </a>
                  </span>
                </div>
              </div>
            </Card>
          </div>
        )}

        {state.currentView === 'sales' && hasAccess('sales') && <SalesComponent />}
        {state.currentView === 'deliveries' && hasAccess('deliveries') && <DeliveryComponent />}
        {state.currentView === 'suppliers' && hasAccess('suppliers') && <SupplierComponent />}
      </main>

      {/* Footer */}
      <footer className="app-footer">
        <div className="footer-content">
          <p>&copy; 2024 PoliMarket CBSE - Component-Based Software Engineering Implementation</p>
          <p>React Client consuming RF4 (Deliveries) and RF5 (Suppliers) components</p>
        </div>
      </footer>
    </div>
  );
};

// Main App Component with Provider
const App: React.FC = () => {
  return (
    <AppProvider>
      <AppContent />
    </AppProvider>
  );
};

export default App;

