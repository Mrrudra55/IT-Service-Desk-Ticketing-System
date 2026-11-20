import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ShieldCheck, KeyRound, AlertCircle, CheckCircle2 } from 'lucide-react';

export const AdminRegisterPage = () => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [adminSecretKey, setAdminSecretKey] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const { registerAdmin } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (password.length < 6) {
      setError('Password must be at least 6 characters.');
      return;
    }

    if (!adminSecretKey.trim()) {
      setError('Administrator registration security key is required.');
      return;
    }

    setSubmitting(true);

    try {
      await registerAdmin(name, email, password, adminSecretKey.trim());
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Administrator registration failed. Please verify your security key.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="flex items-center justify-center gap-2 mb-2">
        <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold bg-indigo-100 text-indigo-800 border border-indigo-200">
          <ShieldCheck className="w-3.5 h-3.5" />
          Restricted Portal
        </span>
      </div>
      <h3 className="text-lg font-bold text-slate-800 text-center mb-1">Register Administrator Account</h3>
      <p className="text-xs text-slate-500 text-center mb-6">
        Full system control, ticket triage, and management privileges
      </p>

      {error && (
        <div className="mb-4 p-3 bg-rose-50 border border-rose-200 rounded-lg flex items-center gap-2 text-xs text-rose-700">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-3.5">
        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Full Name</label>
          <input
            type="text"
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="IT Admin Name"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
          />
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Work Email</label>
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="admin@company.local"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
          />
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Password</label>
          <input
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Minimum 6 characters"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
          />
        </div>

        <div>
          <label className="block text-xs font-semibold text-slate-700 mb-1">Confirm Password</label>
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            placeholder="Repeat password"
            className="w-full px-3 py-2 text-xs border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
          />
        </div>

        {/* Security Passkey Field */}
        <div className="pt-1">
          <div className="flex items-center justify-between mb-1">
            <label className="block text-xs font-semibold text-indigo-900 flex items-center gap-1.5">
              <KeyRound className="w-3.5 h-3.5 text-indigo-600" />
              Admin Security Key
            </label>
            <span className="text-[10px] text-slate-400">Required</span>
          </div>
          <input
            type="password"
            required
            value={adminSecretKey}
            onChange={(e) => setAdminSecretKey(e.target.value)}
            placeholder="Enter configuration secret key"
            className="w-full px-3 py-2 text-xs border-2 border-indigo-200 bg-indigo-50/30 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono transition"
          />
          <p className="text-[10px] text-slate-400 mt-1">
            Default system secret key is configured in backend <code className="font-mono text-indigo-700">appsettings.json</code>.
          </p>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="w-full mt-2 py-2.5 px-4 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold rounded-lg shadow-md hover:shadow-lg disabled:opacity-50 transition flex items-center justify-center gap-2"
        >
          <ShieldCheck className="w-4 h-4" />
          {submitting ? 'Verifying & Registering...' : 'Register as Administrator'}
        </button>
      </form>

      <div className="mt-5 pt-4 border-t border-slate-100 text-center space-y-1.5">
        <p className="text-xs text-slate-500">
          Standard staff member?{' '}
          <Link to="/register" className="font-semibold text-blue-600 hover:underline">
            Register as Employee
          </Link>
        </p>
        <p className="text-xs text-slate-500">
          Already registered?{' '}
          <Link to="/login" className="font-semibold text-slate-700 hover:underline">
            Sign In
          </Link>
        </p>
      </div>
    </div>
  );
};

