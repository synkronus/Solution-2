import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { ApiService, SystemHealthResponse, ComponentHealth } from './services/api.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'PoliMarket Angular Client - CBSE Architecture';

  currentView: 'dashboard' | 'authorization' | 'inventory-dashboard' | 'suppliers' = 'dashboard';
  systemHealth: SystemHealthResponse | null = null;
  isLoadingHealth = false;

  // HR Authentication
  currentHRUser: any = null;
  hrLoginCode: string = '';
  isLoggingIn = false;

  // Angular App Component Filter - Only show components used in Angular app
  private readonly ANGULAR_APP_COMPONENTS = [
    { key: 'autorizacion', names: ['Autorizacion', 'Authorization', 'RF1'] },
    { key: 'hr', names: ['HR', 'Recursos Humanos', 'Human Resources', 'RH', 'Empleados'] },
    { key: 'inventario', names: ['Inventario', 'nventario', 'Inventory', 'RF3'] }
  ];

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadSystemHealth();
    this.loadHREmployees();
  }

  switchView(view: 'dashboard' | 'authorization' | 'inventory-dashboard' | 'suppliers') {
    // Only allow navigation if HR user is logged in
    if (!this.currentHRUser) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acceso Restringido',
        detail: 'Debe iniciar sesión como empleado de RH para acceder a esta aplicación'
      });
      return;
    }
    this.currentView = view;
  }

  // HR Authentication Methods
  hrEmployees: any[] = [];

  loadHREmployees() {
    this.apiService.getHREmployees().subscribe({
      next: (employees) => {
        this.hrEmployees = employees;
      },
      error: (error) => {
        console.error('Error loading HR employees:', error);
      }
    });
  }

  loginHR() {
    if (!this.hrLoginCode.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Código Requerido',
        detail: 'Por favor ingrese su código de empleado RH'
      });
      return;
    }

    this.isLoggingIn = true;

    // Find HR employee by code
    const hrEmployee = this.hrEmployees.find(emp => emp.id === this.hrLoginCode.trim());

    setTimeout(() => {
      if (hrEmployee) {
        this.currentHRUser = hrEmployee;
        this.messageService.add({
          severity: 'success',
          summary: 'Acceso Autorizado',
          detail: `Bienvenido ${hrEmployee.nombre}`
        });
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Acceso Denegado',
          detail: 'Código de empleado RH no válido. Solo personal de RH puede acceder a esta aplicación.'
        });
      }
      this.isLoggingIn = false;
    }, 1000);
  }

  logoutHR() {
    this.currentHRUser = null;
    this.hrLoginCode = '';
    this.currentView = 'dashboard';
    this.messageService.add({
      severity: 'info',
      summary: 'Sesión Cerrada',
      detail: 'Ha cerrado sesión exitosamente'
    });
  }

  loadSystemHealth() {
    this.isLoadingHealth = true;
    this.apiService.getSystemHealth().subscribe({
      next: (health) => {
        this.systemHealth = health;
        this.isLoadingHealth = false;
      },
      error: (error) => {
        console.error('Error loading system health:', error);
        this.isLoadingHealth = false;
      }
    });
  }

  getHealthStatusClass(): string {
    if (!this.systemHealth) return 'unknown';
    return this.systemHealth.isHealthy ? 'healthy' : 'degraded';
  }

  getComponentHealthCount(): { healthy: number; total: number } {
    if (!this.systemHealth?.componentsHealth) {
      return { healthy: 0, total: 0 };
    }

    // Filter to only show Angular app components
    const angularComponents = Object.values(this.systemHealth.componentsHealth).filter(component =>
      this.ANGULAR_APP_COMPONENTS.some(angularComp =>
        angularComp.names.some(name =>
          component.componentName.toLowerCase().includes(name.toLowerCase()) ||
          name.toLowerCase().includes(component.componentName.toLowerCase())
        )
      )
    );

    return {
      healthy: angularComponents.filter(c => c.isHealthy).length,
      total: angularComponents.length
    };
  }

  // Get filtered components for Angular app
  getAngularAppComponents(): { [key: string]: ComponentHealth } {
    if (!this.systemHealth?.componentsHealth) {
      return {};
    }

    const filteredComponents: { [key: string]: ComponentHealth } = {};

    Object.entries(this.systemHealth.componentsHealth).forEach(([key, component]) => {
      if (this.ANGULAR_APP_COMPONENTS.some(angularComp =>
        angularComp.names.some(name =>
          component.componentName.toLowerCase().includes(name.toLowerCase()) ||
          name.toLowerCase().includes(component.componentName.toLowerCase())
        )
      )) {
        filteredComponents[key] = component;
      }
    });

    return filteredComponents;
  }
}