import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { 
  LayoutDashboard, 
  Ticket, 
  PlusCircle, 
  Users, 
  ShieldCheck
} from 'lucide-react';

export const Sidebar = () => {
  const { user } = useAuth();

  const navItems = [
    {
      name: 'Dashboard',
      path: '/dashboard',
      icon: LayoutDashboard,
      roles: ['Employee', 'Admin'],
    },
    {
      name: 'Support Tickets',
      path: '/tickets',
      icon: Ticket,
      roles: ['Employee', 'Admin'],
    },
    {
      name: 'Create Ticket',
      path: '/tickets/new',
      icon: PlusCircle,
      roles: ['Employee', 'Admin'],
    },
    {
      name: 'User Management',
      path: '/users',
      icon: Users,
      roles: ['Admin'],
    },
  ];

  const allowedNav = navItems.filter((item) => item.roles.includes(user?.role));

  return (
    <aside className="w-64 bg-slate-900 text-slate-300 flex flex-col shrink-0 min-h-screen">
      {/* Brand logo & title */}
      <div className="h-16 flex items-center px-6 gap-3 border-b border-slate-800">
        <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center text-white font-bold shadow-md shadow-blue-500/20">
          IT
        </div>
        <div>
          <span className="font-bold text-sm text-white tracking-wide block">SERVICE DESK</span>
          <span className="text-[10px] text-slate-500 block uppercase font-medium tracking-wider">Enterprise Helpdesk</span>
        </div>
      </div>

      {/* Navigation Links */}
      <nav className="flex-1 p-4 space-y-1.5">
        <div className="text-[10px] uppercase font-bold text-slate-500 px-3 py-1.5 tracking-wider">
          Main Navigation
        </div>
        {allowedNav.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/tickets'}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-xs font-semibold transition ${
                  isActive
                    ? 'bg-blue-600 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-100 hover:bg-slate-800/60'
                }`
              }
            >
              <Icon className="w-4 h-4" />
              {item.name}
            </NavLink>
          );
        })}
      </nav>

      {/* Role Pill Card at Bottom */}
      <div className="p-4 border-t border-slate-800">
        <div className="bg-slate-800/80 rounded-xl p-3 border border-slate-700/50 flex items-center gap-2.5">
          <ShieldCheck className="w-4 h-4 text-blue-400 shrink-0" />
          <div className="overflow-hidden">
            <span className="text-[11px] font-semibold text-slate-200 block truncate">
              {user?.role} Portal
            </span>
            <span className="text-[10px] text-slate-400 block truncate">
              {user?.email}
            </span>
          </div>
        </div>
      </div>
    </aside>
  );
};
