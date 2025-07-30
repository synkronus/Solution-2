import { Component, OnInit } from '@angular/core';
import { ApiService } from '../services/api.service';
import { ConfirmationService, MessageService } from 'primeng/api';

interface HREmployee {
  id: string;
  nombre: string;
  email: string;
  departamento: string;
  activo: boolean;
}

interface SellerRequest {
  codigoVendedor: string;
  nombreVendedor: string;
  empleadoRHId: string;
  departamento: string;
  territorio?: string;
  fechaSolicitud: Date;
  estado: 'Pendiente' | 'Aprobado' | 'Rechazado';
}

interface AuthorizedSeller {
  codigoVendedor: string;
  nombreVendedor: string;
  empleadoRH: string;
  departamento: string;
  fechaAutorizacion: Date;
  activo: boolean;
}

@Component({
  selector: 'app-hr-dashboard',
  templateUrl: './hr-dashboard.component.html',
  styleUrls: ['./hr-dashboard.component.scss']
})
export class HRDashboardComponent implements OnInit {
  // Authentication
  currentHREmployee: HREmployee | null = null;
  hrLoginCode: string = '';
  isLoggingIn: boolean = false;
  isProcessing: boolean = false;

  // Data
  hrEmployees: HREmployee[] = [];
  pendingRequests: SellerRequest[] = [];
  authorizedSellers: AuthorizedSeller[] = [];

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadRealData();
  }

  loadRealData() {
    this.loadHREmployees();
    this.loadPendingRequests();
    this.loadAuthorizedSellers();
  }

  loadHREmployees() {
    // Load real HR employees from API
    this.apiService.getHREmployees().subscribe({
      next: (employees) => {
        this.hrEmployees = employees.map(emp => ({
          id: emp.id,
          nombre: emp.nombre,
          email: emp.email,
          departamento: emp.departamento,
          activo: emp.activo
        }));
      },
      error: (error) => {
        console.error('Error loading HR employees:', error);
        // Fallback to mock data if API fails
        this.loadMockHREmployees();
      }
    });
  }

  loadMockHREmployees() {
    this.hrEmployees = [
      {
        id: 'HR001',
        nombre: 'Ana García Rodríguez',
        email: 'ana.garcia@polimarket.com',
        departamento: 'Recursos Humanos',
        activo: true
      },
      {
        id: 'HR002',
        nombre: 'Carlos López Martínez',
        email: 'carlos.lopez@polimarket.com',
        departamento: 'Recursos Humanos',
        activo: true
      }
    ];
  }

  loadPendingRequests() {
    // Load sellers pending authorization
    this.apiService.getPendingSellers().subscribe({
      next: (sellers) => {
        this.pendingRequests = sellers.map(seller => ({
          codigoVendedor: seller.codigoVendedor,
          nombreVendedor: seller.nombre,
          empleadoRHId: '',
          departamento: seller.territorio,
          territorio: seller.territorio,
          fechaSolicitud: new Date(), // Use current date since fechaCreacion doesn't exist in Vendedor interface
          estado: 'Pendiente' as const
        }));
      },
      error: (error) => {
        console.error('Error loading pending requests:', error);
        // Keep empty array if API fails
        this.pendingRequests = [];
      }
    });
  }

  loadAuthorizedSellers() {
    // Load authorized sellers
    this.apiService.getAuthorizedVendedores().subscribe({
      next: (sellers) => {
        this.authorizedSellers = sellers.map(seller => ({
          codigoVendedor: seller.codigoVendedor,
          nombreVendedor: seller.nombre,
          empleadoRH: seller.empleadoRHAutorizo,
          departamento: seller.territorio,
          fechaAutorizacion: new Date(seller.fechaAutorizacion),
          activo: true // Default to true since Vendedor interface doesn't have activo property
        }));
      },
      error: (error) => {
        console.error('Error loading authorized sellers:', error);
        // Fallback to mock data if API fails
        this.loadMockData();
      }
    });
  }

  loadMockData() {
    // Mock Pending Requests
    this.pendingRequests = [
      {
        codigoVendedor: 'V001',
        nombreVendedor: 'Juan Pérez',
        empleadoRHId: 'HR001',
        departamento: 'Ventas',
        territorio: 'Bogotá',
        fechaSolicitud: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
        estado: 'Pendiente'
      },
      {
        codigoVendedor: 'V002',
        nombreVendedor: 'Ana Martínez',
        empleadoRHId: 'HR001',
        departamento: 'Ventas',
        territorio: 'Medellín',
        fechaSolicitud: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000),
        estado: 'Pendiente'
      },
      {
        codigoVendedor: 'V003',
        nombreVendedor: 'Luis Rodríguez',
        empleadoRHId: 'HR002',
        departamento: 'Ventas',
        territorio: 'Cali',
        fechaSolicitud: new Date(),
        estado: 'Pendiente'
      }
    ];

    // Mock Authorized Sellers
    this.authorizedSellers = [
      {
        codigoVendedor: 'V100',
        nombreVendedor: 'Pedro Sánchez',
        empleadoRH: 'HR001',
        departamento: 'Ventas',
        fechaAutorizacion: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000),
        activo: true
      },
      {
        codigoVendedor: 'V101',
        nombreVendedor: 'Laura García',
        empleadoRH: 'HR002',
        departamento: 'Ventas',
        fechaAutorizacion: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000),
        activo: true
      }
    ];
  }

  loginHR() {
    if (!this.hrLoginCode.trim()) return;

    this.isLoggingIn = true;
    
    // Simulate API call
    setTimeout(() => {
      const hrEmployee = this.hrEmployees.find(emp => emp.id === this.hrLoginCode);
      if (hrEmployee) {
        this.currentHREmployee = hrEmployee;
      } else {
        alert('Código de empleado RH no válido');
      }
      this.isLoggingIn = false;
    }, 1000);
  }

  logoutHR() {
    this.currentHREmployee = null;
    this.hrLoginCode = '';
  }

  approveSeller(request: SellerRequest) {
    this.confirmationService.confirm({
      message: `¿Está seguro de que desea aprobar la solicitud del vendedor ${request.nombreVendedor}?`,
      header: 'Confirmar Aprobación',
      icon: 'pi pi-check-circle',
      acceptLabel: 'Sí, Aprobar',
      rejectLabel: 'Cancelar',
      accept: () => {
        this.isProcessing = true;

        // Create authorization request
        const authRequest = {
          codigoVendedor: request.codigoVendedor,
          empleadoRH: this.currentHREmployee?.id || 'HR001',
          nombre: request.nombreVendedor,
          territorio: request.territorio || 'Sin asignar',
          comision: 5.5 // Default commission
        };

        // Call real API to authorize seller
        this.apiService.authorizeVendedor(authRequest).subscribe({
          next: (result) => {
            this.messageService.add({
              severity: 'success',
              summary: 'Aprobación Exitosa',
              detail: `Vendedor ${request.nombreVendedor} ha sido autorizado exitosamente`
            });

            // Remove from pending and refresh data
            this.pendingRequests = this.pendingRequests.filter(r => r.codigoVendedor !== request.codigoVendedor);
            this.loadAuthorizedSellers();
            this.isProcessing = false;
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error en Aprobación',
              detail: error.message || 'Error al aprobar vendedor'
            });
            this.isProcessing = false;
          }
        });

        // Simulate API call
        setTimeout(() => {
          // Remove from pending requests
          this.pendingRequests = this.pendingRequests.filter(r => r.codigoVendedor !== request.codigoVendedor);

          // Add to authorized sellers
          this.authorizedSellers.push({
            codigoVendedor: request.codigoVendedor,
            nombreVendedor: request.nombreVendedor,
            empleadoRH: this.currentHREmployee!.id,
            departamento: request.departamento,
            fechaAutorizacion: new Date(),
            activo: true
          });

          this.isProcessing = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Aprobación Exitosa',
            detail: `Vendedor ${request.nombreVendedor} autorizado exitosamente`
          });
        }, 1000);
      }
    });
  }

  rejectSeller(request: SellerRequest) {
    this.confirmationService.confirm({
      message: `¿Está seguro de que desea rechazar la solicitud del vendedor ${request.nombreVendedor}? Esta acción no se puede deshacer.`,
      header: 'Confirmar Rechazo',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, Rechazar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.isProcessing = true;

        // Simulate API call
        setTimeout(() => {
          // Remove from pending requests
          this.pendingRequests = this.pendingRequests.filter(r => r.codigoVendedor !== request.codigoVendedor);

          this.isProcessing = false;
          this.messageService.add({
            severity: 'warn',
            summary: 'Solicitud Rechazada',
            detail: `Solicitud de ${request.nombreVendedor} rechazada`
          });
        }, 1000);
      }
    });
  }

  toggleSellerStatus(seller: AuthorizedSeller) {
    const action = seller.activo ? 'desactivar' : 'activar';
    const actionPast = seller.activo ? 'desactivado' : 'activado';

    this.confirmationService.confirm({
      message: `¿Está seguro de que desea ${action} al vendedor ${seller.nombreVendedor}?`,
      header: `Confirmar ${action.charAt(0).toUpperCase() + action.slice(1)}ación`,
      icon: seller.activo ? 'pi pi-ban' : 'pi pi-check-circle',
      acceptLabel: `Sí, ${action.charAt(0).toUpperCase() + action.slice(1)}`,
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: seller.activo ? 'p-button-warning' : 'p-button-success',
      accept: () => {
        seller.activo = !seller.activo;
        this.messageService.add({
          severity: seller.activo ? 'success' : 'warn',
          summary: `Vendedor ${actionPast.charAt(0).toUpperCase() + actionPast.slice(1)}`,
          detail: `Vendedor ${seller.nombreVendedor} ${actionPast} exitosamente`
        });
      }
    });
  }
}
