import { useCallback } from 'react';
import { useAppContext, createNotification } from '../context/AppContext';
import apiService, {
  Entrega,
  Proveedor,
  Cliente,
  Producto,
  DeliveryUpdateRequest,
  ScheduleDeliveryRequest,
  SupplierCreateRequest,
  SupplierUpdateRequest,
  CreateCustomerRequest
} from '../services/apiService';

// Custom hook for API operations
export const useApi = () => {
  const { state, dispatch } = useAppContext();

  // Utility function to handle errors
  const handleError = useCallback((error: any, defaultMessage: string) => {
    const message = error.response?.data?.message || error.message || defaultMessage;
    dispatch({
      type: 'ADD_NOTIFICATION',
      payload: createNotification('error', 'Error', message)
    });
    console.error('API Error:', error);
  }, [dispatch]);

  // Utility function to show success messages
  const showSuccess = useCallback((title: string, message: string) => {
    dispatch({
      type: 'ADD_NOTIFICATION',
      payload: createNotification('success', title, message)
    });
  }, [dispatch]);

  // System Health Operations
  const loadSystemHealth = useCallback(async () => {
    dispatch({ type: 'SET_LOADING_HEALTH', payload: true });
    try {
      const health = await apiService.getSystemHealth();
      dispatch({ type: 'SET_SYSTEM_HEALTH', payload: health });
    } catch (error) {
      handleError(error, 'Error loading system health');
    } finally {
      dispatch({ type: 'SET_LOADING_HEALTH', payload: false });
    }
  }, [dispatch, handleError]);

  // Delivery Operations
  const loadDeliveries = useCallback(async (vendorId?: string) => {
    dispatch({ type: 'SET_LOADING_DELIVERIES', payload: true });
    try {
      const deliveries = await apiService.getDeliveries(vendorId);
      dispatch({ type: 'SET_DELIVERIES', payload: deliveries });
    } catch (error) {
      handleError(error, 'Error loading deliveries');
    } finally {
      dispatch({ type: 'SET_LOADING_DELIVERIES', payload: false });
    }
  }, [dispatch, handleError]);

  const loadDeliveryById = useCallback(async (id: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const delivery = await apiService.getDeliveryById(id);
      dispatch({ type: 'SET_SELECTED_DELIVERY', payload: delivery });
      return delivery;
    } catch (error) {
      handleError(error, 'Error loading delivery details');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  const confirmDelivery = useCallback(async (id: string, vendorId?: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      await apiService.confirmDelivery(id);
      showSuccess('Delivery Confirmed', 'The delivery has been confirmed successfully');

      // Reload deliveries to get updated status
      await loadDeliveries(vendorId);
    } catch (error) {
      handleError(error, 'Error confirming delivery');
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess, loadDeliveries]);

  const updateDeliveryStatus = useCallback(async (request: DeliveryUpdateRequest) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const updatedDelivery = await apiService.updateDeliveryStatus(request);
      dispatch({ type: 'UPDATE_DELIVERY', payload: updatedDelivery });
      showSuccess('Status Updated', 'Delivery status has been updated successfully');
      return updatedDelivery;
    } catch (error) {
      handleError(error, 'Error updating delivery status');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);

  const scheduleDelivery = useCallback(async (request: ScheduleDeliveryRequest) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const newDelivery = await apiService.scheduleDelivery(request);
      dispatch({ type: 'ADD_DELIVERY', payload: newDelivery });
      showSuccess('Delivery Scheduled', 'New delivery has been scheduled successfully');
      return newDelivery;
    } catch (error) {
      handleError(error, 'Error scheduling delivery');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);





  const getDeliveryTracking = useCallback(async (id: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const tracking = await apiService.getDeliveryTracking(id);
      return tracking;
    } catch (error) {
      handleError(error, 'Error loading delivery tracking');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  // Supplier Operations
  const loadSuppliers = useCallback(async () => {
    dispatch({ type: 'SET_LOADING_SUPPLIERS', payload: true });
    try {
      const suppliers = await apiService.getSuppliers();
      dispatch({ type: 'SET_SUPPLIERS', payload: suppliers });
    } catch (error) {
      handleError(error, 'Error loading suppliers');
    } finally {
      dispatch({ type: 'SET_LOADING_SUPPLIERS', payload: false });
    }
  }, [dispatch, handleError]);

  const loadSupplierById = useCallback(async (id: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const supplier = await apiService.getSupplierById(id);
      dispatch({ type: 'SET_SELECTED_SUPPLIER', payload: supplier });
      return supplier;
    } catch (error) {
      handleError(error, 'Error loading supplier details');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  const createSupplier = useCallback(async (supplier: SupplierCreateRequest) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const newSupplier = await apiService.createSupplier(supplier);
      dispatch({ type: 'ADD_SUPPLIER', payload: newSupplier });
      showSuccess('Supplier Created', `Supplier ${newSupplier.nombre} has been created successfully`);
      return newSupplier;
    } catch (error) {
      handleError(error, 'Error creating supplier');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);

  const updateSupplier = useCallback(async (supplier: SupplierUpdateRequest) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const updatedSupplier = await apiService.updateSupplier(supplier);
      dispatch({ type: 'UPDATE_SUPPLIER', payload: updatedSupplier });
      showSuccess('Supplier Updated', `Supplier ${updatedSupplier.nombre} has been updated successfully`);
      return updatedSupplier;
    } catch (error) {
      handleError(error, 'Error updating supplier');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);

  const deactivateSupplier = useCallback(async (id: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      await apiService.deactivateSupplier(id);
      showSuccess('Supplier Deactivated', 'Supplier has been deactivated successfully');
      
      // Reload suppliers to get updated status
      await loadSuppliers();
    } catch (error) {
      handleError(error, 'Error deactivating supplier');
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess, loadSuppliers]);

  const evaluateSupplier = useCallback(async (id: string, calificacion: number, comentarios?: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      await apiService.evaluateSupplier(id, calificacion, comentarios);
      showSuccess('Supplier Evaluated', 'Supplier evaluation has been submitted successfully');
      
      // Reload suppliers to get updated ratings
      await loadSuppliers();
    } catch (error) {
      handleError(error, 'Error evaluating supplier');
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess, loadSuppliers]);

  const getSupplierPerformance = useCallback(async (id: string) => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const performance = await apiService.getSupplierPerformance(id);
      return performance;
    } catch (error) {
      handleError(error, 'Error loading supplier performance');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  // Utility Operations
  const testConnection = useCallback(async () => {
    dispatch({ type: 'SET_LOADING', payload: true });
    try {
      const isConnected = await apiService.testConnection();
      if (isConnected) {
        showSuccess('Connection Test', 'API connection is working properly');
      } else {
        dispatch({
          type: 'ADD_NOTIFICATION',
          payload: createNotification('warning', 'Connection Test', 'API connection test failed')
        });
      }
      return isConnected;
    } catch (error) {
      handleError(error, 'Error testing API connection');
      return false;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);

  // Customer Operations
  const loadCustomers = useCallback(async (page: number = 1, pageSize: number = 50) => {
    try {
      dispatch({ type: 'SET_LOADING_CUSTOMERS', payload: true });
      const response = await apiService.getCustomers(page, pageSize);
      dispatch({ type: 'SET_CUSTOMERS', payload: response.customers });
      return response;
    } catch (error) {
      console.error('Error loading customers:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al cargar clientes') });
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING_CUSTOMERS', payload: false });
    }
  }, [dispatch]);

  const createCustomer = useCallback(async (customer: CreateCustomerRequest) => {
    try {
      const newCustomer = await apiService.createCustomer(customer);
      dispatch({ type: 'ADD_CUSTOMER', payload: newCustomer });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Cliente creado exitosamente') });
      return newCustomer;
    } catch (error) {
      console.error('Error creating customer:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al crear cliente') });
      return null;
    }
  }, [dispatch]);

  const updateCustomer = useCallback(async (id: string, customer: CreateCustomerRequest) => {
    try {
      const updatedCustomer = await apiService.updateCustomer(id, customer);
      dispatch({ type: 'UPDATE_CUSTOMER', payload: updatedCustomer });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Cliente actualizado exitosamente') });
      return updatedCustomer;
    } catch (error) {
      console.error('Error updating customer:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al actualizar cliente') });
      return null;
    }
  }, [dispatch]);

  const deleteCustomer = useCallback(async (id: string) => {
    try {
      await apiService.deleteCustomer(id);
      dispatch({ type: 'DELETE_CUSTOMER', payload: id });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Cliente eliminado exitosamente') });
      return true;
    } catch (error) {
      console.error('Error deleting customer:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al eliminar cliente') });
      return false;
    }
  }, [dispatch]);

  // Product Operations
  const loadProducts = useCallback(async (page: number = 1, pageSize: number = 50) => {
    try {
      dispatch({ type: 'SET_LOADING_PRODUCTS', payload: true });
      // Only load active products to match Angular app behavior
      const response = await apiService.getProducts(page, pageSize, undefined, true);

      // Convert API products to component format (same logic as SalesComponent)
      const convertedProducts = response.products.map((producto: any) => {
        return {
          id: producto.id,
          nombre: producto.nombre,
          precio: producto.precio,
          stock: producto.stock || 0, // Keep original stock field for interface compatibility
          stockDisponible: producto.stock || 0, // Use the stock field from API
          stockActual: producto.stock || 0, // Also map to stockActual for compatibility
          stockMinimo: producto.stockMinimo || 0,
          stockMaximo: producto.stockMaximo || 0,
          categoria: producto.categoria,
          descripcion: producto.descripcion || '',
          unidadMedida: producto.unidadMedida || 'Unidad',
          estado: producto.estado,
          fechaCreacion: producto.fechaCreacion,
          fechaActualizacion: producto.fechaActualizacion
        };
      });

      // Store converted products in global state
      dispatch({ type: 'SET_PRODUCTS', payload: convertedProducts });

      // Return response with converted products
      return { ...response, products: convertedProducts };
    } catch (error) {
      console.error('Error loading products:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al cargar productos') });
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING_PRODUCTS', payload: false });
    }
  }, [dispatch]);

  const createProduct = useCallback(async (product: any) => {
    try {
      const newProduct = await apiService.createProduct(product);
      dispatch({ type: 'ADD_PRODUCT', payload: newProduct });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Producto creado exitosamente') });
      return newProduct;
    } catch (error) {
      console.error('Error creating product:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al crear producto') });
      return null;
    }
  }, [dispatch]);

  const updateProduct = useCallback(async (id: string, product: any) => {
    try {
      const updatedProduct = await apiService.updateProduct(id, product);
      dispatch({ type: 'UPDATE_PRODUCT', payload: updatedProduct });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Producto actualizado exitosamente') });
      return updatedProduct;
    } catch (error) {
      console.error('Error updating product:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al actualizar producto') });
      return null;
    }
  }, [dispatch]);

  const deleteProduct = useCallback(async (id: string) => {
    try {
      await apiService.deleteProduct(id);
      dispatch({ type: 'DELETE_PRODUCT', payload: id });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Producto eliminado exitosamente') });
      return true;
    } catch (error) {
      console.error('Error deleting product:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al eliminar producto') });
      return false;
    }
  }, [dispatch]);

  // Sales Operations
  const createSale = useCallback(async (saleRequest: any) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const response = await apiService.createSale(saleRequest);
      showSuccess('Venta Creada', 'La venta se ha procesado exitosamente');
      return response;
    } catch (error) {
      handleError(error, 'Error al crear la venta');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError, showSuccess]);

  const loadSales = useCallback(async (vendedorId?: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const response = await apiService.getSales(vendedorId);
      dispatch({ type: 'SET_SALES', payload: response });
      return response;
    } catch (error) {
      handleError(error, 'Error al cargar las ventas');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  const getSaleById = useCallback(async (id: string) => {
    try {
      const response = await apiService.getSaleById(id);
      return response;
    } catch (error) {
      console.error('Error getting sale:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al obtener la venta') });
      return null;
    }
  }, [dispatch]);

  const updateSale = useCallback(async (id: string, saleRequest: any) => {
    try {
      const updatedSale = await apiService.updateSale(id, saleRequest);
      dispatch({ type: 'UPDATE_SALE', payload: updatedSale });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Venta actualizada exitosamente') });
      return updatedSale;
    } catch (error) {
      console.error('Error updating sale:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al actualizar la venta') });
      return null;
    }
  }, [dispatch]);

  const deleteSale = useCallback(async (id: string, motivo: string) => {
    try {
      await apiService.deleteSale(id, motivo);
      dispatch({ type: 'DELETE_SALE', payload: id });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Venta cancelada exitosamente') });
      return true;
    } catch (error) {
      console.error('Error deleting sale:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al cancelar la venta') });
      return false;
    }
  }, [dispatch]);

  const applyDiscount = useCallback(async (id: string, descuento: number, motivo: string) => {
    try {
      const updatedSale = await apiService.applyDiscount(id, descuento, motivo);
      dispatch({ type: 'UPDATE_SALE', payload: updatedSale });
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('success', 'Éxito', 'Descuento aplicado exitosamente') });
      return updatedSale;
    } catch (error) {
      console.error('Error applying discount:', error);
      dispatch({ type: 'ADD_NOTIFICATION', payload: createNotification('error', 'Error', 'Error al aplicar descuento') });
      return null;
    }
  }, [dispatch]);

  // Authorization Operations
  const validateSeller = useCallback(async (codigoVendedor: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      const response = await apiService.validateSeller(codigoVendedor);
      return response;
    } catch (error) {
      handleError(error, 'Error al validar vendedor');
      return null;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  }, [dispatch, handleError]);

  const getSellerByCode = useCallback(async (codigoVendedor: string) => {
    try {
      const response = await apiService.getSellerByCode(codigoVendedor);
      return response;
    } catch (error) {
      handleError(error, 'Error al obtener información del vendedor');
      return null;
    }
  }, [handleError]);

  // Inventory Operations
  const checkInventoryAvailability = useCallback(async (productoId: string, cantidad: number) => {
    try {
      const response = await apiService.checkInventoryAvailability(productoId, cantidad);
      return response;
    } catch (error) {
      handleError(error, 'Error al verificar disponibilidad');
      return null;
    }
  }, [handleError]);

  const getCurrentStock = useCallback(async (productoId: string) => {
    try {
      const response = await apiService.getCurrentStock(productoId);
      return response;
    } catch (error) {
      handleError(error, 'Error al obtener stock actual');
      return null;
    }
  }, [handleError]);

  // Return all operations
  return {
    // System
    loadSystemHealth,
    testConnection,

    // Deliveries
    loadDeliveries,
    loadDeliveryById,
    confirmDelivery,
    updateDeliveryStatus,
    scheduleDelivery,
    getDeliveryTracking,

    // Suppliers
    loadSuppliers,
    loadSupplierById,
    createSupplier,
    updateSupplier,
    deactivateSupplier,
    evaluateSupplier,
    getSupplierPerformance,

    // Customers
    loadCustomers,
    createCustomer,
    updateCustomer,
    deleteCustomer,

    // Products
    loadProducts,
    createProduct,
    updateProduct,
    deleteProduct,

    // Sales
    createSale,
    loadSales,
    getSaleById,
    updateSale,
    deleteSale,
    applyDiscount,

    // Authorization
    validateSeller,
    getSellerByCode,

    // Inventory
    checkInventoryAvailability,
    getCurrentStock,

    // State
    state,
    dispatch,
  };
};
