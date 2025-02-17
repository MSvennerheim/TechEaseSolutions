import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { useLogin } from '../Components/LoginLogic.jsx';


//styling  och layout för login sidan.
function LoginUI() {
  const { email, setEmail, password, setPassword, error, handleSubmit } = useLogin();

  return (
   <div
      className="d-flex justify-content-center align-items-center bg-primary"
      style={{ minHeight: '100vh', width: '100vw' }}>
      <div className='bg-white p-4 rounded shadow-sm' style={{ width: '400px' }}>
        <h2 className='text-center mb-4'>Login</h2>

        {error && <div className='alert alert-danger' role='alert'>{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className='mb-3'>
            <label htmlFor='email' className='form-label'>Email</label>
            <input
              type='email'
              className='form-control'
              id='email'
              placeholder='Enter email'
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div className='mb-4'>
            <label htmlFor='password' className='form-label'>Password</label>
            <input
              type='password'
              className='form-control'
              id='password'
              placeholder='Enter password'
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <div className='d-grid'>
            <button type='submit' className='btn btn-primary btn-lg'>Login</button>
          </div>

        

          
          
        </form>
      </div>
    </div>
  );
}

export default LoginUI;
