import React, { useEffect, useState } from 'react';

// Styles
import './EnhancedFormStyles.css';

// PrimeReact Imports
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { DataTable } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { Tag } from 'primereact/tag';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { Calendar } from 'primereact/calendar';
import { InputTextarea } from 'primereact/inputtextarea';
import { Panel } from 'primereact/panel';
import { Divider } from 'primereact/divider';
import { ProgressSpinner } from 'primereact/progressspinner';
import { Timeline } from 'primereact/timeline';

// Context and Hooks
import { useAppContext, useAuth } from '../context/AppContext';
import { useApi } from '../hooks/useApi';
import { Entrega, DeliveryUpdateRequest, DeliveryTrackingInfo, Cliente } from '../services/apiService';

const DeliveryComponent: React.FC = () => {
  const { state } = useAppContext();
  const { user, getCurrentUserId } = useAuth();
  const {
    loadDeliveries,
    confirmDelivery,
    updateDeliveryStatus,
    scheduleDelivery,
    getDeliveryTracking,
    loadCustomers,
    loadSales
  } = useApi();

  // Local state
  const [selectedDelivery, setSelectedDelivery] = useState<Entrega | null>(null);
  const [showUpdateDialog, setShowUpdateDialog] = useState(false);
  const [showScheduleDialog, setShowScheduleDialog] = useState(false);
  const [showTrackingDialog, setShowTrackingDialog] = useState(false);
  const [trackingInfo, setTrackingInfo] = useState<DeliveryTrackingInfo | null>(null);
  const [isLoadingTracking, setIsLoadingTracking] = useState(false);
  const [availableSales, setAvailableSales] = useState<any[]>([]);

  // Form states
  const [updateForm, setUpdateForm] = useState<DeliveryUpdateRequest>({
    id: '',
    estado: '',
    observaciones: '',
    fechaEntrega: ''
  });

  const [scheduleForm, setScheduleForm] = useState<Partial<Entrega>>({
    idVenta: '',
    cliente: '',
    direccion: '',
    transportista: '',
    fechaProgramada: '',
    observaciones: ''
  });

  const statusOptions = [
    { label: 'Programada', value: 'Programada' },
    { label: 'En Tránsito', value: 'En Transito' },
    { label: 'Entregada', value: 'Entregada' },
    { label: 'Cancelada', value: 'Cancelada' }
  ];

  useEffect(() => {
    // Use vendedorId if available, otherwise use user id
    const vendorId = user?.vendedorId || user?.id;
    if (vendorId) {
      loadDeliveries(vendorId);
      loadSales(vendorId).then(sales => setAvailableSales(sales || []));
    }
    loadCustomers();
  }, [loadDeliveries, loadCustomers, loadSales, user?.vendedorId, user?.id]);

  // Event Handlers
  const handleConfirmDelivery = async (delivery: Entrega) => {
    const currentUserId = getCurrentUserId();
    await confirmDelivery(delivery.id, currentUserId || undefined);
  };

  const handleUpdateStatus = () => {
    if (selectedDelivery) {
      setUpdateForm({
        id: selectedDelivery.id || '',
        estado: selectedDelivery.estado || 'Programada',
        observaciones: selectedDelivery.observaciones || '',
        fechaEntrega: selectedDelivery.fechaEntrega || ''
      });
      setShowUpdateDialog(true);
    }
  };

  const handleSubmitUpdate = async () => {
    if (updateForm.id) {
      const result = await updateDeliveryStatus(updateForm);
      if (result) {
        setShowUpdateDialog(false);
        setSelectedDelivery(null);
        resetUpdateForm();
      }
    }
  };

  const handleScheduleDelivery = async () => {
    const currentUserId = getCurrentUserId();
    if (!currentUserId) {
      console.error('No user logged in');
      return;
    }

    // Create the proper request object
    const deliveryRequest = {
      idVenta: scheduleForm.idVenta || '',
      fechaProgramada: scheduleForm.fechaProgramada || '',
      direccion: scheduleForm.direccion || '',
      cliente: scheduleForm.cliente || '',
      transportistaId: scheduleForm.transportista || undefined,
      observaciones: scheduleForm.observaciones || undefined
    };

    const result = await scheduleDelivery(deliveryRequest);
    if (result) {
      setShowScheduleDialog(false);
      resetScheduleForm();
    }
  };

  const handleShowTracking = async (delivery: Entrega) => {
    setSelectedDelivery(delivery);
    setShowTrackingDialog(true);
    setIsLoadingTracking(true);
    
    try {
      const tracking = await getDeliveryTracking(delivery.id);
      setTrackingInfo(tracking);
    } catch (error) {
      console.error('Error loading tracking:', error);
    } finally {
      setIsLoadingTracking(false);
    }
  };

  const resetUpdateForm = () => {
    setUpdateForm({
      id: '',
      estado: '',
      observaciones: '',
      fechaEntrega: ''
    });
  };

  const resetScheduleForm = () => {
    setScheduleForm({
      idVenta: '',
      cliente: '',
      direccion: '',
      transportista: '',
      fechaProgramada: '',
      observaciones: ''
    });
  };

  // Template Functions
  const statusBodyTemplate = (rowData: Entrega) => {
    const getSeverity = (status: string): "success" | "warning" | "danger" | "info" => {
      switch (status) {
        case 'Entregada': return 'success';
        case 'En Transito': return 'info';
        case 'Programada': return 'warning';
        case 'Cancelada': return 'danger';
        default: return 'info';
      }
    };

    const estado = rowData?.estado || 'Programada';
    return <Tag value={estado} severity={getSeverity(estado)} />;
  };

  const dateBodyTemplate = (rowData: Entrega) => {
    const fecha = rowData?.fechaProgramada;
    if (!fecha) return 'No programada';
    return new Date(fecha).toLocaleDateString();
  };

  const actionsBodyTemplate = (rowData: Entrega) => {
    return (
      <div className="flex gap-2">
        <Button
          icon="pi pi-eye"
          className="p-button-rounded p-button-text p-button-sm"
          onClick={() => handleShowTracking(rowData)}
          tooltip="Ver Seguimiento"
        />
        <Button
          icon="pi pi-pencil"
          className="p-button-rounded p-button-text p-button-sm"
          onClick={() => {
            setSelectedDelivery(rowData);
            handleUpdateStatus();
          }}
          tooltip="Actualizar Estado"
        />
        <Button
          icon="pi pi-check"
          className="p-button-rounded p-button-success p-button-sm"
          onClick={() => handleConfirmDelivery(rowData)}
          disabled={rowData?.estado === 'Entregada'}
          tooltip="Confirmar Entrega"
        />
      </div>
    );
  };

  return (
    <div className="delivery-container">
      {/* Header */}
      <div className="header-section mb-4">
        <h1 className="text-center">
          <i className="pi pi-truck text-primary"></i>
          RF4: Gestión de Entregas
        </h1>
        <p className="text-center text-secondary">
          Component-Based Software Engineering - Delivery Component
        </p>
      </div>

      {/* Quick Actions */}
      <Card className="mb-4 fade-in">
        <div className="card-header-content">
          <h3><i className="pi pi-bolt"></i> Acciones Rápidas</h3>
          <div className="flex gap-2">
            <Button
              label="Programar Entrega"
              icon="pi pi-plus"
              onClick={() => {
                resetScheduleForm();
                setShowScheduleDialog(true);
              }}
              className="p-button-primary"
            />
            <Button
              label="Actualizar Lista"
              icon="pi pi-refresh"
              onClick={() => {
                const currentUserId = getCurrentUserId();
                if (currentUserId) {
                  loadDeliveries(currentUserId);
                }
              }}
              loading={state.isLoadingDeliveries}
              className="p-button-outlined"
            />
          </div>
        </div>
      </Card>

      {/* Deliveries Table */}
      <Card className="fade-in">
        <div className="card-header-content">
          <h3><i className="pi pi-list"></i> Lista de Entregas ({state.deliveries.length})</h3>
        </div>

        <DataTable
          value={state.deliveries}
          paginator
          rows={10}
          rowsPerPageOptions={[5, 10, 20]}
          loading={state.isLoadingDeliveries}
          emptyMessage="No hay entregas registradas"
          className="p-datatable-striped"
          showGridlines
          responsiveLayout="scroll"
        >
          <Column field="idVenta" header="ID Venta" sortable />
          <Column field="cliente" header="Cliente" sortable />
          <Column field="direccion" header="Dirección" />
          <Column field="transportista" header="Transportista" sortable />
          <Column 
            field="fechaProgramada" 
            header="Fecha Programada" 
            body={dateBodyTemplate}
            sortable 
          />
          <Column 
            field="estado" 
            header="Estado" 
            body={statusBodyTemplate}
            sortable 
          />
          <Column 
            header="Acciones" 
            body={actionsBodyTemplate}
            style={{ width: '12rem' }}
          />
        </DataTable>
      </Card>

      {/* Update Status Dialog */}
      <Dialog
        header="Actualizar Estado de Entrega"
        visible={showUpdateDialog}
        style={{ width: '450px' }}
        onHide={() => {
          setShowUpdateDialog(false);
          setSelectedDelivery(null);
          resetUpdateForm();
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              onClick={() => setShowUpdateDialog(false)}
              className="p-button-text"
            />
            <Button
              label="Actualizar"
              icon="pi pi-check"
              onClick={handleSubmitUpdate}
              disabled={!updateForm.estado}
            />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field">
            <label htmlFor="estado">Estado</label>
            <Dropdown
              id="estado"
              value={updateForm.estado}
              options={statusOptions}
              onChange={(e) => setUpdateForm({ ...updateForm, estado: e.value })}
              placeholder="Seleccione estado"
            />
          </div>
          
          <div className="field">
            <label htmlFor="fechaEntrega">Fecha de Entrega</label>
            <Calendar
              id="fechaEntrega"
              value={updateForm.fechaEntrega ? new Date(updateForm.fechaEntrega) : null}
              onChange={(e) => setUpdateForm({ 
                ...updateForm, 
                fechaEntrega: e.value ? e.value.toISOString() : '' 
              })}
              showTime
              hourFormat="24"
            />
          </div>
          
          <div className="field">
            <label htmlFor="observaciones">Observaciones</label>
            <InputTextarea
              id="observaciones"
              value={updateForm.observaciones}
              onChange={(e) => setUpdateForm({ ...updateForm, observaciones: e.target.value })}
              rows={3}
              placeholder="Ingrese observaciones..."
            />
          </div>
        </div>
      </Dialog>

      {/* Schedule Delivery Dialog */}
      <Dialog
        header="Programar Nueva Entrega"
        visible={showScheduleDialog}
        style={{ width: '600px' }}
        onHide={() => {
          setShowScheduleDialog(false);
          resetScheduleForm();
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              onClick={() => setShowScheduleDialog(false)}
              className="p-button-text"
            />
            <Button
              label="Programar"
              icon="pi pi-check"
              onClick={handleScheduleDelivery}
              disabled={!scheduleForm.idVenta || !scheduleForm.cliente}
            />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="idVenta">Seleccionar Venta *</label>
              <Dropdown
                id="idVenta"
                value={scheduleForm.idVenta}
                options={availableSales.map(sale => ({
                  label: `${sale.id.substring(0, 8)}... - Cliente: ${sale.idCliente} - $${sale.total}`,
                  value: sale.id
                }))}
                onChange={(e) => {
                  const selectedSale = availableSales.find(sale => sale.id === e.value);
                  const customer = state.customers.find(c => c.id === selectedSale?.idCliente);
                  setScheduleForm({
                    ...scheduleForm,
                    idVenta: e.value,
                    cliente: customer?.nombre || selectedSale?.idCliente || '',
                    direccion: customer?.direccion || ''
                  });
                }}
                placeholder="Seleccione una venta"
                filter
                showClear
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="cliente">Cliente *</label>
              <InputText
                id="cliente"
                value={scheduleForm.cliente}
                readOnly
                className="p-inputtext-readonly"
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="direccion">Dirección</label>
            <InputText
              id="direccion"
              value={scheduleForm.direccion}
              onChange={(e) => setScheduleForm({ ...scheduleForm, direccion: e.target.value })}
              placeholder="Dirección de entrega"
              readOnly={!!scheduleForm.idVenta}
              className={scheduleForm.idVenta ? "p-inputtext-readonly" : ""}
            />
          </div>
          
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="transportista">Transportista</label>
              <InputText
                id="transportista"
                value={scheduleForm.transportista}
                onChange={(e) => setScheduleForm({ ...scheduleForm, transportista: e.target.value })}
                placeholder="Nombre del transportista"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="fechaProgramada">Fecha Programada</label>
              <Calendar
                id="fechaProgramada"
                value={scheduleForm?.fechaProgramada ? new Date(scheduleForm.fechaProgramada) : null}
                onChange={(e) => setScheduleForm({
                  ...scheduleForm,
                  fechaProgramada: e.value ? e.value.toISOString() : ''
                })}
                minDate={new Date()}
                dateFormat="dd/mm/yy"
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="observacionesSchedule">Observaciones</label>
            <InputTextarea
              id="observacionesSchedule"
              value={scheduleForm.observaciones}
              onChange={(e) => setScheduleForm({ ...scheduleForm, observaciones: e.target.value })}
              rows={3}
              placeholder="Observaciones adicionales..."
            />
          </div>
        </div>
      </Dialog>

      {/* Tracking Dialog */}
      <Dialog
        header={`Seguimiento de Entrega - ${selectedDelivery?.idVenta}`}
        visible={showTrackingDialog}
        style={{ width: '700px' }}
        onHide={() => {
          setShowTrackingDialog(false);
          setSelectedDelivery(null);
          setTrackingInfo(null);
        }}
      >
        {isLoadingTracking ? (
          <div className="text-center p-4">
            <ProgressSpinner />
            <p>Cargando información de seguimiento...</p>
          </div>
        ) : trackingInfo ? (
          <div>
            <Panel header="Información Actual" className="mb-3">
              <div className="grid">
                <div className="col-12 md:col-6">
                  <p><strong>Ubicación Actual:</strong> {trackingInfo.ubicacionActual}</p>
                  <p><strong>Estado:</strong> {trackingInfo.estadoDetallado}</p>
                </div>
                <div className="col-12 md:col-6">
                  <p><strong>Tiempo Estimado:</strong> {trackingInfo.tiempoEstimado}</p>
                </div>
              </div>
            </Panel>

            <Panel header="Historial de Movimientos">
              <Timeline
                value={trackingInfo.historialMovimientos}
                content={(item) => (
                  <div>
                    <h5>{item.ubicacion}</h5>
                    <p><strong>Estado:</strong> {item.estado}</p>
                    <p>{item.observaciones}</p>
                    <small>{new Date(item.fecha).toLocaleString()}</small>
                  </div>
                )}
              />
            </Panel>
          </div>
        ) : (
          <div className="text-center p-4">
            <p>No se pudo cargar la información de seguimiento.</p>
          </div>
        )}
      </Dialog>
    </div>
  );
};

export default DeliveryComponent;
