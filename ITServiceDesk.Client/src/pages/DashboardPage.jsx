import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../api/axios';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';
import { Link } from 'react-router-dom';
import { 
  Ticket, 
  Clock, 
  CheckCircle2, 
  AlertTriangle, 
  Users, 
  PlusCircle, 
  ArrowRight,
  TrendingUp,
  Inbox,
  Download,
  ShieldCheck
} from 'lucide-react';

export const DashboardPage = () => {
  const { user, isEmployee, isAdmin } = useAuth();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  const handleExportCsv = async () => {
    try {
      const res = await api.get('/reports/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `IT_Service_Desk_Report_${new Date().toISOString().slice(0, 10)}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch {
      alert('Failed to export CSV report.');
    }
  };

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const endpoint = isAdmin ? '/dashboard/admin' : '/dashboard/employee';
        const res = await api.get(endpoint);
        setData(res.data);
      } catch (err) {
        console.error('Failed to load dashboard metrics', err);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [user, isAdmin]);

  if (loading) {
    return <LoadingSpinner text="Compiling dashboard analytics..." />;
  }

  // ================= ADMINISTRATOR DASHBOARD =================
  if (isAdmin) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <div className="flex items-center gap-2">
              <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-indigo-100 text-indigo-800">
                <ShieldCheck className="w-3 h-3" /> Administrator Portal
              </span>
            </div>
            <h2 className="text-xl font-bold text-slate-900 mt-1">Enterprise IT Command Center</h2>
            <p className="text-xs text-slate-500 mt-0.5">Real-time infrastructure health, support operations, and ticket triage</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleExportCsv}
              className="inline-flex items-center gap-1.5 px-3 py-2 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 text-xs font-semibold rounded-lg shadow-sm transition"
            >
              <Download className="w-3.5 h-3.5 text-slate-500" /> Export CSV
            </button>
            <Link
              to="/tickets/new"
              className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold rounded-lg shadow-sm transition"
            >
              <PlusCircle className="w-4 h-4" /> New Ticket
            </Link>
          </div>
        </div>

        {/* Top Metric Cards */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <span className="text-xs text-slate-500 font-medium">Total Registered Users</span>
            <div className="flex items-center justify-between mt-2">
              <span className="text-2xl font-bold text-slate-800">{data?.totalUsers || 0}</span>
              <Users className="w-5 h-5 text-blue-500" />
            </div>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <span className="text-xs text-slate-500 font-medium">Active Tickets</span>
            <div className="flex items-center justify-between mt-2">
              <span className="text-2xl font-bold text-blue-600">{(data?.openTickets || 0) + (data?.inProgressTickets || 0)}</span>
              <Ticket className="w-5 h-5 text-blue-500" />
            </div>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <span className="text-xs text-slate-500 font-medium">Critical Tickets</span>
            <div className="flex items-center justify-between mt-2">
              <span className="text-2xl font-bold text-rose-600">{data?.criticalTickets || 0}</span>
              <AlertTriangle className="w-5 h-5 text-rose-500" />
            </div>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <span className="text-xs text-slate-500 font-medium">Avg Resolution Time</span>
            <div className="flex items-center justify-between mt-2">
              <span className="text-2xl font-bold text-emerald-600">{data?.averageResolutionHours || 0} hrs</span>
              <Clock className="w-5 h-5 text-emerald-500" />
            </div>
          </div>
        </div>

        {/* Breakdown Charts & Administrator Workload */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Category Distribution */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <h3 className="text-sm font-bold text-slate-800 mb-3 flex items-center gap-2">
              <TrendingUp className="w-4 h-4 text-blue-600" /> Tickets by Category
            </h3>
            <div className="space-y-3">
              {data?.ticketsByCategory?.map((item) => (
                <div key={item.label}>
                  <div className="flex justify-between text-xs font-semibold text-slate-700 mb-1">
                    <span>{item.label}</span>
                    <span>{item.count} tickets</span>
                  </div>
                  <div className="w-full bg-slate-100 rounded-full h-2 overflow-hidden">
                    <div
                      className="bg-blue-600 h-2 rounded-full transition-all duration-500"
                      style={{
                        width: `${data?.totalTickets > 0 ? (item.count / data.totalTickets) * 100 : 0}%`,
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Administrator Team Workload Table */}
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <h3 className="text-sm font-bold text-slate-800 mb-3 flex items-center gap-2">
              <Users className="w-4 h-4 text-indigo-600" /> Admin Support Specialists
            </h3>
            <div className="divide-y divide-slate-100">
              {data?.agentWorkload?.map((agent) => (
                <div key={agent.agentId} className="py-2.5 flex items-center justify-between text-xs">
                  <div>
                    <span className="font-semibold text-slate-800 block">{agent.agentName}</span>
                    <span className="text-slate-400 text-[11px]">{agent.agentEmail}</span>
                  </div>
                  <div className="text-right">
                    <span className="font-bold text-blue-600 block">{agent.assignedTicketsCount} active</span>
                    <span className="text-slate-400 text-[10px]">{agent.resolvedTicketsCount} resolved</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Recent Tickets Table */}
        <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
          <div className="p-4 border-b border-slate-200 flex justify-between items-center bg-slate-50/50">
            <div>
              <h3 className="text-sm font-bold text-slate-800">Recent System Tickets</h3>
              <p className="text-xs text-slate-500">Live operational feed of user inquiries</p>
            </div>
            <Link to="/tickets" className="text-xs text-blue-600 font-semibold hover:underline flex items-center gap-1">
              View All Tickets <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>
          <div className="divide-y divide-slate-100">
            {data?.recentTickets?.length === 0 ? (
              <div className="p-8 text-center text-xs text-slate-400">No tickets found in the system.</div>
            ) : (
              data?.recentTickets?.map((t) => (
                <div key={t.id} className="p-4 flex items-center justify-between hover:bg-slate-50 transition">
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-mono font-bold text-blue-600">{t.ticketNumber}</span>
                      <PriorityBadge priority={t.priority} />
                      <StatusBadge status={t.status} />
                    </div>
                    <Link to={`/tickets/${t.id}`} className="text-sm font-semibold text-slate-800 hover:text-blue-600 block">
                      {t.title}
                    </Link>
                    <p className="text-xs text-slate-400">
                      Reported by {t.createdByUserName} • Assigned to: <span className="font-medium text-slate-600">{t.assignedToUserName || 'Unassigned'}</span>
                    </p>
                  </div>
                  <Link
                    to={`/tickets/${t.id}`}
                    className="p-2 text-slate-400 hover:text-blue-600 rounded-lg hover:bg-slate-100 transition"
                  >
                    <ArrowRight className="w-4 h-4" />
                  </Link>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    );
  }

  // ================= EMPLOYEE DASHBOARD =================
  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900">Welcome, {user?.name}!</h2>
          <p className="text-xs text-slate-500 mt-0.5">Track your workplace technical support requests and submit new inquiries</p>
        </div>
        <Link
          to="/tickets/new"
          className="inline-flex items-center gap-2 px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold rounded-lg shadow-sm hover:shadow transition self-start sm:self-auto"
        >
          <PlusCircle className="w-4 h-4" /> Raise New Ticket
        </Link>
      </div>

      {/* Employee Quick Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
          <span className="text-xs text-slate-500 font-medium">Total Tickets</span>
          <div className="flex items-center justify-between mt-2">
            <span className="text-2xl font-bold text-slate-800">{data?.totalTickets || 0}</span>
            <Ticket className="w-5 h-5 text-blue-500" />
          </div>
        </div>
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
          <span className="text-xs text-slate-500 font-medium">Pending Review</span>
          <div className="flex items-center justify-between mt-2">
            <span className="text-2xl font-bold text-amber-600">{data?.openTickets || 0}</span>
            <Clock className="w-5 h-5 text-amber-500" />
          </div>
        </div>
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
          <span className="text-xs text-slate-500 font-medium">In Progress</span>
          <div className="flex items-center justify-between mt-2">
            <span className="text-2xl font-bold text-blue-600">{data?.inProgressTickets || 0}</span>
            <Inbox className="w-5 h-5 text-blue-500" />
          </div>
        </div>
        <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
          <span className="text-xs text-slate-500 font-medium">Resolved</span>
          <div className="flex items-center justify-between mt-2">
            <span className="text-2xl font-bold text-emerald-600">{data?.resolvedTickets || 0}</span>
            <CheckCircle2 className="w-5 h-5 text-emerald-500" />
          </div>
        </div>
      </div>

      {/* Employee Recent Tickets */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="p-4 border-b border-slate-200 flex justify-between items-center bg-slate-50/50">
          <div>
            <h3 className="text-sm font-bold text-slate-800">Your Recent Tickets</h3>
            <p className="text-xs text-slate-500">Track status updates and communication with IT administrators</p>
          </div>
          <Link to="/tickets" className="text-xs text-blue-600 font-semibold hover:underline flex items-center gap-1">
            View All <ArrowRight className="w-3.5 h-3.5" />
          </Link>
        </div>

        <div className="divide-y divide-slate-100">
          {data?.recentTickets?.length === 0 ? (
            <div className="p-8 text-center text-xs text-slate-400">
              You haven't submitted any IT tickets yet. Click "Raise New Ticket" above to submit one.
            </div>
          ) : (
            data?.recentTickets?.map((t) => (
              <div key={t.id} className="p-4 flex items-center justify-between hover:bg-slate-50 transition">
                <div className="space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="text-xs font-mono font-bold text-blue-600">{t.ticketNumber}</span>
                    <PriorityBadge priority={t.priority} />
                    <StatusBadge status={t.status} />
                    <span className="text-xs text-slate-400">• {t.categoryName}</span>
                  </div>
                  <Link to={`/tickets/${t.id}`} className="text-sm font-semibold text-slate-800 hover:text-blue-600 block">
                    {t.title}
                  </Link>
                  <p className="text-xs text-slate-400">
                    Assigned Admin: <span className="font-medium text-slate-600">{t.assignedToUserName || 'Awaiting assignment'}</span>
                  </p>
                </div>
                <Link
                  to={`/tickets/${t.id}`}
                  className="p-2 text-slate-400 hover:text-blue-600 rounded-lg hover:bg-slate-100 transition"
                >
                  <ArrowRight className="w-4 h-4" />
                </Link>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};
