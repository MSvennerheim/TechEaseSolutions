import React from 'react';
import { useLogin } from '../Components/LoginLogic.jsx';
import '../styles/Login.css';

//styling  och layout för login sidan.
function LoginUI() {
  const { email, setEmail, password, setPassword, error, handleSubmit } = useLogin();

  return (
    <div className="login-container">
      <div className="login-card">

        <div className="login-header">
          <h1>Logga in</h1>
        </div>

        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className="input-group">
            <label htmlFor="password">Lösenord</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <button type="submit">
            Logga in
          </button>
        </form>
      </div>
    </div>
  );
}

export default LoginUI;
