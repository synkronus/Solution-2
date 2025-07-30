import { Component, OnInit } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';

interface Supplier {
  id: string;
  nombre: string;
  contacto: string;
  telefono: string;
  email: string;
  direccion: string;
  categoria: string;
  calificacion: number;
  tiempoEntrega: number;
  activo: boolean;
  fechaRegistro: Date;
}

@Component({
  selector: 'app-supplier-management',
  templateUrl: './supplier-management.component.html',
  styleUrls: ['./supplier-management.component.scss']
})
export class SupplierManagementComponent implements OnInit {
  suppliers: Supplier[] = [];
  filteredSuppliers: Supplier[] = [];
  
  // Dialog states
  showCreateDialog = false;
  showEditDialog = false;
  
  // Forms
  newSupplier: Partial<Supplier> = {};
  editSupplier: Partial<Supplier> = {};
  selectedSupplier: Supplier | null = null;
  
  // Filters
  searchTerm = '';
  categoryFilter = '';
  statusFilter = '';
  
  // Loading states
  isLoading = false;
  
  // Categories
  categories = [
    { label: 'Alimentos', value: 'Alimentos' },
    { label: 'Limpieza', value: 'Limpieza' },
    { label: 'Tecnología', value: 'Tecnología' },
    { label: 'Textiles', value: 'Textiles' },
    { label: 'Farmacéuticos', value: 'Farmacéuticos' },
    { label: 'Construcción', value: 'Construcción' },
    { label: 'Automotriz', value: 'Automotriz' },
    { label: 'Bebidas', value: 'Bebidas' }
  ];

  // Computed property for category options
  get categoryOptions() {
    return [
      { label: 'Todas las categorías', value: '' },
      ...this.categories
    ];
  }

  constructor(
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.isLoading = true;
    
    // Mock data - in real app this would come from API
    setTimeout(() => {
      this.suppliers = [
        {
          id: 'PROV001',
          nombre: 'Distribuidora Nacional de Alimentos S.A.',
          contacto: 'María Fernanda Gómez',
          telefono: '+57 1 789 0123',
          email: 'ventas@dnalimentos.com',
          direccion: 'Zona Industrial Bogotá, Calle 13 #45-67',
          categoria: 'Alimentos',
          calificacion: 4.5,
          tiempoEntrega: 3,
          activo: true,
          fechaRegistro: new Date('2024-01-15')
        },
        {
          id: 'PROV002',
          nombre: 'Productos de Limpieza El Aseo Ltda',
          contacto: 'Carlos Alberto Ruiz',
          telefono: '+57 4 890 1234',
          email: 'pedidos@elaseo.com',
          direccion: 'Medellín, Carrera 50 #23-45',
          categoria: 'Limpieza',
          calificacion: 4.2,
          tiempoEntrega: 2,
          activo: true,
          fechaRegistro: new Date('2024-02-10')
        },
        {
          id: 'PROV003',
          nombre: 'Tecnología y Equipos Industriales S.A.S',
          contacto: 'Ana Patricia López',
          telefono: '+57 2 567 8901',
          email: 'comercial@tecequipos.com',
          direccion: 'Cali, Avenida 6N #28-45',
          categoria: 'Tecnología',
          calificacion: 4.8,
          tiempoEntrega: 5,
          activo: true,
          fechaRegistro: new Date('2024-01-20')
        },
        {
          id: 'PROV004',
          nombre: 'Textiles y Confecciones del Norte',
          contacto: 'Roberto Martínez',
          telefono: '+57 5 234 5678',
          email: 'ventas@textilesnorte.com',
          direccion: 'Barranquilla, Carrera 43 #76-123',
          categoria: 'Textiles',
          calificacion: 4.1,
          tiempoEntrega: 4,
          activo: true,
          fechaRegistro: new Date('2024-03-05')
        },
        {
          id: 'PROV005',
          nombre: 'Farmacéuticos Unidos de Colombia',
          contacto: 'Dr. Luis Fernando Herrera',
          telefono: '+57 1 345 6789',
          email: 'pedidos@farmaunidos.com',
          direccion: 'Bogotá, Zona Rosa, Calle 85 #15-32',
          categoria: 'Farmacéuticos',
          calificacion: 4.7,
          tiempoEntrega: 1,
          activo: true,
          fechaRegistro: new Date('2024-02-28')
        },
        {
          id: 'PROV006',
          nombre: 'Construcción y Materiales del Pacífico',
          contacto: 'Ingeniero Miguel Torres',
          telefono: '+57 2 678 9012',
          email: 'construccion@matpacifico.com',
          direccion: 'Buenaventura, Zona Industrial, Km 5',
          categoria: 'Construcción',
          calificacion: 4.3,
          tiempoEntrega: 7,
          activo: true,
          fechaRegistro: new Date('2024-01-10')
        },
        {
          id: 'PROV007',
          nombre: 'Automotriz Central Ltda',
          contacto: 'Sandra Milena Castro',
          telefono: '+57 4 789 0123',
          email: 'repuestos@autocentral.com',
          direccion: 'Medellín Centro, Carrera 52 #45-67',
          categoria: 'Automotriz',
          calificacion: 4.0,
          tiempoEntrega: 3,
          activo: false,
          fechaRegistro: new Date('2023-12-15')
        },
        {
          id: 'PROV008',
          nombre: 'Bebidas y Licores Premium S.A.',
          contacto: 'Juan Carlos Pérez',
          telefono: '+57 1 890 1234',
          email: 'distribución@licorpremium.com',
          direccion: 'Bogotá Norte, Calle 127 #45-89',
          categoria: 'Bebidas',
          calificacion: 4.6,
          tiempoEntrega: 2,
          activo: true,
          fechaRegistro: new Date('2024-03-12')
        }
      ];
      
      this.filterSuppliers();
      this.isLoading = false;
    }, 1000);
  }

  filterSuppliers() {
    this.filteredSuppliers = this.suppliers.filter(supplier => {
      const matchesSearch = !this.searchTerm || 
        supplier.nombre.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        supplier.contacto.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        supplier.email.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesCategory = !this.categoryFilter || supplier.categoria === this.categoryFilter;
      const matchesStatus = !this.statusFilter || 
        (this.statusFilter === 'active' && supplier.activo) ||
        (this.statusFilter === 'inactive' && !supplier.activo);
      
      return matchesSearch && matchesCategory && matchesStatus;
    });
  }

  // Statistics
  get totalSuppliers() { return this.suppliers.length; }
  get activeSuppliers() { return this.suppliers.filter(s => s.activo).length; }
  get totalCategories() { return new Set(this.suppliers.map(s => s.categoria)).size; }
  get averageRating() { 
    const ratings = this.suppliers.filter(s => s.activo).map(s => s.calificacion);
    return ratings.length > 0 ? (ratings.reduce((a, b) => a + b, 0) / ratings.length).toFixed(1) : '0.0';
  }

  // Dialog methods
  openCreateDialog() {
    this.newSupplier = {};
    this.showCreateDialog = true;
  }

  openEditDialog(supplier: Supplier) {
    this.editSupplier = { ...supplier };
    this.selectedSupplier = supplier;
    this.showEditDialog = true;
  }

  createSupplier() {
    if (this.newSupplier.nombre && this.newSupplier.email && this.newSupplier.categoria) {
      const supplier: Supplier = {
        id: `PROV${String(this.suppliers.length + 1).padStart(3, '0')}`,
        nombre: this.newSupplier.nombre!,
        contacto: this.newSupplier.contacto || '',
        telefono: this.newSupplier.telefono || '',
        email: this.newSupplier.email!,
        direccion: this.newSupplier.direccion || '',
        categoria: this.newSupplier.categoria!,
        calificacion: 0,
        tiempoEntrega: this.newSupplier.tiempoEntrega || 3,
        activo: true,
        fechaRegistro: new Date()
      };
      
      this.suppliers.push(supplier);
      this.filterSuppliers();
      this.showCreateDialog = false;
      this.newSupplier = {};
      
      this.messageService.add({
        severity: 'success',
        summary: 'Proveedor Creado',
        detail: `Proveedor ${supplier.nombre} creado exitosamente`
      });
    }
  }

  updateSupplier() {
    if (this.selectedSupplier && this.editSupplier.nombre && this.editSupplier.email) {
      const index = this.suppliers.findIndex(s => s.id === this.selectedSupplier!.id);
      if (index !== -1) {
        this.suppliers[index] = { ...this.suppliers[index], ...this.editSupplier };
        this.filterSuppliers();
        this.showEditDialog = false;
        this.selectedSupplier = null;
        this.editSupplier = {};
        
        this.messageService.add({
          severity: 'success',
          summary: 'Proveedor Actualizado',
          detail: `Proveedor actualizado exitosamente`
        });
      }
    }
  }

  deactivateSupplier(supplier: Supplier) {
    this.confirmationService.confirm({
      message: `¿Está seguro de que desea desactivar al proveedor ${supplier.nombre}?`,
      header: 'Confirmar Desactivación',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, Desactivar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        supplier.activo = false;
        this.filterSuppliers();
        this.messageService.add({
          severity: 'warn',
          summary: 'Proveedor Desactivado',
          detail: `Proveedor ${supplier.nombre} desactivado exitosamente`
        });
      }
    });
  }

  activateSupplier(supplier: Supplier) {
    this.confirmationService.confirm({
      message: `¿Está seguro de que desea activar al proveedor ${supplier.nombre}?`,
      header: 'Confirmar Activación',
      icon: 'pi pi-check-circle',
      acceptLabel: 'Sí, Activar',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-success',
      accept: () => {
        supplier.activo = true;
        this.filterSuppliers();
        this.messageService.add({
          severity: 'success',
          summary: 'Proveedor Activado',
          detail: `Proveedor ${supplier.nombre} activado exitosamente`
        });
      }
    });
  }
}
