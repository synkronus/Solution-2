import React, { useState, useEffect, useRef } from 'react';

// Styles
import './SalesComponent.css';
import './EnhancedFormStyles.css';

// PrimeReact Components
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { InputNumber } from 'primereact/inputnumber';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';
import { ProgressSpinner } from 'primereact/progressspinner';
import { Divider } from 'primereact/divider';
import { Panel } from 'primereact/panel';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { Toolbar } from 'primereact/toolbar';

// Context and Hooks
import { useAppContext, useAuth } from '../context/AppContext';
import { useApi } from '../hooks/useApi';

// Interfaces
interface Sale {
  id: string;
  fecha: string;
  idCliente: string;
  cliente: string;
  idVendedor: string;
  vendedor: string;
  total: number;
  estado: 'Procesada' | 'Pendiente' | 'Cancelada';
  observaciones?: string;
  detalles: SaleDetail[];
}

interface SaleDetail {
  idProducto: string;
  nombreProducto: string;
  cantidad: number;
  precio: number;
  descuento: number;
  subtotal: number;
}

interface Customer {
  id: string;
  nombre: string;
  email: string;
  telefono: string;
  direccion: string;
  ciudad?: string; // Make ciudad optional to match Cliente interface
}

interface Product {
  id: string;
  nombre: string;
  precio: number;
  stockDisponible: number;
  categoria: string;
}

interface CreateSaleRequest {
  idCliente: string;
  idVendedor: string;
  observaciones?: string;
  detalles: {
    idProducto: string;
    cantidad: number;
    precio: number;
    descuento: number;
  }[];
}

const SalesComponent: React.FC = () => {
  const { state } = useAppContext();
  const { user } = useAuth();
  const toast = useRef<Toast>(null);
  const {
    loadCustomers,
    loadProducts,
    createSale,
    loadSales,
    validateSeller,
    getSellerByCode,
    checkInventoryAvailability,
    getCurrentStock,
    createCustomer,
    updateCustomer,
    deleteCustomer,
    createProduct,
    updateProduct,
    deleteProduct,
    getSaleById,
    updateSale,
    deleteSale,
    applyDiscount
  } = useApi();

  // State
  const [sales, setSales] = useState<Sale[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showSellerLogin, setShowSellerLogin] = useState(!user?.vendedorId);
  const [showCustomersDialog, setShowCustomersDialog] = useState(false);
  const [showProductsDialog, setShowProductsDialog] = useState(false);
  const [showCreateCustomerDialog, setShowCreateCustomerDialog] = useState(false);
  const [showEditCustomerDialog, setShowEditCustomerDialog] = useState(false);
  const [showCreateProductDialog, setShowCreateProductDialog] = useState(false);
  const [showEditProductDialog, setShowEditProductDialog] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);

  // Seller authentication
  const [sellerCode, setSellerCode] = useState('');
  const [currentSeller, setCurrentSeller] = useState<any>(null);
  const [isValidatingSeller, setIsValidatingSeller] = useState(false);

  // Sale form
  const [saleForm, setSaleForm] = useState<CreateSaleRequest>({
    idCliente: '',
    idVendedor: '',
    observaciones: '',
    detalles: []
  });

  const [selectedProducts, setSelectedProducts] = useState<any[]>([]);
  const [cartItems, setCartItems] = useState<any[]>([]);
  const [stockValidation, setStockValidation] = useState<{[key: string]: boolean}>({});

  // Customer form
  const [customerForm, setCustomerForm] = useState({
    nombre: '',
    direccion: '',
    telefono: '',
    email: '',
    tipoCliente: 'Regular',
    limiteCredito: 0
  });

  // Product form
  const [productForm, setProductForm] = useState({
    nombre: '',
    descripcion: '',
    precio: 0,
    categoria: '',
    stock: 0,
    stockMinimo: 0,
    stockMaximo: 0,
    unidadMedida: 'Unidad'
  });

  useEffect(() => {
    // Load initial data from API
    const loadInitialData = async () => {
      try {
        setIsLoading(true);

        // Load customers and products from API
        const [customersResponse, productsResponse] = await Promise.all([
          loadCustomers(1, 100),
          loadProducts(1, 100) // Note: loadProducts internally calls apiService.getProducts with activo=true
        ]);

        // Convert API data to component format
        if (customersResponse && customersResponse.customers) {
          const convertedCustomers = customersResponse.customers.map((cliente: any) => ({
            id: cliente.id,
            nombre: cliente.nombre,
            email: cliente.email,
            telefono: cliente.telefono,
            direccion: cliente.direccion,
            ciudad: cliente.ciudad
          }));
          setCustomers(convertedCustomers);
        }

        // Products are now converted in useApi hook, so we can use them directly
        if (productsResponse && productsResponse.products) {
          setProducts(productsResponse.products); // These are already converted
        }
      } catch (error) {
        console.error('Error loading initial data:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadInitialData();
  }, [loadCustomers, loadProducts]);

  // Auto-authenticate seller if user is already logged in with vendorId
  useEffect(() => {
    const autoAuthenticateSeller = async () => {
      if (user?.vendedorId && !currentSeller && !showSellerLogin) {
        try {
          setIsValidatingSeller(true);

          // Get seller information using the vendorId from authenticated user
          const sellerInfo = await getSellerByCode(user.vendedorId);

          if (sellerInfo) {
            setCurrentSeller(sellerInfo);
            setSaleForm(prev => ({ ...prev, idVendedor: user.vendedorId || '' }));
            setSellerCode(user.vendedorId || '');

            // Load seller's sales history
            const salesData = await loadSales(user.vendedorId || '');
            if (salesData) {
              setSales(salesData);
            }
          }
        } catch (error) {
          console.error('Error auto-authenticating seller:', error);
        } finally {
          setIsValidatingSeller(false);
        }
      }
    };

    autoAuthenticateSeller();
  }, [user, currentSeller, showSellerLogin, getSellerByCode, loadSales]);

  // Seller Authentication
  const handleSellerLogin = async () => {
    if (!sellerCode.trim()) return;

    setIsValidatingSeller(true);
    try {
      // Validate seller with real API
      const validation = await validateSeller(sellerCode);

      if (validation && validation.isValid && validation.vendedor) {
        setCurrentSeller(validation.vendedor);
        setSaleForm(prev => ({ ...prev, idVendedor: sellerCode }));
        setShowSellerLogin(false);

        // Load seller's sales history
        const salesData = await loadSales(sellerCode);
        if (salesData) {
          setSales(salesData);
        }
      } else {
        // Show error message
        console.error('Seller validation failed:', validation?.reason || 'Unknown error');
        alert(validation?.reason || 'Vendedor no autorizado');
      }
    } catch (error) {
      console.error('Error validating seller:', error);
      alert('Error al validar vendedor. Verifique la conexión con el servidor.');
    } finally {
      setIsValidatingSeller(false);
    }
  };

  // Cart Management Functions
  const addToCart = async (product: Product, quantity: number = 1) => {
    try {
      // Check inventory availability
      const availability = await checkInventoryAvailability(product.id, quantity);

      if (!availability || !availability.disponibleParaVenta) {
        toast.current?.show({
          severity: 'error',
          summary: 'Stock Insuficiente',
          detail: `Stock insuficiente para ${product.nombre}. Stock disponible: ${availability?.stockDisponible || 0}`,
          life: 4000
        });
        return;
      }

      // Check if product already in cart
      const existingItemIndex = cartItems.findIndex(item => item.idProducto === product.id);

      if (existingItemIndex >= 0) {
        // Update quantity
        const newQuantity = cartItems[existingItemIndex].cantidad + quantity;

        // Check if new quantity is available
        const newAvailability = await checkInventoryAvailability(product.id, newQuantity);
        if (!newAvailability || !newAvailability.disponibleParaVenta) {
          toast.current?.show({
            severity: 'error',
            summary: 'Stock Insuficiente',
            detail: `Stock insuficiente. Máximo disponible: ${newAvailability?.stockDisponible || 0}`,
            life: 4000
          });
          return;
        }

        const updatedCart = [...cartItems];
        updatedCart[existingItemIndex].cantidad = newQuantity;
        updatedCart[existingItemIndex].subtotal = newQuantity * product.precio;
        setCartItems(updatedCart);

        // Show success toast for quantity update
        toast.current?.show({
          severity: 'success',
          summary: 'Cantidad Actualizada',
          detail: `${product.nombre} - Nueva cantidad: ${newQuantity}`,
          life: 3000
        });
      } else {
        // Add new item to cart
        const newItem = {
          idProducto: product.id,
          nombreProducto: product.nombre,
          cantidad: quantity,
          precio: product.precio,
          descuento: 0,
          subtotal: quantity * product.precio
        };
        setCartItems([...cartItems, newItem]);

        // Show success toast for new product
        toast.current?.show({
          severity: 'success',
          summary: 'Producto Agregado',
          detail: `${product.nombre} agregado al carrito`,
          life: 3000
        });
      }

      // Update stock validation
      setStockValidation(prev => ({
        ...prev,
        [product.id]: true
      }));

    } catch (error) {
      console.error('Error adding to cart:', error);
      toast.current?.show({
        severity: 'error',
        summary: 'Error',
        detail: 'Error al agregar producto al carrito',
        life: 4000
      });
    }
  };

  const removeFromCart = (productId: string) => {
    const itemToRemove = cartItems.find(item => item.idProducto === productId);

    if (itemToRemove) {
      confirmDialog({
        message: `¿Está seguro que desea eliminar "${itemToRemove.nombreProducto}" del carrito?`,
        header: 'Confirmar Eliminación',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          setCartItems(cartItems.filter(item => item.idProducto !== productId));
          setStockValidation(prev => {
            const updated = { ...prev };
            delete updated[productId];
            return updated;
          });

          toast.current?.show({
            severity: 'success',
            summary: 'Producto Eliminado',
            detail: `${itemToRemove.nombreProducto} eliminado del carrito`,
            life: 3000
          });
        },
        reject: () => {
          toast.current?.show({
            severity: 'info',
            summary: 'Cancelado',
            detail: 'El producto no fue eliminado',
            life: 2000
          });
        }
      });
    }
  };

  const updateCartQuantity = async (productId: string, newQuantity: number) => {
    if (newQuantity <= 0) {
      removeFromCart(productId);
      return;
    }

    try {
      // Check availability for new quantity
      const availability = await checkInventoryAvailability(productId, newQuantity);

      if (!availability || !availability.disponibleParaVenta) {
        alert(`Stock insuficiente. Máximo disponible: ${availability?.stockDisponible || 0}`);
        return;
      }

      const updatedCart = cartItems.map(item => {
        if (item.idProducto === productId) {
          return {
            ...item,
            cantidad: newQuantity,
            subtotal: newQuantity * item.precio
          };
        }
        return item;
      });

      setCartItems(updatedCart);
    } catch (error) {
      console.error('Error updating cart quantity:', error);
      alert('Error al actualizar cantidad');
    }
  };

  const getCartTotal = () => {
    return cartItems.reduce((total, item) => total + item.subtotal, 0);
  };

  const clearCart = () => {
    setCartItems([]);
    setStockValidation({});
  };

  // Customer Management Functions
  const handleCreateCustomer = async () => {
    if (!customerForm.nombre || !customerForm.email || !customerForm.telefono) {
      alert('Por favor complete los campos obligatorios');
      return;
    }

    try {
      setIsLoading(true);
      const newCustomer = await createCustomer(customerForm);

      if (newCustomer) {
        alert('Cliente creado exitosamente');
        setShowCreateCustomerDialog(false);

        // Reset form
        setCustomerForm({
          nombre: '',
          direccion: '',
          telefono: '',
          email: '',
          tipoCliente: 'Regular',
          limiteCredito: 0
        });

        // Reload customers and select the new one
        await loadCustomers(1, 100);
        setSaleForm(prev => ({ ...prev, idCliente: newCustomer.id }));
      }
    } catch (error) {
      console.error('Error creating customer:', error);
      alert('Error al crear cliente');
    } finally {
      setIsLoading(false);
    }
  };

  // Sale Management
  const handleCreateSale = () => {
    setShowCreateDialog(true);
    setSaleForm({
      idCliente: '',
      idVendedor: currentSeller?.codigoVendedor || '',
      observaciones: '',
      detalles: []
    });
    setSelectedProducts([]);
  };

  const handleShowCustomers = async () => {
    try {
      await loadCustomers(1, 50);
      setShowCustomersDialog(true);
    } catch (error) {
      console.log('Using existing customer data');
      setShowCustomersDialog(true);
    }
  };

  const handleShowProducts = async () => {
    try {
      await loadProducts(1, 50); // loadProducts now filters for active products only
      setShowProductsDialog(true);
    } catch (error) {
      console.log('Using existing product data');
      setShowProductsDialog(true);
    }
  };

  const handleAddProduct = () => {
    setSelectedProducts([...selectedProducts, {
      idProducto: '',
      nombreProducto: '',
      cantidad: 1,
      precio: 0,
      descuento: 0,
      subtotal: 0
    }]);
  };

  const handleRemoveProduct = (index: number) => {
    const newProducts = selectedProducts.filter((_, i) => i !== index);
    setSelectedProducts(newProducts);
  };

  const handleProductChange = (index: number, field: string, value: any) => {
    const newProducts = [...selectedProducts];
    newProducts[index] = { ...newProducts[index], [field]: value };
    
    if (field === 'idProducto') {
      const product = products.find(p => p.id === value);
      if (product) {
        newProducts[index].nombreProducto = product.nombre;
        newProducts[index].precio = product.precio;
      }
    }
    
    // Calculate subtotal
    const cantidad = newProducts[index].cantidad || 0;
    const precio = newProducts[index].precio || 0;
    const descuento = newProducts[index].descuento || 0;
    newProducts[index].subtotal = (cantidad * precio) - descuento;
    
    setSelectedProducts(newProducts);
  };

  const calculateTotal = () => {
    return selectedProducts.reduce((total, item) => total + (item.subtotal || 0), 0);
  };

  const handleSubmitSale = async () => {
    // Enhanced validation
    if (!saleForm.idCliente) {
      toast.current?.show({
        severity: 'error',
        summary: 'Error de Validación',
        detail: 'Debe seleccionar un cliente',
        life: 3000
      });
      return;
    }

    if (!currentSeller?.codigoVendedor) {
      toast.current?.show({
        severity: 'error',
        summary: 'Error de Validación',
        detail: 'No hay vendedor autenticado',
        life: 3000
      });
      return;
    }

    if (cartItems.length === 0) {
      toast.current?.show({
        severity: 'error',
        summary: 'Error de Validación',
        detail: 'Debe agregar productos al carrito',
        life: 3000
      });
      return;
    }

    setIsLoading(true);
    try {
      // Final stock validation before processing
      for (const item of cartItems) {
        const availability = await checkInventoryAvailability(item.idProducto, item.cantidad);
        if (!availability || !availability.disponibleParaVenta) {
          toast.current?.show({
            severity: 'error',
            summary: 'Stock Insuficiente',
            detail: `Stock insuficiente para ${item.nombreProducto}. Stock disponible: ${availability?.stockDisponible || 0}`,
            life: 5000
          });
          setIsLoading(false);
          return;
        }
      }

      const saleRequest: CreateSaleRequest = {
        idCliente: saleForm.idCliente,
        idVendedor: currentSeller.codigoVendedor,
        observaciones: saleForm.observaciones || '',
        detalles: cartItems.map(item => ({
          idProducto: item.idProducto,
          cantidad: item.cantidad,
          precio: item.precio,
          descuento: item.descuento || 0
        }))
      };

      // Debug logging
      console.log('Sale request payload:', saleRequest);

      // Call the real sales API
      const result = await createSale(saleRequest);

      if (result) {
        console.log('Sale created successfully:', result);

        toast.current?.show({
          severity: 'success',
          summary: 'Venta Creada',
          detail: `Venta procesada exitosamente. ID: ${result.ventaId || result.id || 'N/A'}`,
          life: 5000
        });

        setShowCreateDialog(false);

        // Reset form and cart
        setSaleForm({
          idCliente: '',
          idVendedor: currentSeller?.codigoVendedor || '',
          observaciones: '',
          detalles: []
        });
        clearCart();
        setSelectedProducts([]);

        // Reload sales data
        if (currentSeller?.codigoVendedor) {
          const salesData = await loadSales(currentSeller.codigoVendedor);
          if (salesData) {
            setSales(salesData);
          }
        }
      } else {
        toast.current?.show({
          severity: 'error',
          summary: 'Error',
          detail: 'No se pudo procesar la venta. Intente nuevamente.',
          life: 5000
        });
      }
    } catch (error: any) {
      console.error('Error creating sale:', error);

      let errorMessage = 'Error interno del servidor';
      if (error?.response?.data?.message) {
        errorMessage = error.response.data.message;
      } else if (error?.message) {
        errorMessage = error.message;
      }

      toast.current?.show({
        severity: 'error',
        summary: 'Error al Crear Venta',
        detail: errorMessage,
        life: 5000
      });
    } finally {
      setIsLoading(false);
    }
  };

  // Render Methods
  const statusBodyTemplate = (rowData: Sale) => {
    const severity = rowData.estado === 'Procesada' ? 'success' : 
                    rowData.estado === 'Pendiente' ? 'warning' : 'danger';
    return <Tag value={rowData.estado} severity={severity} />;
  };

  const totalBodyTemplate = (rowData: Sale) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(rowData.total);
  };

  const customerOptions = customers.map(c => ({
    label: c.nombre,
    value: c.id
  }));

  const productOptions = products.map(p => ({
    label: `${p.nombre} - Stock: ${p.stockDisponible}`,
    value: p.id
  }));

  // Seller Login Dialog
  if (showSellerLogin) {
    return (
      <div className="sales-container">
        <Card className="login-card" style={{ maxWidth: '500px', margin: '2rem auto' }}>
          <div className="text-center mb-4">
            <i className="pi pi-user text-primary" style={{ fontSize: '3rem' }}></i>
            <h2>Acceso de Vendedor</h2>
            <p className="text-secondary">Ingrese su código de vendedor autorizado</p>
          </div>
          
          <div className="p-field">
            <label htmlFor="sellerCode">Código de Vendedor</label>
            <InputText
              id="sellerCode"
              value={sellerCode}
              onChange={(e) => setSellerCode(e.target.value)}
              placeholder="Ej: V001"
              className="w-full"
              onKeyPress={(e) => e.key === 'Enter' && handleSellerLogin()}
            />
          </div>
          
          <Button
            label="Iniciar Sesión"
            icon="pi pi-sign-in"
            onClick={handleSellerLogin}
            loading={isValidatingSeller}
            className="w-full mt-3"
          />
        </Card>
      </div>
    );
  }

  // CRUD Handler Functions
  const handleDeleteCustomer = async (id: string) => {
    if (window.confirm('¿Está seguro de que desea eliminar este cliente?')) {
      const success = await deleteCustomer(id);
      if (success) {
        await loadCustomers(1, 50);
      }
    }
  };

  const handleUpdateCustomer = async () => {
    if (!editingCustomer || !customerForm.nombre || !customerForm.email || !customerForm.telefono) {
      alert('Por favor complete los campos obligatorios');
      return;
    }

    try {
      setIsLoading(true);
      const updatedCustomer = await updateCustomer(editingCustomer.id, customerForm);

      if (updatedCustomer) {
        setShowEditCustomerDialog(false);
        setEditingCustomer(null);

        // Reset form
        setCustomerForm({
          nombre: '',
          direccion: '',
          telefono: '',
          email: '',
          tipoCliente: 'Regular',
          limiteCredito: 0
        });

        // Reload customers
        await loadCustomers(1, 100);
      }
    } catch (error) {
      console.error('Error updating customer:', error);
      alert('Error al actualizar cliente');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateProduct = async () => {
    if (!productForm.nombre || !productForm.precio || !productForm.categoria) {
      alert('Por favor complete los campos obligatorios');
      return;
    }

    try {
      setIsLoading(true);
      const newProduct = await createProduct(productForm);

      if (newProduct) {
        setShowCreateProductDialog(false);

        // Reset form
        setProductForm({
          nombre: '',
          descripcion: '',
          precio: 0,
          categoria: '',
          stock: 0,
          stockMinimo: 0,
          stockMaximo: 0,
          unidadMedida: 'Unidad'
        });

        // Reload products
        await loadProducts(1, 100);
      }
    } catch (error) {
      console.error('Error creating product:', error);
      alert('Error al crear producto');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUpdateProduct = async () => {
    if (!editingProduct || !productForm.nombre || !productForm.precio || !productForm.categoria) {
      alert('Por favor complete los campos obligatorios');
      return;
    }

    try {
      setIsLoading(true);
      const updatedProduct = await updateProduct(editingProduct.id, productForm);

      if (updatedProduct) {
        setShowEditProductDialog(false);
        setEditingProduct(null);

        // Reset form
        setProductForm({
          nombre: '',
          descripcion: '',
          precio: 0,
          categoria: '',
          stock: 0,
          stockMinimo: 0,
          stockMaximo: 0,
          unidadMedida: 'Unidad'
        });

        // Reload products
        await loadProducts(1, 100);
      }
    } catch (error) {
      console.error('Error updating product:', error);
      alert('Error al actualizar producto');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteProduct = async (id: string) => {
    if (window.confirm('¿Está seguro de que desea eliminar este producto?')) {
      const success = await deleteProduct(id);
      if (success) {
        await loadProducts(1, 50);
      }
    }
  };

  const handleDeleteSale = async (id: string) => {
    const motivo = prompt('Ingrese el motivo de cancelación:');
    if (motivo) {
      const success = await deleteSale(id, motivo);
      if (success) {
        await loadSales(currentSeller?.codigoVendedor);
      }
    }
  };

  const handleApplyDiscount = async (sale: any) => {
    const descuentoStr = prompt('Ingrese el porcentaje de descuento (0-100):');
    const motivo = prompt('Ingrese el motivo del descuento:');

    if (descuentoStr && motivo) {
      const descuento = parseFloat(descuentoStr);
      if (descuento >= 0 && descuento <= 100) {
        const updatedSale = await applyDiscount(sale.id, descuento, motivo);
        if (updatedSale) {
          await loadSales(currentSeller?.codigoVendedor);
        }
      } else {
        alert('El descuento debe estar entre 0 y 100');
      }
    }
  };

  return (
    <div className="sales-container">
      {/* Header */}
      <div className="header-section mb-4">
        <div className="flex justify-content-between align-items-start">
          <div className="flex-grow-1">
            <h1>
              <i className="pi pi-shopping-cart text-primary"></i>
              RF2: Gestión de Ventas
            </h1>
            <p className="text-secondary">
              Component-Based Software Engineering - Sales Component
            </p>
            <p className="text-sm">
              <strong>Vendedor:</strong> {currentSeller?.nombre} ({currentSeller?.codigoVendedor})
            </p>
          </div>
          <div className="flex-shrink-0 ml-3">
            <Button
              icon="pi pi-sign-out"
              className="p-button-outlined p-button-sm compact-logout-btn"
              onClick={() => setShowSellerLogin(true)}
              tooltip="Cerrar Sesión"
              tooltipOptions={{ position: 'left' }}
            />
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <Card className="mb-4">
        <div className="card-header-content">
          <h3>Acciones Rápidas</h3>
          <div className="flex gap-2">
            <Button
              label="Nueva Venta"
              icon="pi pi-plus"
              onClick={handleCreateSale}
            />
            <Button
              label="Ver Clientes"
              icon="pi pi-users"
              className="p-button-outlined"
              onClick={handleShowCustomers}
            />
            <Button
              label="Catálogo Productos"
              icon="pi pi-list"
              className="p-button-outlined"
              onClick={handleShowProducts}
            />
          </div>
        </div>
      </Card>

      {/* Sales Statistics */}
      <div className="stats-grid mb-4">
        <div className="stat-card">
          <i className="pi pi-chart-line text-blue-500"></i>
          <div className="stat-content">
            <h3>{sales.filter(sale => {
              const today = new Date();
              const saleDate = new Date(sale.fecha);
              return saleDate.toDateString() === today.toDateString();
            }).length}</h3>
            <p>Ventas Hoy</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-dollar text-green-500"></i>
          <div className="stat-content">
            <h3>{new Intl.NumberFormat('es-CO', {
              style: 'currency',
              currency: 'COP'
            }).format(sales.reduce((total, sale) => total + sale.total, 0))}</h3>
            <p>Total Ventas</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-users text-purple-500"></i>
          <div className="stat-content">
            <h3>{customers.length}</h3>
            <p>Clientes</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-box text-orange-500"></i>
          <div className="stat-content">
            <h3>{products.length}</h3>
            <p>Productos</p>
          </div>
        </div>
      </div>

      {/* Sales Metrics */}
      {currentSeller && (
        <Card className="mb-4">
          <div className="card-header-content">
            <h3>Métricas de Ventas - {currentSeller.nombre}</h3>
          </div>

          <div className="grid sales-metrics-grid">
            <div className="col-3">
              <div className="text-center p-3 border-1 border-300 border-round">
                <div className="text-2xl font-bold text-primary">
                  {sales.length}
                </div>
                <div className="text-sm text-600">Total Ventas</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 border-1 border-300 border-round">
                <div className="text-2xl font-bold text-green-500">
                  {new Intl.NumberFormat('es-CO', {
                    style: 'currency',
                    currency: 'COP'
                  }).format(sales.reduce((total, sale) => total + sale.total, 0))}
                </div>
                <div className="text-sm text-600">Ventas Totales</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 border-1 border-300 border-round">
                <div className="text-2xl font-bold text-blue-500">
                  {sales.filter(sale => sale.estado === 'Procesada').length}
                </div>
                <div className="text-sm text-600">Ventas Procesadas</div>
              </div>
            </div>
            <div className="col-3">
              <div className="text-center p-3 border-1 border-300 border-round">
                <div className="text-2xl font-bold text-orange-500">
                  {sales.length > 0 ?
                    new Intl.NumberFormat('es-CO', {
                      style: 'currency',
                      currency: 'COP'
                    }).format(sales.reduce((total, sale) => total + sale.total, 0) / sales.length)
                    : '$0'
                  }
                </div>
                <div className="text-sm text-600">Promedio por Venta</div>
              </div>
            </div>
          </div>
        </Card>
      )}

      {/* Sales History */}
      <Card>
        <div className="card-header-content">
          <h3>Historial de Ventas</h3>
          <Button
            label="Actualizar"
            icon="pi pi-refresh"
            className="p-button-sm p-button-outlined"
            onClick={() => currentSeller && loadSales(currentSeller.codigoVendedor)}
            loading={isLoading}
          />
        </div>
        
        <DataTable
          value={state.sales || sales}
          paginator
          rows={10}
          loading={isLoading}
          emptyMessage="No hay ventas registradas para este vendedor"
          className="p-datatable-striped"
          showGridlines
          responsiveLayout="scroll"
          sortField="fecha"
          sortOrder={-1}
        >
          <Column
            field="id"
            header="ID Venta"
            sortable
            style={{ width: '120px' }}
          />
          <Column
            field="fecha"
            header="Fecha"
            sortable
            body={(rowData) => {
              const date = new Date(rowData.fecha);
              return date.toLocaleDateString('es-CO', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
              });
            }}
          />
          <Column
            field="idCliente"
            header="Cliente"
            sortable
            body={(rowData) => {
              const customer = customers.find(c => c.id === rowData.idCliente);
              return customer ? customer.nombre : rowData.idCliente;
            }}
          />
          <Column
            field="total"
            header="Total"
            body={totalBodyTemplate}
            sortable
            style={{ width: '120px' }}
          />
          <Column
            field="estado"
            header="Estado"
            body={statusBodyTemplate}
            sortable
            style={{ width: '120px' }}
          />
          <Column
            field="detalles"
            header="Items"
            body={(rowData) => (
              <Tag
                value={`${rowData.detalles?.length || 0} items`}
                severity="info"
              />
            )}
            style={{ width: '100px' }}
          />
          <Column
            field="observaciones"
            header="Observaciones"
            body={(rowData) => (
              rowData.observaciones ?
                <span title={rowData.observaciones}>
                  {rowData.observaciones.length > 30 ?
                    `${rowData.observaciones.substring(0, 30)}...` :
                    rowData.observaciones
                  }
                </span> :
                <span className="text-400">Sin observaciones</span>
            )}
          />
          <Column
            header="Acciones"
            body={(rowData) => (
              <div className="flex gap-2">
                <Button
                  icon="pi pi-percentage"
                  className="p-button-info p-button-sm"
                  tooltip="Aplicar descuento"
                  onClick={() => handleApplyDiscount(rowData)}
                  disabled={rowData.estado === 'Cancelada'}
                />
                <Button
                  icon="pi pi-trash"
                  className="p-button-danger p-button-sm"
                  tooltip="Cancelar venta"
                  onClick={() => handleDeleteSale(rowData.id)}
                  disabled={rowData.estado === 'Cancelada'}
                />
              </div>
            )}
          />
        </DataTable>
      </Card>

      {/* Create Sale Dialog */}
      <Dialog
        header="Nueva Venta"
        visible={showCreateDialog}
        style={{ width: '80vw', maxWidth: '1000px' }}
        onHide={() => setShowCreateDialog(false)}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              className="p-button-text"
              onClick={() => setShowCreateDialog(false)}
            />
            <Button
              label="Crear Venta"
              icon="pi pi-check"
              onClick={handleSubmitSale}
              loading={isLoading}
              disabled={!saleForm.idCliente || cartItems.length === 0}
            />
          </div>
        }
      >
        <div className="grid">
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="cliente">Cliente *</label>
              <Dropdown
                id="cliente"
                value={saleForm.idCliente}
                options={customerOptions}
                onChange={(e) => setSaleForm({...saleForm, idCliente: e.value})}
                placeholder="Seleccione un cliente"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="vendedor">Vendedor</label>
              <InputText
                id="vendedor"
                value={currentSeller?.nombre}
                disabled
                className="w-full"
              />
            </div>
          </div>
        </div>

        <Divider />

        <div className="flex justify-content-between align-items-center mb-3 w-full">
          <div className="flex align-items-center gap-2">
            <i className="pi pi-shopping-cart text-primary"></i>
            <span className="font-semibold text-primary">Carrito de Compras</span>
            {cartItems.length > 0 && (
              <span className="text-sm text-600">
                {cartItems.length} {cartItems.length === 1 ? 'producto' : 'productos'}
              </span>
            )}
          </div>
          <div className="flex gap-2">
            <Button
              label="Ver Productos"
              icon="pi pi-shopping-bag"
              className="p-button-success p-button-sm"
              onClick={handleShowProducts}
            />
            {cartItems.length > 0 && (
              <Button
                label="Limpiar"
                icon="pi pi-trash"
                className="p-button-outlined p-button-danger p-button-sm"
                onClick={() => {
                  confirmDialog({
                    message: '¿Está seguro que desea eliminar todos los productos del carrito?',
                    header: 'Confirmar Limpieza',
                    icon: 'pi pi-exclamation-triangle',
                    accept: () => {
                      clearCart();
                      toast.current?.show({
                        severity: 'success',
                        summary: 'Carrito Limpiado',
                        detail: 'Todos los productos han sido eliminados del carrito',
                        life: 3000
                      });
                    },
                    reject: () => {
                      toast.current?.show({
                        severity: 'info',
                        summary: 'Cancelado',
                        detail: 'El carrito no fue modificado',
                        life: 2000
                      });
                    }
                  });
                }}
              />
            )}
          </div>
        </div>

        {/* Cart Items Display - Organized Layout */}
        {cartItems.length > 0 ? (
          <Card className="mb-3">
            {/* Cart Items */}
            <div className="p-0">
              <DataTable
                value={cartItems}
                responsiveLayout="scroll"
                className="p-datatable-sm"
                showGridlines={false}
                stripedRows
                emptyMessage="No hay productos en el carrito"
              >
                <Column
                  field="nombreProducto"
                  header="Producto"
                  body={(rowData) => (
                    <div className="py-1">
                      <div className="font-semibold text-900">{rowData.nombreProducto}</div>
                      <div className="text-xs text-500">ID: {rowData.idProducto}</div>
                    </div>
                  )}
                  style={{ width: '40%' }}
                />

                <Column
                  field="cantidad"
                  header="Cantidad"
                  body={(rowData) => (
                    <div className="flex align-items-center justify-content-center">
                      <Button
                        icon="pi pi-minus"
                        className="p-button-text p-button-sm"
                        onClick={() => updateCartQuantity(rowData.idProducto, rowData.cantidad - 1)}
                        disabled={rowData.cantidad <= 1}
                        style={{ width: '24px', height: '24px', padding: '0' }}
                      />
                      <div className="bg-primary text-white px-2 py-1 border-round font-semibold mx-1 text-center" style={{ minWidth: '32px', fontSize: '0.875rem' }}>
                        {rowData.cantidad}
                      </div>
                      <Button
                        icon="pi pi-plus"
                        className="p-button-text p-button-sm"
                        onClick={() => updateCartQuantity(rowData.idProducto, rowData.cantidad + 1)}
                        style={{ width: '24px', height: '24px', padding: '0' }}
                      />
                    </div>
                  )}
                  style={{ width: '18%', textAlign: 'center' }}
                />

                <Column
                  field="precio"
                  header="Precio Unit."
                  body={(rowData) => (
                    <div className="text-right font-semibold text-600">
                      {new Intl.NumberFormat('es-CO', {
                        style: 'currency',
                        currency: 'COP'
                      }).format(rowData.precio)}
                    </div>
                  )}
                  style={{ width: '18%' }}
                />

                <Column
                  field="subtotal"
                  header="Subtotal"
                  body={(rowData) => (
                    <div className="text-right font-bold text-primary">
                      {new Intl.NumberFormat('es-CO', {
                        style: 'currency',
                        currency: 'COP'
                      }).format(rowData.subtotal)}
                    </div>
                  )}
                  style={{ width: '18%' }}
                />

                <Column
                  header=""
                  body={(rowData) => (
                    <div className="text-center">
                      <Button
                        icon="pi pi-trash"
                        className="p-button-text p-button-danger p-button-sm"
                        onClick={() => removeFromCart(rowData.idProducto)}
                        tooltip="Eliminar"
                        style={{ width: '28px', height: '28px' }}
                      />
                    </div>
                  )}
                  style={{ width: '6%' }}
                />
              </DataTable>
            </div>

            {/* Cart Summary */}
            <div className="p-3 bg-primary-50 border-round-bottom border-top-1 border-primary-200">
              <div className="flex flex-column gap-2">
                <div className="flex align-items-center gap-2">
                  <i className="pi pi-info-circle text-primary"></i>
                  <span className="text-primary font-semibold">
                    Resumen: {cartItems.length} productos • {cartItems.reduce((total, item) => total + item.cantidad, 0)} unidades
                  </span>
                </div>
                <div className="flex justify-content-end">
                  <div className="flex align-items-center gap-3">
                    <span className="font-semibold text-900">Total a Pagar:</span>
                    <div className="bg-primary text-white px-4 py-2 border-round font-bold text-xl shadow-2">
                      {new Intl.NumberFormat('es-CO', {
                        style: 'currency',
                        currency: 'COP'
                      }).format(cartItems.reduce((total, item) => total + item.subtotal, 0))}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </Card>
        ) : (
          <Card className="mb-3">
            <div className="text-center p-5">
              <i className="pi pi-shopping-cart text-5xl text-400 mb-3"></i>
              <div className="text-lg text-600 mb-2 font-semibold">Tu carrito está vacío</div>
              <div className="text-sm text-500 mb-3">
                Explora nuestro catálogo y encuentra los productos que necesitas
              </div>
              <Button
                label="Explorar Productos"
                icon="pi pi-search"
                className="p-button-outlined p-button-primary"
                onClick={handleShowProducts}
              />
            </div>
          </Card>
        )}



        <div className="p-field mt-3">
          <label htmlFor="observaciones">Observaciones</label>
          <InputTextarea
            id="observaciones"
            value={saleForm.observaciones}
            onChange={(e) => setSaleForm({...saleForm, observaciones: e.target.value})}
            rows={3}
            className="w-full"
          />
        </div>
      </Dialog>

      {/* Customers Dialog */}
      <Dialog
        header="Seleccionar Cliente"
        visible={showCustomersDialog}
        style={{ width: '80vw', maxWidth: '1000px' }}
        onHide={() => setShowCustomersDialog(false)}
        maximizable
        footer={
          <div>
            <Button
              label="Crear Nuevo Cliente"
              icon="pi pi-plus"
              className="p-button-success"
              onClick={() => {
                setShowCustomersDialog(false);
                setShowCreateCustomerDialog(true);
              }}
            />
          </div>
        }
      >
        <DataTable
          value={state.customers.length > 0 ? state.customers : customers}
          paginator
          rows={10}
          loading={state.isLoadingCustomers}
          emptyMessage="No hay clientes registrados"
          className="p-datatable-striped"
          showGridlines
          responsiveLayout="scroll"
        >
          <Column field="id" header="ID" sortable />
          <Column field="nombre" header="Nombre" sortable />
          <Column field="email" header="Email" sortable />
          <Column field="telefono" header="Teléfono" />
          <Column field="direccion" header="Dirección" />
          <Column field="ciudad" header="Ciudad" sortable />
          <Column
            field="tipoCliente"
            header="Tipo Cliente"
            body={(rowData) => (
              <Tag value={rowData.tipoCliente || 'Corporativo'} severity="info" />
            )}
            sortable
          />
          <Column
            header="Acciones"
            body={(rowData) => (
              <div className="flex gap-2">
                <Button
                  label="Seleccionar"
                  icon="pi pi-check"
                  className="p-button-sm"
                  onClick={() => {
                    setSaleForm(prev => ({ ...prev, idCliente: rowData.id }));
                    setShowCustomersDialog(false);
                  }}
                />
                <Button
                  icon="pi pi-pencil"
                  className="p-button-sm p-button-warning"
                  tooltip="Editar cliente"
                  onClick={() => {
                    setEditingCustomer(rowData);
                    setCustomerForm({
                      nombre: rowData.nombre,
                      direccion: rowData.direccion,
                      telefono: rowData.telefono,
                      email: rowData.email,
                      tipoCliente: rowData.tipoCliente,
                      limiteCredito: rowData.limiteCredito
                    });
                    setShowEditCustomerDialog(true);
                  }}
                />
                <Button
                  icon="pi pi-trash"
                  className="p-button-sm p-button-danger"
                  tooltip="Eliminar cliente"
                  onClick={() => handleDeleteCustomer(rowData.id)}
                />
              </div>
            )}
          />
        </DataTable>
      </Dialog>

      {/* Products Dialog */}
      <Dialog
        header="Catálogo de Productos"
        visible={showProductsDialog}
        style={{ width: '80vw', maxWidth: '1000px' }}
        onHide={() => setShowProductsDialog(false)}
        maximizable
      >
        <DataTable
          value={state.products} // Use global state (now properly converted)
          paginator
          rows={10}
          loading={state.isLoadingProducts}
          emptyMessage="No hay productos registrados"
          className="p-datatable-striped"
          showGridlines
          responsiveLayout="scroll"
        >
          <Column field="id" header="ID" sortable />
          <Column field="nombre" header="Nombre" sortable />
          <Column
            field="precio"
            header="Precio"
            body={(rowData) => (
              new Intl.NumberFormat('es-CO', {
                style: 'currency',
                currency: 'COP'
              }).format(rowData.precio)
            )}
            sortable
          />
          <Column
            field="stockDisponible"
            header="Stock"
            body={(rowData) => (
              <Tag
                value={rowData.stockDisponible || rowData.stockActual || 0}
                severity={
                  (rowData.stockDisponible || rowData.stockActual || 0) > 10 ? 'success' :
                  (rowData.stockDisponible || rowData.stockActual || 0) > 0 ? 'warning' : 'danger'
                }
              />
            )}
            sortable
          />
          <Column
            field="categoria"
            header="Categoría"
            body={(rowData) => (
              <Tag value={rowData.categoria} severity="info" />
            )}
            sortable
          />
          <Column
            field="proveedor"
            header="Proveedor"
            sortable
          />
          <Column
            header="Acciones"
            body={(rowData) => (
              <div className="flex gap-2">
                <Button
                  icon="pi pi-plus"
                  className="p-button-success p-button-sm"
                  tooltip="Agregar al carrito"
                  onClick={() => addToCart({
                    id: rowData.id,
                    nombre: rowData.nombre,
                    precio: rowData.precio,
                    stockDisponible: rowData.stockDisponible || rowData.stockActual || 0,
                    categoria: rowData.categoria
                  }, 1)}
                  disabled={(rowData.stockDisponible || rowData.stockActual || 0) <= 0}
                />
                <Button
                  icon="pi pi-pencil"
                  className="p-button-warning p-button-sm"
                  tooltip="Editar producto"
                  onClick={() => {
                    setEditingProduct(rowData);
                    setProductForm({
                      nombre: rowData.nombre,
                      descripcion: rowData.descripcion || '',
                      precio: rowData.precio,
                      categoria: rowData.categoria,
                      stock: rowData.stockActual || 0,
                      stockMinimo: rowData.stockMinimo || 0,
                      stockMaximo: rowData.stockMaximo || 0,
                      unidadMedida: rowData.unidadMedida || 'Unidad'
                    });
                    setShowEditProductDialog(true);
                  }}
                />
                <Button
                  icon="pi pi-trash"
                  className="p-button-danger p-button-sm"
                  tooltip="Eliminar producto"
                  onClick={() => handleDeleteProduct(rowData.id)}
                />
              </div>
            )}
          />
        </DataTable>
      </Dialog>

      {/* Create Customer Dialog */}
      <Dialog
        header="Crear Nuevo Cliente"
        visible={showCreateCustomerDialog}
        style={{ width: '50vw', maxWidth: '600px' }}
        onHide={() => setShowCreateCustomerDialog(false)}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              className="p-button-text"
              onClick={() => setShowCreateCustomerDialog(false)}
            />
            <Button
              label="Crear Cliente"
              icon="pi pi-check"
              onClick={handleCreateCustomer}
              loading={isLoading}
              disabled={!customerForm.nombre || !customerForm.email || !customerForm.telefono}
            />
          </div>
        }
      >
        <div className="grid">
          <div className="col-12">
            <div className="p-field">
              <label htmlFor="customerName">Nombre *</label>
              <InputText
                id="customerName"
                value={customerForm.nombre}
                onChange={(e) => setCustomerForm({...customerForm, nombre: e.target.value})}
                placeholder="Nombre del cliente"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="customerEmail">Email *</label>
              <InputText
                id="customerEmail"
                value={customerForm.email}
                onChange={(e) => setCustomerForm({...customerForm, email: e.target.value})}
                placeholder="email@ejemplo.com"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="customerPhone">Teléfono *</label>
              <InputText
                id="customerPhone"
                value={customerForm.telefono}
                onChange={(e) => setCustomerForm({...customerForm, telefono: e.target.value})}
                placeholder="+57 300 123 4567"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12">
            <div className="p-field">
              <label htmlFor="customerAddress">Dirección</label>
              <InputText
                id="customerAddress"
                value={customerForm.direccion}
                onChange={(e) => setCustomerForm({...customerForm, direccion: e.target.value})}
                placeholder="Dirección completa"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="customerType">Tipo de Cliente</label>
              <Dropdown
                id="customerType"
                value={customerForm.tipoCliente}
                options={[
                  { label: 'Regular', value: 'Regular' },
                  { label: 'Corporativo', value: 'Corporativo' },
                  { label: 'VIP', value: 'VIP' }
                ]}
                onChange={(e) => setCustomerForm({...customerForm, tipoCliente: e.value})}
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="creditLimit">Límite de Crédito</label>
              <InputNumber
                id="creditLimit"
                value={customerForm.limiteCredito}
                onValueChange={(e) => setCustomerForm({...customerForm, limiteCredito: e.value || 0})}
                mode="currency"
                currency="COP"
                locale="es-CO"
                className="w-full"
              />
            </div>
          </div>
        </div>
      </Dialog>

      {/* Edit Customer Dialog */}
      <Dialog
        header="Editar Cliente"
        visible={showEditCustomerDialog}
        style={{ width: '50vw', maxWidth: '600px' }}
        onHide={() => {
          setShowEditCustomerDialog(false);
          setEditingCustomer(null);
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              className="p-button-text"
              onClick={() => {
                setShowEditCustomerDialog(false);
                setEditingCustomer(null);
              }}
            />
            <Button
              label="Actualizar Cliente"
              icon="pi pi-check"
              onClick={handleUpdateCustomer}
              loading={isLoading}
            />
          </div>
        }
      >
        <div className="grid">
          <div className="col-12">
            <div className="p-field">
              <label htmlFor="editCustomerName">Nombre *</label>
              <InputText
                id="editCustomerName"
                value={customerForm.nombre}
                onChange={(e) => setCustomerForm({...customerForm, nombre: e.target.value})}
                placeholder="Nombre del cliente"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="editCustomerEmail">Email *</label>
              <InputText
                id="editCustomerEmail"
                value={customerForm.email}
                onChange={(e) => setCustomerForm({...customerForm, email: e.target.value})}
                placeholder="email@ejemplo.com"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="editCustomerPhone">Teléfono *</label>
              <InputText
                id="editCustomerPhone"
                value={customerForm.telefono}
                onChange={(e) => setCustomerForm({...customerForm, telefono: e.target.value})}
                placeholder="+57 300 123 4567"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12">
            <div className="p-field">
              <label htmlFor="editCustomerAddress">Dirección *</label>
              <InputText
                id="editCustomerAddress"
                value={customerForm.direccion}
                onChange={(e) => setCustomerForm({...customerForm, direccion: e.target.value})}
                placeholder="Dirección completa"
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="editCustomerType">Tipo de Cliente</label>
              <Dropdown
                id="editCustomerType"
                value={customerForm.tipoCliente}
                options={[
                  { label: 'Regular', value: 'Regular' },
                  { label: 'Corporativo', value: 'Corporativo' },
                  { label: 'VIP', value: 'VIP' }
                ]}
                onChange={(e) => setCustomerForm({...customerForm, tipoCliente: e.value})}
                className="w-full"
              />
            </div>
          </div>
          <div className="col-12 md:col-6">
            <div className="p-field">
              <label htmlFor="editCreditLimit">Límite de Crédito</label>
              <InputNumber
                id="editCreditLimit"
                value={customerForm.limiteCredito}
                onValueChange={(e) => setCustomerForm({...customerForm, limiteCredito: e.value || 0})}
                mode="currency"
                currency="COP"
                locale="es-CO"
                className="w-full"
              />
            </div>
          </div>
        </div>
      </Dialog>

      <Toast ref={toast} />
      <ConfirmDialog />
    </div>
  );
};

export default SalesComponent;
