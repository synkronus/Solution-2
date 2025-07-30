import React, { useState } from 'react';
import { Card } from 'primereact/card';
import { InputText } from 'primereact/inputtext';
import { Password } from 'primereact/password';
import { Button } from 'primereact/button';
import { Message } from 'primereact/message';
import { ProgressSpinner } from 'primereact/progressspinner';
import { useAuth } from '../context/AppContext';
import { apiService } from '../services/apiService';
import './EnhancedFormStyles.css';

interface LoginComponentProps {
  onLoginSuccess?: () => void;
}

const LoginComponent: React.FC<LoginComponentProps> = ({ onLoginSuccess }) => {
  const { login } = useAuth();
  const [formData, setFormData] = useState({
    username: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
    // Clear error when user starts typing
    if (error) {
      setError(null);
    }
  };

  const handleLogin = async () => {
    if (!formData.username.trim() || !formData.password.trim()) {
      setError('Por favor ingrese usuario y contraseña');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const result = await apiService.login(formData.username, formData.password);
      
      // Store token for future API calls
      localStorage.setItem('authToken', result.token);
      
      // Update user context
      const userData = {
        id: result.usuario.vendedorId || result.usuario.id,
        username: result.usuario.username,
        email: result.usuario.email,
        role: getRoleString(result.usuario.rol),
        nombre: result.usuario.nombre,
        apellido: result.usuario.apellido,
        vendedorId: result.usuario.vendedorId,
        empleadoRHId: result.usuario.empleadoRHId,
        token: result.token,
        expiresAt: result.expiresAt
      };

      // Restrict access to sales representatives only
      if (userData.role !== 'sales_rep') {
        setError('Esta aplicación está destinada exclusivamente para representantes de ventas. Solo usuarios con rol "sales_rep" pueden acceder.');
        localStorage.removeItem('authToken');
        return;
      }

      login(userData);
      
      if (onLoginSuccess) {
        onLoginSuccess();
      }
    } catch (error: any) {
      console.error('Login error:', error);
      setError(
        error.response?.data?.message || 
        'Error de autenticación. Verifique sus credenciales.'
      );
    } finally {
      setLoading(false);
    }
  };

  const getRoleString = (role: number): string => {
    switch (role) {
      case 1: return 'admin';
      case 2: return 'hr_manager';
      case 3: return 'sales_rep';
      case 4: return 'inventory_manager';
      case 5: return 'delivery_manager';
      default: return 'user';
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleLogin();
    }
  };

  return (
    <div className="login-container" style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    }}>
      <Card 
        className="login-card" 
        style={{ 
          width: '100%', 
          maxWidth: '400px', 
          padding: '2rem',
          boxShadow: '0 10px 30px rgba(0,0,0,0.3)'
        }}
      >
        <div className="text-center mb-4">
          <i className="pi pi-user text-primary" style={{ fontSize: '3rem' }}></i>
          <h2 style={{ margin: '1rem 0', color: '#333' }}>Representantes de Ventas</h2>
        </div>

        {error && (
          <Message 
            severity="error" 
            text={error} 
            className="w-full mb-3"
          />
        )}

        <div className="p-field mb-3">
          <label htmlFor="username" className="block mb-2 font-medium">
            Usuario
          </label>
          <InputText
            id="username"
            value={formData.username}
            onChange={(e) => handleInputChange('username', e.target.value)}
            placeholder="Ingrese su usuario"
            className="w-full"
            disabled={loading}
            onKeyPress={handleKeyPress}
          />
        </div>

        <div className="p-field mb-4">
          <label htmlFor="password" className="block mb-2 font-medium">
            Contraseña
          </label>
          <Password
            id="password"
            value={formData.password}
            onChange={(e) => handleInputChange('password', e.target.value)}
            placeholder="Ingrese su contraseña"
            className="w-full"
            disabled={loading}
            feedback={false}
            toggleMask
            onKeyPress={handleKeyPress}
          />
        </div>

        <Button
          label={loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
          icon={loading ? 'pi pi-spin pi-spinner' : 'pi pi-sign-in'}
          onClick={handleLogin}
          className="w-full p-button-lg"
          disabled={loading || !formData.username.trim() || !formData.password.trim()}
        />

        <div className="text-center mt-4">
          <small className="text-secondary">
            Contacte al administrador si tiene problemas de acceso
          </small>
        </div>
      </Card>
    </div>
  );
};

export default LoginComponent;
