import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';


function PasswordResetForm() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    email: '',
    token: '',
    newPassword: '',
    confirmPassword: '',
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [passwordStrength, setPasswordStrength] = useState('');

  useEffect(() => {
    // få token och email från URL-parametrar
    const email = searchParams.get('email');
    const token = searchParams.get('token');
    
    if (email && token) {
      setFormData(prev => ({
        ...prev,
        email,
        token
      }));
    } else {
      setError('Invalid password reset link. Please request a new one.');
    }
  }, [searchParams]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));

    // kolla lösenordets styrka när det ändras
    if (name === 'newPassword') {
      checkPasswordStrength(value);
    }
  };

  const checkPasswordStrength = (password) => {
    if (password.length === 0) {
      setPasswordStrength('');
      return;
    }
    
    // enkel lösenordsstyrka-kontrollering
    if (password.length < 6) {
      setPasswordStrength('weak');
    } else if (password.length < 10 || !/[A-Z]/.test(password) || !/[0-9]/.test(password)) {
      setPasswordStrength('medium');
    } else {
      setPasswordStrength('strong');
    }
  };

  const validatePassword = () => {
    if (formData.newPassword.length < 6) {
      setError('Password must be at least 6 characters long');
      return false;
    }
    
    if (formData.newPassword !== formData.confirmPassword) {
      setError('Passwords do not match');
      return false;
    }
    
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');

    // validera lösenordet innan vi skickar den till servern
    if (!validatePassword()) {
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('/api/reset-password', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: formData.email,
          token: formData.token,
          newPassword: formData.newPassword
        }),
      });

      const data = await response.json();

      if (response.ok) {
        setMessage('Password set successfully! Redirecting to login...');
        // redierctar användaren till loginsidan efter 3 sekunder
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        setError(data.message || 'Failed to reset password. Please try again.');
      }
    } catch (err) {
      setError('An error occurred. Please try again later.');
      console.error('Error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="password-reset-container">
      <div className="form-header">
        <h1 className="form-title">Reset Your Password</h1>
        <p className="form-subtitle">Enter a new password for your account</p>
      </div>

      {error && <div className="error-message">{error}</div>}
      {message && <div className="success-message">{message}</div>}

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label className="form-label" htmlFor="newPassword">New Password</label>
          <input
            className="form-input"
            type="password"
            id="newPassword"
            name="newPassword"
            value={formData.newPassword}
            onChange={handleChange}
            placeholder="Enter new password"
            disabled={loading}
          />
          {passwordStrength && (
            <div className="password-strength">
              <div className={`strength-${passwordStrength}`}></div>
            </div>
          )}
        </div>

        <div className="form-group">
          <label className="form-label" htmlFor="confirmPassword">Confirm Password</label>
          <input
            className="form-input"
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="Confirm your password"
            disabled={loading}
          />
        </div>

        <button 
          className="submit-button" 
          type="submit" 
          disabled={loading}
        >
          {loading ? (
            <>
              <span className="loading-spinner"></span>
              Resetting...
            </>
          ) : 'Reset Password'}
        </button>
      </form>
    </div>
  );
}

export default PasswordResetForm;