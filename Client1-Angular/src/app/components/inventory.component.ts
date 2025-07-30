import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import {
  ApiService,
  AvailabilityCheckRequest,
  DisponibilidadInventario,
  StockUpdateRequest,
  StockOperationResponse,
  AlertaStock
} from '../services/api.service';

@Component({
  selector: 'app-inventory',
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.scss']
})
export class InventoryComponent implements OnInit {
  availabilityRequest: AvailabilityCheckRequest = {
    idProducto: '',
    cantidadRequerida: 1
  };

  stockUpdateRequest: StockUpdateRequest = {
    idProducto: '',
    cantidad: 0,
    tipoMovimiento: '',
    motivo: '',
    usuarioResponsable: ''
  };

  availabilityResult: DisponibilidadInventario | null = null;
  quickStockResult: DisponibilidadInventario | null = null;
  updateResult: StockOperationResponse | null = null;
  stockAlerts: AlertaStock[] = [];

  isCheckingAvailability: boolean = false;
  isQuickChecking: boolean = false;
  isUpdatingStock: boolean = false;
  isGeneratingAlerts: boolean = false;

  predefinedProducts = [
    { label: 'Laptop Dell', value: 'PROD001' },
    { label: 'Mouse Inalámbrico', value: 'PROD002' },
    { label: 'Teclado Mecánico', value: 'PROD003' }
  ];

  movementTypes = [
    { label: 'Seleccione tipo...', value: '' },
    { label: 'Entrada (Aumentar Stock)', value: 'entrada' },
    { label: 'Salida (Reducir Stock)', value: 'salida' },
    { label: 'Ajuste (Establecer Stock)', value: 'ajuste' }
  ];

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.generateStockAlerts();
  }

  checkAvailability() {
    this.isCheckingAvailability = true;
    this.availabilityResult = null;

    this.apiService.checkAvailability(this.availabilityRequest).subscribe({
      next: (result) => {
        this.availabilityResult = result;
        this.isCheckingAvailability = false;
        
        this.messageService.add({
          severity: result.disponibleParaVenta ? 'success' : 'warn',
          summary: result.disponibleParaVenta ? 'Producto Disponible' : 'Producto No Disponible',
          detail: `Stock disponible: ${result.stockDisponible} unidades`
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Error al consultar disponibilidad'
        });
        this.isCheckingAvailability = false;
      }
    });
  }

  quickStockCheck(productId: string) {
    this.isQuickChecking = true;
    this.quickStockResult = null;

    this.apiService.getCurrentStock(productId).subscribe({
      next: (result) => {
        this.quickStockResult = result;
        this.isQuickChecking = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Error al consultar stock'
        });
        this.isQuickChecking = false;
      }
    });
  }

  updateStock() {
    this.isUpdatingStock = true;
    this.updateResult = null;

    this.apiService.updateStock(this.stockUpdateRequest).subscribe({
      next: (result) => {
        this.updateResult = result;
        this.messageService.add({
          severity: 'success',
          summary: 'Éxito',
          detail: 'Stock actualizado exitosamente'
        });
        this.isUpdatingStock = false;
        this.resetUpdateForm();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Error al actualizar stock'
        });
        this.isUpdatingStock = false;
      }
    });
  }

  generateStockAlerts() {
    this.isGeneratingAlerts = true;

    this.apiService.generateStockAlerts().subscribe({
      next: (alerts) => {
        this.stockAlerts = alerts;
        this.isGeneratingAlerts = false;
        
        if (alerts.length > 0) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Alertas de Stock',
            detail: `Se encontraron ${alerts.length} alertas de stock`
          });
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.message || 'Error al generar alertas'
        });
        this.isGeneratingAlerts = false;
      }
    });
  }

  getStockStatusSeverity(stock: number): 'success' | 'warning' | 'danger' {
    if (stock > 50) return 'success';
    if (stock > 10) return 'warning';
    return 'danger';
  }

  getStockStatusText(stock: number): string {
    if (stock > 50) return 'Alto';
    if (stock > 10) return 'Medio';
    return 'Bajo';
  }

  getAlertSeverity(tipoAlerta: string): 'success' | 'warning' | 'danger' {
    switch (tipoAlerta.toLowerCase()) {
      case 'stockbajo': return 'danger';
      case 'stockagotado': return 'danger';
      default: return 'warning';
    }
  }

  resetUpdateForm() {
    this.stockUpdateRequest = {
      idProducto: '',
      cantidad: 0,
      tipoMovimiento: '',
      motivo: '',
      usuarioResponsable: ''
    };
  }

  trackByAlert(_index: number, alert: AlertaStock): string {
    return alert.id;
  }
}
