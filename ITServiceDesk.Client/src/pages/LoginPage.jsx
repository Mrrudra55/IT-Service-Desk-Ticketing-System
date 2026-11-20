import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LogIn, AlertCircle, Shield } from 'lucide-react';

export const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Login failed. Please verify credentials.');
    } finally {
      setSubmitting(false);
    }
  };

  // Helper for 1-click test fill
  const handleQuickFill = (demoEmail, demoPassword) => {
    setEmail(demoEmail);
    setPassword(demoPassword);
    setError('');
  };

  return (
    <div>
      <h3 className="text-lg font-bold text-slate-800 text-center mb-1">Sign In to Your Account</h3>
      <p className="text-xs text-slate-500 text-center mb-6">Enter your organizational credentials below</p>

      {error && (
        <div className="mb-4 p-3 bg-rose-50 border border-rose-200 rounded-lg flex items-center gap-2 text-xs text-rose-700">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Email Address</label>
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@servicedesk.local"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
          />
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Password</label>
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
          />
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="w-full py-2.5 px-4 bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold rounded-lg shadow-md hover:shadow-lg disabled:opacity-50 transition flex items-center justify-center gap-2"
        >
          <LogIn className="w-4 h-4" />
          {submitting ? 'Authenticating...' : 'Sign In'}
        </button>
      </form>

      {/* Quick Demo Credentials Switcher (Only Admin and Employee) */}
      <div className="mt-6 pt-5 border-t border-slate-100">
        <span className="text-[10px] uppercase font-bold text-slate-400 block text-center mb-2">
          Quick Demo Accounts (1-Click Fill)
        </span>
        <div className="grid grid-cols-2 gap-2">
          <button
            type="button"
            onClick={() => handleQuickFill('admin@servicedesk.local', 'Admin@123!')}
            className="px-3 py-2 bg-indigo-50 hover:bg-indigo-100 border border-indigo-200 text-indigo-800 rounded-lg text-xs font-semibold transition flex items-center justify-center gap-1.5"
          >
            <Shield className="w-3.5 h-3.5" />
            Admin Account
          </button>
          <button
            type="button"
            onClick={() => handleQuickFill('employee@servicedesk.local', 'Employee@123!')}
            className="px-3 py-2 bg-slate-100 hover:bg-slate-200 border border-slate-200 text-slate-800 rounded-lg text-xs font-semibold transition"
          >
            Employee Account
          </button>
        </div>
      </div>

      <div className="mt-5 text-center space-y-1.5">
        <p className="text-xs text-slate-500">
          Need an account?{' '}
          <Link to="/register" className="font-semibold text-blue-600 hover:underline">
            Register as Employee
          </Link>
        </p>
        <p className="text-xs text-slate-500">
          IT Administrator?{' '}
          <Link to="/register-admin" className="font-semibold text-indigo-600 hover:underline">
            Register as Admin (Key Required)
          </Link>
        </p>
      </div>
    </div>
  );
};
