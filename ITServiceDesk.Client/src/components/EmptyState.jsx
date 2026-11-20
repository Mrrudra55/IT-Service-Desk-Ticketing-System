import React from 'react';
import { Inbox } from 'lucide-react';

export const EmptyState = ({ title = 'No items found', message = 'There are no records matching your current filter criteria.', action }) => {
  return (
    <div className="flex flex-col items-center justify-center p-12 text-center bg-white rounded-xl border border-dashed border-slate-300">
      <div className="w-12 h-12 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 mb-3">
        <Inbox className="w-6 h-6" />
      </div>
      <h3 className="text-base font-semibold text-slate-800">{title}</h3>
      <p className="text-sm text-slate-500 max-w-sm mt-1 mb-4">{message}</p>
      {action}
    </div>
  );
};

