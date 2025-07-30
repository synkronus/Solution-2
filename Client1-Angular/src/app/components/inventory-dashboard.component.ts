import { Component, OnInit } from '@angular/core';
import { ApiService, Producto, ProductListResponse, AdjustStockRequest, StockOperationResponse } from '../services/api.service';

interface Product {
  id: string;
  nombre: string;
  categoria: string;
  precio: number;
  stockActual: number;
  stockMinimo: number;
  stockMaximo: number;
  proveedor: string;
  fechaActualizacion: Date;
  activo: boolean;
}

interface StockAlert {
  id: string;
  productoId: string;
  nombreProducto: string;
  stockActual: number;
  stockMinimo: number;
  tipo: 'Bajo Stock' | 'Sin Stock' | 'Sobrestock';
  prioridad: 'Alta' | 'Media' | 'Baja';
  fechaAlerta: Date;
}

interface InventoryMovement {
  id: string;
  productoId: string;
  nombreProducto: string;
  tipo: 'Entrada' | 'Salida' | 'Ajuste';
  cantidad: number;
  motivo: string;
  usuario: string;
  fecha: Date;
}

interface PurchaseOrder {
  id: string;
  proveedor: string;
  productos: {
    productoId: string;
    nombreProducto: string;
    cantidad: number;
    precio: number;
  }[];
  total: number;
  estado: 'Pendiente' | 'Enviada' | 'Recibida' | 'Cancelada';
  fechaCreacion: Date;
  fechaEntregaEstimada?: Date;
}

@Component({
  selector: 'app-inventory-dashboard',
  templateUrl: './inventory-dashboard.component.html',
  styleUrls: ['./inventory-dashboard.component.scss']
})
export class InventoryDashboardComponent implements OnInit {
  // Data
  products: Product[] = [];
  filteredProducts: Product[] = [];
  stockAlerts: StockAlert[] = [];
  recentMovements: InventoryMovement[] = [];
  purchaseOrders: PurchaseOrder[] = [];

  // Filters
  selectedCategory: string = '';
  selectedStockStatus: string = '';

  // UI State
  showAddProduct: boolean = false;
  newProduct: Partial<Product> = {};

  constructor(private apiService: ApiService) {}

  ngOnInit() {
    this.loadRealData();
    this.initializeFilters();
  }

  initializeFilters() {
    // Initialize dropdown values to show all items by default
    this.selectedCategory = '';
    this.selectedStockStatus = '';

    // Debug: Log available options
    console.log('Category options:', this.getCategoryOptions());
    console.log('Stock status options:', this.getStockStatusOptions());
  }

  loadRealData() {
    console.log('Loading real product data from API...');

    // Load products from API
    this.apiService.getProducts(1, 100, undefined, true).subscribe({
      next: (response: ProductListResponse) => {
        console.log('API Response:', response);

        // Convert API products to component format
        this.products = response.products.map(producto => ({
          id: producto.id,
          nombre: producto.nombre,
          categoria: producto.categoria,
          precio: producto.precio,
          stockActual: producto.stock,
          stockMinimo: producto.stockMinimo,
          stockMaximo: producto.stockMaximo,
          proveedor: 'API Provider', // Default since API doesn't have provider field
          fechaActualizacion: new Date(producto.fechaActualizacion || producto.fechaCreacion),
          activo: producto.estado
        }));

        console.log('Converted products:', this.products);

        // Generate mock data for other components since they don't have API endpoints yet
        this.generateMockSupportingData();

        // Apply filters after data is loaded
        this.filterProducts();
        this.initializeFilters(); // Re-initialize filters after data loads
      },
      error: (error) => {
        console.error('Error loading products from API:', error);
        console.log('Falling back to mock data...');
        this.loadMockData();
        this.initializeFilters(); // Initialize filters after fallback data loads
      }
    });
  }

  generateMockSupportingData() {
    // Generate stock alerts based on real product data
    this.stockAlerts = this.products
      .filter(p => p.stockActual <= p.stockMinimo)
      .map(p => ({
        id: `ALERT_${p.id}`,
        productoId: p.id,
        nombreProducto: p.nombre,
        stockActual: p.stockActual,
        stockMinimo: p.stockMinimo,
        tipo: p.stockActual === 0 ? 'Sin Stock' : 'Bajo Stock',
        prioridad: p.stockActual === 0 ? 'Alta' : 'Media',
        fechaAlerta: new Date()
      } as StockAlert));

    // Mock Recent Movements
    this.recentMovements = [
      {
        id: 'MOV001',
        productoId: 'P001',
        nombreProducto: 'Laptop Dell Inspiron 15',
        tipo: 'Entrada',
        cantidad: 10,
        motivo: 'Compra a proveedor',
        usuario: 'admin',
        fecha: new Date(Date.now() - 2 * 60 * 60 * 1000)
      },
      {
        id: 'MOV002',
        productoId: 'P002',
        nombreProducto: 'Mouse Logitech MX Master',
        tipo: 'Salida',
        cantidad: 5,
        motivo: 'Venta a cliente',
        usuario: 'vendedor1',
        fecha: new Date(Date.now() - 1 * 60 * 60 * 1000)
      },
      {
        id: 'MOV003',
        productoId: 'P003',
        nombreProducto: 'Monitor Samsung 24"',
        tipo: 'Salida',
        cantidad: 8,
        motivo: 'Venta corporativa',
        usuario: 'vendedor2',
        fecha: new Date(Date.now() - 30 * 60 * 1000)
      }
    ];

    // Mock Purchase Orders
    this.purchaseOrders = [
      {
        id: 'PO001',
        proveedor: 'TechSupply Colombia',
        productos: [
          { productoId: 'P002', nombreProducto: 'Mouse Logitech MX Master', cantidad: 20, precio: 350000 },
          { productoId: 'P004', nombreProducto: 'Teclado Mecánico RGB', cantidad: 15, precio: 450000 }
        ],
        total: 13750000,
        estado: 'Pendiente',
        fechaCreacion: new Date(Date.now() - 24 * 60 * 60 * 1000),
        fechaEntregaEstimada: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)
      },
      {
        id: 'PO002',
        proveedor: 'Distribuidora Valle',
        productos: [
          { productoId: 'P003', nombreProducto: 'Monitor Samsung 24"', cantidad: 10, precio: 800000 }
        ],
        total: 8000000,
        estado: 'Enviada',
        fechaCreacion: new Date(Date.now() - 48 * 60 * 60 * 1000),
        fechaEntregaEstimada: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000)
      }
    ];
  }

  getCategories(): string[] {
    return [...new Set(this.products.map(p => p.categoria))];
  }

  getCategoryOptions() {
    const categories = this.getCategories();
    return [{label: 'Todas las categorías', value: ''}].concat(
      categories.map(c => ({label: c, value: c}))
    );
  }

  getStockStatusOptions() {
    return [
      {label: 'Todos', value: ''},
      {label: 'En Stock', value: 'inStock'},
      {label: 'Stock Bajo', value: 'lowStock'},
      {label: 'Sin Stock', value: 'outOfStock'}
    ];
  }

  getSupplierOptions() {
    const suppliers = [...new Set(this.products.map(p => p.proveedor))];
    return [{label: 'Todos los proveedores', value: ''}].concat(
      suppliers.map(s => ({label: s, value: s}))
    );
  }

  filterProducts() {
    this.filteredProducts = this.products.filter(product => {
      const categoryMatch = !this.selectedCategory || product.categoria === this.selectedCategory;

      let stockMatch = true;
      if (this.selectedStockStatus === 'lowStock') {
        stockMatch = product.stockActual <= product.stockMinimo && product.stockActual > 0;
      } else if (this.selectedStockStatus === 'outOfStock') {
        stockMatch = product.stockActual === 0;
      } else if (this.selectedStockStatus === 'inStock') {
        stockMatch = product.stockActual > product.stockMinimo;
      }

      return categoryMatch && stockMatch;
    });
  }

  getTotalInventoryValue(): number {
    return this.products.reduce((total, product) => total + (product.precio * product.stockActual), 0);
  }

  addProduct() {
    if (this.newProduct.nombre && this.newProduct.categoria && this.newProduct.precio) {
      const product: Product = {
        id: `P${String(this.products.length + 1).padStart(3, '0')}`,
        nombre: this.newProduct.nombre,
        categoria: this.newProduct.categoria,
        precio: this.newProduct.precio,
        stockActual: this.newProduct.stockActual || 0,
        stockMinimo: 5,
        stockMaximo: 100,
        proveedor: 'Proveedor Genérico',
        fechaActualizacion: new Date(),
        activo: true
      };
      
      this.products.push(product);
      this.filterProducts();
      this.showAddProduct = false;
      this.newProduct = {};
      alert('Producto agregado exitosamente');
    }
  }

  editProduct(product: Product) {
    alert(`Editar producto: ${product.nombre}`);
  }

  viewMovements(productId: string) {
    const product = this.products.find(p => p.id === productId);
    if (product) {
      console.log(`Loading movements for product: ${productId}`);

      // Load real movements from API
      this.apiService.getStockMovements(productId).subscribe({
        next: (movements) => {
          console.log('Stock movements loaded:', movements);

          if (movements && movements.length > 0) {
            const movementsList = movements.map(m =>
              `${new Date(m.fechaMovimiento).toLocaleString()} - ${m.tipoMovimiento}: ${m.cantidad} unidades (${m.motivo})`
            ).join('\n');

            alert(`Movimientos de ${product.nombre}:\n\n${movementsList}`);
          } else {
            alert(`No hay movimientos registrados para ${product.nombre}`);
          }
        },
        error: (error) => {
          console.error('Error loading movements:', error);
          alert(`Error al cargar movimientos para ${product.nombre}`);
        }
      });
    }
  }

  adjustStock(productId: string) {
    const product = this.products.find(p => p.id === productId);
    if (product) {
      const newStock = prompt(`Ajustar stock para ${product.nombre}. Stock actual: ${product.stockActual}`);
      if (newStock !== null) {
        const stockValue = parseInt(newStock);
        if (!isNaN(stockValue) && stockValue >= 0) {

          // Create adjustment request
          const adjustRequest: AdjustStockRequest = {
            idProducto: product.id,
            nuevoStock: stockValue,
            motivo: 'Ajuste manual de inventario desde dashboard',
            usuarioResponsable: 'admin' // In a real app, this would come from authentication
          };

          console.log('Sending stock adjustment request:', adjustRequest);

          // Call the real API
          this.apiService.adjustStock(adjustRequest).subscribe({
            next: (response: StockOperationResponse) => {
              console.log('Stock adjustment successful:', response);

              // Update local product data
              product.stockActual = stockValue;
              product.fechaActualizacion = new Date();

              // Add movement record to local data
              this.recentMovements.unshift({
                id: response.movimientoId || `MOV${String(this.recentMovements.length + 1).padStart(3, '0')}`,
                productoId: product.id,
                nombreProducto: product.nombre,
                tipo: 'Ajuste',
                cantidad: Math.abs(response.stockNuevo - response.stockAnterior),
                motivo: 'Ajuste manual de inventario',
                usuario: 'admin',
                fecha: new Date()
              });

              // Update alerts based on new stock levels
              this.updateStockAlerts();

              this.filterProducts();
              alert(`Stock ajustado exitosamente. Stock anterior: ${response.stockAnterior}, Stock nuevo: ${response.stockNuevo}`);
            },
            error: (error) => {
              console.error('Error adjusting stock:', error);
              alert('Error al ajustar el stock. Por favor intente nuevamente.');
            }
          });
        } else {
          alert('Por favor ingrese un valor numérico válido mayor o igual a 0');
        }
      }
    }
  }

  updateStockAlerts() {
    this.stockAlerts = this.products
      .filter(p => p.stockActual <= p.stockMinimo)
      .map(p => ({
        id: `ALERT_${p.id}`,
        productoId: p.id,
        nombreProducto: p.nombre,
        stockActual: p.stockActual,
        stockMinimo: p.stockMinimo,
        tipo: p.stockActual === 0 ? 'Sin Stock' : 'Bajo Stock',
        prioridad: p.stockActual === 0 ? 'Alta' : 'Media',
        fechaAlerta: new Date()
      } as StockAlert));
  }

  createPurchaseOrderForProduct(productId: string) {
    const product = this.products.find(p => p.id === productId);
    if (product) {
      const quantity = product.stockMaximo - product.stockActual;
      const newOrder: PurchaseOrder = {
        id: `PO${String(this.purchaseOrders.length + 1).padStart(3, '0')}`,
        proveedor: product.proveedor,
        productos: [{
          productoId: product.id,
          nombreProducto: product.nombre,
          cantidad: quantity,
          precio: product.precio
        }],
        total: quantity * product.precio,
        estado: 'Pendiente',
        fechaCreacion: new Date(),
        fechaEntregaEstimada: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)
      };
      
      this.purchaseOrders.unshift(newOrder);
      alert(`Orden de compra creada para ${quantity} unidades de ${product.nombre}`);
    }
  }

  generatePurchaseOrder() {
    const lowStockProducts = this.products.filter(p => p.stockActual <= p.stockMinimo);
    if (lowStockProducts.length > 0) {
      alert(`Se pueden generar órdenes para ${lowStockProducts.length} productos con stock bajo`);
    } else {
      alert('No hay productos que requieran reposición');
    }
  }

  // Fallback method for when API is not available
  loadMockData() {
    console.log('Loading mock data as fallback...');

    // Mock Products
    this.products = [
      {
        id: 'P001',
        nombre: 'Laptop Dell Inspiron 15',
        categoria: 'Electrónicos',
        precio: 2500000,
        stockActual: 15,
        stockMinimo: 5,
        stockMaximo: 50,
        proveedor: 'TechSupply Colombia',
        fechaActualizacion: new Date(),
        activo: true
      },
      {
        id: 'P002',
        nombre: 'Mouse Logitech MX Master',
        categoria: 'Accesorios',
        precio: 350000,
        stockActual: 3,
        stockMinimo: 10,
        stockMaximo: 100,
        proveedor: 'TechSupply Colombia',
        fechaActualizacion: new Date(),
        activo: true
      },
      {
        id: 'P003',
        nombre: 'Monitor Samsung 24"',
        categoria: 'Electrónicos',
        precio: 800000,
        stockActual: 0,
        stockMinimo: 3,
        stockMaximo: 20,
        proveedor: 'Distribuidora Valle',
        fechaActualizacion: new Date(),
        activo: true
      },
      {
        id: 'P004',
        nombre: 'Teclado Mecánico RGB',
        categoria: 'Accesorios',
        precio: 450000,
        stockActual: 12,
        stockMinimo: 8,
        stockMaximo: 40,
        proveedor: 'TechSupply Colombia',
        fechaActualizacion: new Date(),
        activo: true
      }
    ];

    // Generate supporting data
    this.generateMockSupportingData();

    // Apply filters
    this.filterProducts();
  }
}
