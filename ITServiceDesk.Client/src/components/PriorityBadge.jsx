import React from 'react';

const priorityConfig = {
  Low: 'bg-slate-100 text-slate-700 border-slate-300',
  Medium: 'bg-sky-50 text-sky-700 border-sky-200',
  High: 'bg-orange-50 text-orange-700 border-orange-200',
  Critical: 'bg-rose-50 text-rose-700 border-rose-300 animate-pulse',
};

export const PriorityBadge = ({ priority }) => {
  const style = priorityConfig[priority] || priorityConfig.Medium;

  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border ${style}`}
    >
      {priority}
    </span>
  );
};

