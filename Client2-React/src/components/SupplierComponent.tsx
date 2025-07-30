import React, { useEffect, useState } from 'react';

// Styles
import './SupplierComponent.css';
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
import { InputTextarea } from 'primereact/inputtextarea';
import { InputNumber } from 'primereact/inputnumber';
import { Panel } from 'primereact/panel';
import { Divider } from 'primereact/divider';
import { Rating } from 'primereact/rating';
import { ProgressBar } from 'primereact/progressbar';
import { Chip } from 'primereact/chip';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';

// Context and Hooks
import { useAppContext } from '../context/AppContext';
import { useApi } from '../hooks/useApi';
import { 
  Proveedor, 
  SupplierCreateRequest, 
  SupplierUpdateRequest,
  SupplierPerformanceMetrics 
} from '../services/apiService';

const SupplierComponent: React.FC = () => {
  const { state } = useAppContext();
  const { 
    loadSuppliers, 
    createSupplier, 
    updateSupplier, 
    deactivateSupplier,
    evaluateSupplier,
    getSupplierPerformance 
  } = useApi();

  // Local state
  const [selectedSupplier, setSelectedSupplier] = useState<Proveedor | null>(null);
  const [showCreateDialog, setShowCreateDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [showEvaluationDialog, setShowEvaluationDialog] = useState(false);
  const [showPerformanceDialog, setShowPerformanceDialog] = useState(false);
  const [performanceData, setPerformanceData] = useState<SupplierPerformanceMetrics | null>(null);
  const [isLoadingPerformance, setIsLoadingPerformance] = useState(false);

  // Form states
  const [createForm, setCreateForm] = useState<SupplierCreateRequest>({
    nombre: '',
    contacto: '',
    direccion: '',
    telefono: '',
    email: '',
    categoria: '',
    condicionesPago: '',
    tiempoEntrega: 0
  });

  const [editForm, setEditForm] = useState<SupplierUpdateRequest>({
    id: '',
    nombre: '',
    contacto: '',
    direccion: '',
    telefono: '',
    email: '',
    categoria: '',
    condicionesPago: '',
    tiempoEntrega: 0,
    activo: true
  });

  const [evaluationForm, setEvaluationForm] = useState({
    calificacion: 0,
    comentarios: ''
  });

  const categoryOptions = [
    { label: 'Seleccione categoría...', value: '' },
    { label: 'Tecnología', value: 'Tecnologia' },
    { label: 'Oficina', value: 'Oficina' },
    { label: 'Logística', value: 'Logistica' },
    { label: 'Servicios', value: 'Servicios' },
    { label: 'Materiales', value: 'Materiales' }
  ];

  useEffect(() => {
    loadSuppliers();
  }, [loadSuppliers]);

  // Event Handlers
  const handleCreateSupplier = async () => {
    try {
      const result = await createSupplier(createForm);
      if (result) {
        setShowCreateDialog(false);
        resetCreateForm();
      }
    } catch (error) {
      console.error('Error creating supplier:', error);
    }
  };

  const handleEditSupplier = (supplier: Proveedor) => {
    setEditForm({
      id: supplier.id,
      nombre: supplier.nombre,
      contacto: supplier.contacto,
      direccion: supplier.direccion,
      telefono: supplier.telefono,
      email: supplier.email,
      categoria: supplier.categoria,
      condicionesPago: supplier.condicionesPago || '',
      tiempoEntrega: supplier.tiempoEntrega || 0,
      activo: supplier.activo
    });
    setSelectedSupplier(supplier);
    setShowEditDialog(true);
  };

  const handleUpdateSupplier = async () => {
    try {
      const result = await updateSupplier(editForm);
      if (result) {
        setShowEditDialog(false);
        setSelectedSupplier(null);
        resetEditForm();
      }
    } catch (error) {
      console.error('Error updating supplier:', error);
    }
  };

  const handleDeactivateSupplier = async (supplier: Proveedor) => {
    confirmDialog({
      message: `¿Está seguro de que desea desactivar al proveedor ${supplier.nombre}?`,
      header: 'Confirmar Desactivación',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sí, Desactivar',
      rejectLabel: 'Cancelar',
      acceptClassName: 'p-button-danger',
      accept: async () => {
        await deactivateSupplier(supplier.id);
      }
    });
  };

  const handleEvaluateSupplier = (supplier: Proveedor) => {
    setSelectedSupplier(supplier);
    setEvaluationForm({
      calificacion: supplier.calificacion || 0,
      comentarios: ''
    });
    setShowEvaluationDialog(true);
  };

  const handleSubmitEvaluation = async () => {
    if (selectedSupplier) {
      try {
        await evaluateSupplier(
          selectedSupplier.id,
          evaluationForm.calificacion,
          evaluationForm.comentarios
        );
        setShowEvaluationDialog(false);
        setSelectedSupplier(null);
        resetEvaluationForm();
      } catch (error) {
        console.error('Error evaluating supplier:', error);
      }
    }
  };

  const handleShowPerformance = async (supplier: Proveedor) => {
    setSelectedSupplier(supplier);
    setShowPerformanceDialog(true);
    setIsLoadingPerformance(true);
    
    try {
      const performance = await getSupplierPerformance(supplier.id);
      setPerformanceData(performance);
    } catch (error) {
      console.error('Error loading performance:', error);
    } finally {
      setIsLoadingPerformance(false);
    }
  };

  const resetCreateForm = () => {
    setCreateForm({
      nombre: '',
      contacto: '',
      direccion: '',
      telefono: '',
      email: '',
      categoria: '',
      condicionesPago: '',
      tiempoEntrega: 0
    });
  };

  const resetEditForm = () => {
    setEditForm({
      id: '',
      nombre: '',
      contacto: '',
      direccion: '',
      telefono: '',
      email: '',
      categoria: '',
      condicionesPago: '',
      tiempoEntrega: 0,
      activo: true
    });
  };

  const resetEvaluationForm = () => {
    setEvaluationForm({
      calificacion: 0,
      comentarios: ''
    });
  };

  // Template Functions
  const statusBodyTemplate = (rowData: Proveedor) => {
    return (
      <Tag 
        value={rowData.activo ? 'Activo' : 'Inactivo'} 
        severity={rowData.activo ? 'success' : 'danger'} 
      />
    );
  };

  const categoryBodyTemplate = (rowData: Proveedor) => {
    return <Chip label={rowData.categoria} />;
  };

  const ratingBodyTemplate = (rowData: Proveedor) => {
    return (
      <div className="flex align-items-center">
        <Rating 
          value={rowData.calificacion || 0} 
          readOnly 
          cancel={false}
          className="mr-2"
        />
        <span>({rowData.calificacion?.toFixed(1) || 'N/A'})</span>
      </div>
    );
  };

  const deliveryTimeBodyTemplate = (rowData: Proveedor) => {
    return rowData.tiempoEntrega ? `${rowData.tiempoEntrega} días` : 'N/A';
  };

  const actionsBodyTemplate = (rowData: Proveedor) => {
    return (
      <div className="flex gap-2">
        <Button
          icon="pi pi-chart-line"
          className="p-button-rounded p-button-text p-button-sm"
          onClick={() => handleShowPerformance(rowData)}
          tooltip="Ver Rendimiento"
        />
        <Button
          icon="pi pi-star"
          className="p-button-rounded p-button-text p-button-sm"
          onClick={() => handleEvaluateSupplier(rowData)}
          tooltip="Evaluar Proveedor"
        />
        <Button
          icon="pi pi-pencil"
          className="p-button-rounded p-button-text p-button-sm"
          onClick={() => handleEditSupplier(rowData)}
          tooltip="Editar"
        />
        <Button
          icon="pi pi-times"
          className="p-button-rounded p-button-danger p-button-sm"
          onClick={() => handleDeactivateSupplier(rowData)}
          disabled={!rowData.activo}
          tooltip="Desactivar"
        />
      </div>
    );
  };

  return (
    <div className="supplier-container">
      {/* Header */}
      <div className="header-section mb-4">
        <h1 className="text-center">
          <i className="pi pi-building text-primary"></i>
          RF5: Gestión de Proveedores
        </h1>
        <p className="text-center text-secondary">
          Component-Based Software Engineering - Supplier Component
        </p>
      </div>

      {/* Quick Actions */}
      <Card className="mb-4 fade-in">
        <div className="card-header-content">
          <h3><i className="pi pi-bolt"></i> Acciones Rápidas</h3>
          <div className="flex gap-2">
            <Button
              label="Nuevo Proveedor"
              icon="pi pi-plus"
              onClick={() => setShowCreateDialog(true)}
              className="p-button-primary"
            />
            <Button
              label="Actualizar Lista"
              icon="pi pi-refresh"
              onClick={loadSuppliers}
              loading={state.isLoadingSuppliers}
              className="p-button-outlined"
            />
          </div>
        </div>
      </Card>

      {/* Suppliers Statistics */}
      <div className="stats-grid mb-4">
        <div className="stat-card">
          <i className="pi pi-building text-blue-500"></i>
          <div className="stat-content">
            <h3>{state.suppliers.length}</h3>
            <p>Total Proveedores</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-check-circle text-green-500"></i>
          <div className="stat-content">
            <h3>{state.suppliers.filter(s => s.activo).length}</h3>
            <p>Activos</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-tags text-purple-500"></i>
          <div className="stat-content">
            <h3>{new Set(state.suppliers.map(s => s.categoria)).size}</h3>
            <p>Categorías</p>
          </div>
        </div>
        <div className="stat-card">
          <i className="pi pi-star text-orange-500"></i>
          <div className="stat-content">
            <h3>
              {state.suppliers.length > 0
                ? (state.suppliers.reduce((acc, s) => acc + (s.calificacion || 0), 0) / state.suppliers.length).toFixed(1)
                : '0.0'
              }
            </h3>
            <p>Calificación Promedio</p>
          </div>
        </div>
      </div>

      {/* Suppliers Table */}
      <Card className="fade-in">
        <div className="card-header-content">
          <h3><i className="pi pi-list"></i> Lista de Proveedores ({state.suppliers.length})</h3>
        </div>

        <DataTable
          value={state.suppliers}
          paginator
          rows={10}
          rowsPerPageOptions={[5, 10, 20]}
          loading={state.isLoadingSuppliers}
          emptyMessage="No hay proveedores registrados"
          className="p-datatable-striped"
          showGridlines
          responsiveLayout="scroll"
          globalFilterFields={['nombre', 'categoria', 'contacto', 'email']}
        >
          <Column field="nombre" header="Nombre" sortable />
          <Column field="contacto" header="Contacto" sortable />
          <Column field="email" header="Email" />
          <Column 
            field="categoria" 
            header="Categoría" 
            body={categoryBodyTemplate}
            sortable 
          />
          <Column 
            field="calificacion" 
            header="Calificación" 
            body={ratingBodyTemplate}
            sortable 
          />
          <Column 
            field="tiempoEntrega" 
            header="Tiempo Entrega" 
            body={deliveryTimeBodyTemplate}
            sortable 
          />
          <Column 
            field="activo" 
            header="Estado" 
            body={statusBodyTemplate}
            sortable 
          />
          <Column 
            header="Acciones" 
            body={actionsBodyTemplate}
            style={{ width: '14rem' }}
          />
        </DataTable>
      </Card>

      {/* Create Supplier Dialog */}
      <Dialog
        header="Crear Nuevo Proveedor"
        visible={showCreateDialog}
        style={{ width: '700px' }}
        onHide={() => {
          setShowCreateDialog(false);
          resetCreateForm();
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              onClick={() => setShowCreateDialog(false)}
              className="p-button-text"
            />
            <Button
              label="Crear"
              icon="pi pi-check"
              onClick={handleCreateSupplier}
              disabled={!createForm.nombre || !createForm.email}
            />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="nombre">Nombre *</label>
              <InputText
                id="nombre"
                value={createForm.nombre}
                onChange={(e) => setCreateForm({ ...createForm, nombre: e.target.value })}
                placeholder="Nombre del proveedor"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="contacto">Contacto</label>
              <InputText
                id="contacto"
                value={createForm.contacto}
                onChange={(e) => setCreateForm({ ...createForm, contacto: e.target.value })}
                placeholder="Persona de contacto"
              />
            </div>
          </div>
          
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="email">Email *</label>
              <InputText
                id="email"
                type="email"
                value={createForm.email}
                onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                placeholder="email@ejemplo.com"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="telefono">Teléfono</label>
              <InputText
                id="telefono"
                value={createForm.telefono}
                onChange={(e) => setCreateForm({ ...createForm, telefono: e.target.value })}
                placeholder="Número de teléfono"
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="direccion">Dirección</label>
            <InputText
              id="direccion"
              value={createForm.direccion}
              onChange={(e) => setCreateForm({ ...createForm, direccion: e.target.value })}
              placeholder="Dirección completa"
            />
          </div>
          
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="categoria">Categoría</label>
              <Dropdown
                id="categoria"
                value={createForm.categoria}
                options={categoryOptions}
                onChange={(e) => setCreateForm({ ...createForm, categoria: e.value })}
                placeholder="Seleccione categoría"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="tiempoEntrega">Tiempo de Entrega (días)</label>
              <InputNumber
                id="tiempoEntrega"
                value={createForm.tiempoEntrega}
                onValueChange={(e) => setCreateForm({ ...createForm, tiempoEntrega: e.value || 0 })}
                min={0}
                max={365}
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="condicionesPago">Condiciones de Pago</label>
            <InputTextarea
              id="condicionesPago"
              value={createForm.condicionesPago}
              onChange={(e) => setCreateForm({ ...createForm, condicionesPago: e.target.value })}
              rows={3}
              placeholder="Condiciones de pago..."
            />
          </div>
        </div>
      </Dialog>

      {/* Edit Supplier Dialog */}
      <Dialog
        header="Editar Proveedor"
        visible={showEditDialog}
        style={{ width: '700px' }}
        onHide={() => {
          setShowEditDialog(false);
          setSelectedSupplier(null);
          resetEditForm();
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              onClick={() => setShowEditDialog(false)}
              className="p-button-text"
            />
            <Button
              label="Actualizar"
              icon="pi pi-check"
              onClick={handleUpdateSupplier}
              disabled={!editForm.nombre || !editForm.email}
            />
          </div>
        }
      >
        {/* Similar form structure as create, but with editForm */}
        <div className="p-fluid">
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="editNombre">Nombre *</label>
              <InputText
                id="editNombre"
                value={editForm.nombre}
                onChange={(e) => setEditForm({ ...editForm, nombre: e.target.value })}
                placeholder="Nombre del proveedor"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="editContacto">Contacto</label>
              <InputText
                id="editContacto"
                value={editForm.contacto}
                onChange={(e) => setEditForm({ ...editForm, contacto: e.target.value })}
                placeholder="Persona de contacto"
              />
            </div>
          </div>
          
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="editEmail">Email *</label>
              <InputText
                id="editEmail"
                type="email"
                value={editForm.email}
                onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                placeholder="email@ejemplo.com"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="editTelefono">Teléfono</label>
              <InputText
                id="editTelefono"
                value={editForm.telefono}
                onChange={(e) => setEditForm({ ...editForm, telefono: e.target.value })}
                placeholder="Número de teléfono"
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="editDireccion">Dirección</label>
            <InputText
              id="editDireccion"
              value={editForm.direccion}
              onChange={(e) => setEditForm({ ...editForm, direccion: e.target.value })}
              placeholder="Dirección completa"
            />
          </div>
          
          <div className="formgrid grid">
            <div className="field col-12 md:col-6">
              <label htmlFor="editCategoria">Categoría</label>
              <Dropdown
                id="editCategoria"
                value={editForm.categoria}
                options={categoryOptions}
                onChange={(e) => setEditForm({ ...editForm, categoria: e.value })}
                placeholder="Seleccione categoría"
              />
            </div>
            
            <div className="field col-12 md:col-6">
              <label htmlFor="editTiempoEntrega">Tiempo de Entrega (días)</label>
              <InputNumber
                id="editTiempoEntrega"
                value={editForm.tiempoEntrega}
                onValueChange={(e) => setEditForm({ ...editForm, tiempoEntrega: e.value || 0 })}
                min={0}
                max={365}
              />
            </div>
          </div>
          
          <div className="field">
            <label htmlFor="editCondicionesPago">Condiciones de Pago</label>
            <InputTextarea
              id="editCondicionesPago"
              value={editForm.condicionesPago}
              onChange={(e) => setEditForm({ ...editForm, condicionesPago: e.target.value })}
              rows={3}
              placeholder="Condiciones de pago..."
            />
          </div>
        </div>
      </Dialog>

      {/* Evaluation Dialog */}
      <Dialog
        header={`Evaluar Proveedor - ${selectedSupplier?.nombre}`}
        visible={showEvaluationDialog}
        style={{ width: '500px' }}
        onHide={() => {
          setShowEvaluationDialog(false);
          setSelectedSupplier(null);
          resetEvaluationForm();
        }}
        footer={
          <div>
            <Button
              label="Cancelar"
              icon="pi pi-times"
              onClick={() => setShowEvaluationDialog(false)}
              className="p-button-text"
            />
            <Button
              label="Evaluar"
              icon="pi pi-check"
              onClick={handleSubmitEvaluation}
              disabled={evaluationForm.calificacion === 0}
            />
          </div>
        }
      >
        <div className="p-fluid">
          <div className="field">
            <label htmlFor="calificacion">Calificación</label>
            <Rating
              id="calificacion"
              value={evaluationForm.calificacion}
              onChange={(e) => setEvaluationForm({ ...evaluationForm, calificacion: e.value || 0 })}
              cancel={false}
            />
          </div>
          
          <div className="field">
            <label htmlFor="comentarios">Comentarios</label>
            <InputTextarea
              id="comentarios"
              value={evaluationForm.comentarios}
              onChange={(e) => setEvaluationForm({ ...evaluationForm, comentarios: e.target.value })}
              rows={4}
              placeholder="Comentarios sobre el proveedor..."
            />
          </div>
        </div>
      </Dialog>

      {/* Performance Dialog */}
      <Dialog
        header={`Rendimiento del Proveedor - ${selectedSupplier?.nombre}`}
        visible={showPerformanceDialog}
        style={{ width: '600px' }}
        onHide={() => {
          setShowPerformanceDialog(false);
          setSelectedSupplier(null);
          setPerformanceData(null);
        }}
      >
        {isLoadingPerformance ? (
          <div className="text-center p-4">
            <i className="pi pi-spin pi-spinner" style={{ fontSize: '2rem' }}></i>
            <p>Cargando datos de rendimiento...</p>
          </div>
        ) : performanceData ? (
          <div>
            <Panel header="Métricas de Rendimiento" className="mb-3">
              <div className="grid">
                <div className="col-12 md:col-6">
                  <div className="metric-item">
                    <label>Total de Pedidos:</label>
                    <span className="font-bold">{performanceData.totalPedidos}</span>
                  </div>
                  <div className="metric-item">
                    <label>Pedidos a Tiempo:</label>
                    <span className="font-bold text-green-500">{performanceData.pedidosATiempo}</span>
                  </div>
                </div>
                <div className="col-12 md:col-6">
                  <div className="metric-item">
                    <label>Calificación Promedio:</label>
                    <div className="flex align-items-center">
                      <Rating 
                        value={performanceData.calificacionPromedio} 
                        readOnly 
                        cancel={false}
                        className="mr-2"
                      />
                      <span>({performanceData.calificacionPromedio.toFixed(1)})</span>
                    </div>
                  </div>
                  <div className="metric-item">
                    <label>Tiempo Entrega Promedio:</label>
                    <span className="font-bold">{performanceData.tiempoEntregaPromedio} días</span>
                  </div>
                </div>
              </div>
            </Panel>

            <Panel header="Eficiencia de Entrega">
              <div className="mb-2">
                <label>Porcentaje de Entregas a Tiempo:</label>
              </div>
              <ProgressBar
                value={performanceData.totalPedidos > 0
                  ? (performanceData.pedidosATiempo / performanceData.totalPedidos) * 100
                  : 0
                }
                displayValueTemplate={(value) => `${Number(value || 0).toFixed(1)}%`}
              />
              <small className="text-muted">
                Última evaluación: {new Date(performanceData.ultimaEvaluacion).toLocaleDateString()}
              </small>
            </Panel>
          </div>
        ) : (
          <div className="text-center p-4">
            <p>No se pudieron cargar los datos de rendimiento.</p>
          </div>
        )}
      </Dialog>

      {/* Confirmation Dialog */}
      <ConfirmDialog />
    </div>
  );
};

export default SupplierComponent;
