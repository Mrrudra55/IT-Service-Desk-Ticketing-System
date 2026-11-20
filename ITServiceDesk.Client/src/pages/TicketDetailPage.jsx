import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { StatusBadge } from '../components/StatusBadge';
import { PriorityBadge } from '../components/PriorityBadge';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { 
  ArrowLeft, 
  Send, 
  Clock, 
  User, 
  ShieldAlert, 
  History, 
  MessageSquare, 
  UserCheck,
  CheckCircle2,
  AlertTriangle,
  Paperclip,
  Download,
  Trash2,
  FileText,
  Upload
} from 'lucide-react';

export const TicketDetailPage = () => {
  const { id } = useParams();
  const { user, isAdmin } = useAuth();

  const [ticket, setTicket] = useState(null);
  const [admins, setAdmins] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('discussion'); // 'discussion', 'attachments', or 'timeline'

  // Comment submission state
  const [commentText, setCommentText] = useState('');
  const [submittingComment, setSubmittingComment] = useState(false);
  const [commentError, setCommentError] = useState('');

  // Status / Assignment update state
  const [updating, setUpdating] = useState(false);

  // Attachment upload state
  const [uploadingFile, setUploadingFile] = useState(false);
  const [uploadError, setUploadError] = useState('');

  const handleFileUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      setUploadError('File exceeds 10 MB maximum limit.');
      return;
    }

    setUploadingFile(true);
    setUploadError('');

    const formData = new FormData();
    formData.append('file', file);

    try {
      await api.post(`/tickets/${id}/attachments`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      await fetchTicket();
      e.target.value = ''; // Reset input
    } catch (err) {
      setUploadError(err.response?.data?.message || 'File upload failed.');
    } finally {
      setUploadingFile(false);
    }
  };

  const handleDownloadAttachment = async (attachmentId, fileName) => {
    try {
      const res = await api.get(`/attachments/${attachmentId}/download`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch {
      alert('Failed to download attachment.');
    }
  };

  const handleDeleteAttachment = async (attachmentId) => {
    if (!window.confirm('Are you sure you want to delete this attachment?')) return;

    try {
      await api.delete(`/attachments/${attachmentId}`);
      await fetchTicket();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to delete attachment.');
    }
  };

  const fetchTicket = async () => {
    try {
      const res = await api.get(`/tickets/${id}`);
      setTicket(res.data);
    } catch (err) {
      console.error('Failed to load ticket', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTicket();

    // If user is Admin, load assignable admins
    if (isAdmin) {
      api.get('/users/admins').then(res => setAdmins(res.data)).catch(console.error);
    }
  }, [id, user, isAdmin]);

  const handleStatusChange = async (newStatus) => {
    setUpdating(true);
    try {
      await api.patch(`/tickets/${id}/status`, { status: newStatus });
      await fetchTicket();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to update ticket status.');
    } finally {
      setUpdating(false);
    }
  };

  const handleAssignAdmin = async (adminUserId) => {
    setUpdating(true);
    try {
      const parsedId = adminUserId ? parseInt(adminUserId) : null;
      await api.patch(`/tickets/${id}/assign`, { assignedToUserId: parsedId });
      await fetchTicket();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to assign administrator.');
    } finally {
      setUpdating(false);
    }
  };

  const handleCommentSubmit = async (e) => {
    e.preventDefault();
    if (!commentText.trim()) return;

    setSubmittingComment(true);
    setCommentError('');

    try {
      await api.post(`/tickets/${id}/comments`, { content: commentText.trim() });
      setCommentText('');
      await fetchTicket();
    } catch (err) {
      setCommentError(err.response?.data?.message || 'Failed to post comment.');
    } finally {
      setSubmittingComment(false);
    }
  };

  if (loading) {
    return <LoadingSpinner text="Retrieving ticket details & audit history..." />;
  }

  if (!ticket) {
    return (
      <div className="p-8 text-center">
        <h3 className="text-base font-semibold text-slate-800">Ticket Not Found</h3>
        <Link to="/tickets" className="text-xs text-blue-600 underline mt-2 block">Return to tickets</Link>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Top Navigation */}
      <div className="flex items-center gap-3">
        <Link
          to="/tickets"
          className="p-1.5 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition"
        >
          <ArrowLeft className="w-5 h-5" />
        </Link>
        <div>
          <div className="flex items-center gap-2">
            <span className="font-mono text-sm font-extrabold text-blue-600">{ticket.ticketNumber}</span>
            <span className="text-xs text-slate-400">• {ticket.categoryName}</span>
          </div>
          <h2 className="text-lg font-bold text-slate-900 leading-snug">{ticket.title}</h2>
        </div>
      </div>

      {/* Main Grid: Details Column & Controls Column */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column: Problem Details & Tabbed Thread (Discussion / Attachments / Timeline) */}
        <div className="lg:col-span-2 space-y-6">
          {/* Problem Description Card */}
          <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-3">
            <div className="flex items-center justify-between pb-3 border-b border-slate-100">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <User className="w-4 h-4 text-slate-400" />
                <span>Submitted by <strong className="text-slate-700">{ticket.createdByUserName}</strong></span>
              </div>
              <span className="text-xs text-slate-400">{new Date(ticket.createdAt).toLocaleString()}</span>
            </div>
            <div className="text-xs text-slate-700 leading-relaxed whitespace-pre-wrap font-normal">
              {ticket.description}
            </div>
          </div>

          {/* Tabs: Discussion vs Attachments vs Audit Timeline */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="flex border-b border-slate-200 bg-slate-50/70">
              <button
                onClick={() => setActiveTab('discussion')}
                className={`flex-1 py-3 px-4 text-xs font-bold border-b-2 transition flex items-center justify-center gap-2 ${
                  activeTab === 'discussion'
                    ? 'border-blue-600 text-blue-600 bg-white'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                <MessageSquare className="w-4 h-4" /> Discussion ({ticket.comments?.length || 0})
              </button>
              <button
                onClick={() => setActiveTab('attachments')}
                className={`flex-1 py-3 px-4 text-xs font-bold border-b-2 transition flex items-center justify-center gap-2 ${
                  activeTab === 'attachments'
                    ? 'border-blue-600 text-blue-600 bg-white'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                <Paperclip className="w-4 h-4" /> Attachments ({ticket.attachments?.length || 0})
              </button>
              <button
                onClick={() => setActiveTab('timeline')}
                className={`flex-1 py-3 px-4 text-xs font-bold border-b-2 transition flex items-center justify-center gap-2 ${
                  activeTab === 'timeline'
                    ? 'border-blue-600 text-blue-600 bg-white'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                <History className="w-4 h-4" /> History ({ticket.histories?.length || 0})
              </button>
            </div>

            {/* Tab 1: Discussion Thread */}
            {activeTab === 'discussion' && (
              <div className="p-6 space-y-6">
                <div className="space-y-4">
                  {ticket.comments?.length === 0 ? (
                    <div className="text-center py-8 text-xs text-slate-400">
                      No comments yet. Start the conversation below.
                    </div>
                  ) : (
                    ticket.comments?.map((comment) => {
                      const isSenderAdmin = comment.authorRole === 'Admin';
                      return (
                        <div
                          key={comment.id}
                          className={`p-4 rounded-xl border text-xs space-y-1.5 ${
                            isSenderAdmin
                              ? 'bg-indigo-50/50 border-indigo-100 ml-4'
                              : 'bg-slate-50 border-slate-200 mr-4'
                          }`}
                        >
                          <div className="flex items-center justify-between text-[11px]">
                            <span className="font-bold text-slate-800 flex items-center gap-1.5">
                              {comment.authorName}
                              <span
                                className={`text-[10px] px-1.5 py-0.5 rounded font-semibold ${
                                  isSenderAdmin ? 'bg-indigo-100 text-indigo-700' : 'bg-slate-200 text-slate-600'
                                }`}
                              >
                                {comment.authorRole}
                              </span>
                            </span>
                            <span className="text-slate-400">{new Date(comment.createdAt).toLocaleString()}</span>
                          </div>
                          <p className="text-slate-700 leading-relaxed whitespace-pre-wrap">{comment.content}</p>
                        </div>
                      );
                    })
                  )}
                </div>

                {/* Comment Box */}
                {commentError && (
                  <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-xs text-rose-700">
                    {commentError}
                  </div>
                )}
                <form onSubmit={handleCommentSubmit} className="space-y-2">
                  <textarea
                    rows={3}
                    value={commentText}
                    onChange={(e) => setCommentText(e.target.value)}
                    placeholder="Add an update or response to this ticket..."
                    className="w-full p-3 text-xs border border-slate-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <div className="flex justify-end">
                    <button
                      type="submit"
                      disabled={submittingComment || !commentText.trim()}
                      className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold rounded-lg shadow-sm disabled:opacity-50 transition flex items-center gap-1.5"
                    >
                      <Send className="w-3.5 h-3.5" />
                      {submittingComment ? 'Posting...' : 'Post Message'}
                    </button>
                  </div>
                </form>
              </div>
            )}

            {/* Tab 2: Attachments */}
            {activeTab === 'attachments' && (
              <div className="p-6 space-y-6">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 mb-2">Upload New File or Screenshot</label>
                  <div className="flex items-center gap-3">
                    <label className="cursor-pointer inline-flex items-center gap-2 px-4 py-2 bg-blue-50 hover:bg-blue-100 text-blue-700 border border-blue-200 rounded-lg text-xs font-semibold transition">
                      <Upload className="w-3.5 h-3.5" />
                      <span>{uploadingFile ? 'Uploading...' : 'Choose File'}</span>
                      <input
                        type="file"
                        onChange={handleFileUpload}
                        disabled={uploadingFile}
                        className="hidden"
                      />
                    </label>
                    <span className="text-[11px] text-slate-400">PDF, PNG, JPG, DOCX (Max 10 MB)</span>
                  </div>
                  {uploadError && <p className="text-xs text-rose-600 mt-2">{uploadError}</p>}
                </div>

                <div className="space-y-2">
                  <span className="text-xs font-bold text-slate-700 uppercase tracking-wider block">Attached Files</span>
                  {ticket.attachments?.length === 0 ? (
                    <div className="text-center py-8 text-xs text-slate-400 border border-dashed rounded-xl">
                      No files attached to this ticket.
                    </div>
                  ) : (
                    ticket.attachments?.map((file) => (
                      <div key={file.id} className="p-3 bg-slate-50 border border-slate-200 rounded-lg flex items-center justify-between text-xs">
                        <div className="flex items-center gap-2.5 overflow-hidden">
                          <FileText className="w-4 h-4 text-blue-600 shrink-0" />
                          <div className="truncate">
                            <span className="font-semibold text-slate-800 block truncate">{file.fileName}</span>
                            <span className="text-[11px] text-slate-400">
                              {(file.fileSize / 1024).toFixed(1)} KB • Uploaded by {file.uploadedByUserName}
                            </span>
                          </div>
                        </div>
                        <div className="flex items-center gap-2 shrink-0">
                          <button
                            onClick={() => handleDownloadAttachment(file.id, file.fileName)}
                            className="p-1.5 text-slate-600 hover:text-blue-600 hover:bg-slate-200 rounded transition"
                            title="Download"
                          >
                            <Download className="w-4 h-4" />
                          </button>
                          {(isAdmin || file.uploadedByUserId === user?.id) && (
                            <button
                              onClick={() => handleDeleteAttachment(file.id)}
                              className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded transition"
                              title="Delete"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          )}
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}

            {/* Tab 3: Audit Timeline */}
            {activeTab === 'timeline' && (
              <div className="p-6">
                <div className="relative pl-6 border-l-2 border-slate-200 space-y-6">
                  {ticket.histories?.map((h) => (
                    <div key={h.id} className="relative">
                      <div className="absolute -left-[31px] top-0.5 w-3 h-3 bg-blue-600 rounded-full border-2 border-white" />
                      <div className="text-xs space-y-0.5">
                        <span className="font-semibold text-slate-800">
                          {h.action} by {h.changedByUserName} ({h.changedByUserRole})
                        </span>
                        {h.oldValue && h.newValue && (
                          <p className="text-slate-500 text-[11px]">
                            Changed from <span className="font-mono bg-slate-100 px-1 rounded">{h.oldValue}</span> to{' '}
                            <span className="font-mono bg-blue-50 text-blue-700 px-1 rounded">{h.newValue}</span>
                          </p>
                        )}
                        <span className="text-[10px] text-slate-400 block">{new Date(h.createdAt).toLocaleString()}</span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Right Column: Ticket Metadata & Management Controls */}
        <div className="space-y-6">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm space-y-4">
            <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider pb-2 border-b border-slate-100">
              Ticket Overview
            </h3>

            <div>
              <span className="text-xs text-slate-400 block mb-1">Workflow Status</span>
              <StatusBadge status={ticket.status} />
            </div>

            <div>
              <span className="text-xs text-slate-400 block mb-1">Urgency / Priority</span>
              <PriorityBadge priority={ticket.priority} />
            </div>

            <div>
              <span className="text-xs text-slate-400 block mb-1">Category</span>
              <span className="text-xs font-medium text-slate-800">{ticket.categoryName}</span>
            </div>

            <div>
              <span className="text-xs text-slate-400 block mb-1">Target Resolution Deadline (SLA)</span>
              <span className="text-xs font-medium text-slate-800">
                {ticket.dueAt ? new Date(ticket.dueAt).toLocaleString() : 'Not Set'}
              </span>
            </div>

            <div>
              <span className="text-xs text-slate-400 block mb-1">Assigned Administrator</span>
              <span className="text-xs font-semibold text-slate-800">
                {ticket.assignedToUserName || 'Unassigned'}
              </span>
            </div>
          </div>

          {/* Administrator Operations Control Panel */}
          {isAdmin && (
            <div className="bg-white p-5 rounded-xl border border-indigo-200 shadow-sm space-y-4">
              <h3 className="text-xs font-bold text-indigo-900 uppercase tracking-wider flex items-center gap-1.5 pb-2 border-b border-indigo-100">
                <UserCheck className="w-3.5 h-3.5 text-indigo-600" />
                Admin Operations
              </h3>

              {/* Assignment Control */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Assign Admin Specialist</label>
                <select
                  disabled={updating}
                  value={ticket.assignedToUserId || ''}
                  onChange={(e) => handleAssignAdmin(e.target.value)}
                  className="w-full border border-slate-300 rounded-lg px-3 py-2 text-xs bg-white focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="">Unassigned</option>
                  {admins.map((adm) => (
                    <option key={adm.id} value={adm.id}>
                      {adm.name} ({adm.email})
                    </option>
                  ))}
                </select>
              </div>

              {/* Status Update Control */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">Update Ticket Status</label>
                <div className="grid grid-cols-2 gap-2">
                  <button
                    disabled={updating || ticket.status === 'Open'}
                    onClick={() => handleStatusChange('Open')}
                    className="px-2.5 py-1.5 border rounded-lg text-xs font-semibold text-blue-700 border-blue-200 hover:bg-blue-50 disabled:opacity-40 transition"
                  >
                    Open
                  </button>
                  <button
                    disabled={updating || ticket.status === 'InProgress'}
                    onClick={() => handleStatusChange('InProgress')}
                    className="px-2.5 py-1.5 border rounded-lg text-xs font-semibold text-indigo-700 border-indigo-200 hover:bg-indigo-50 disabled:opacity-40 transition"
                  >
                    In Progress
                  </button>
                  <button
                    disabled={updating || ticket.status === 'Resolved'}
                    onClick={() => handleStatusChange('Resolved')}
                    className="px-2.5 py-1.5 border rounded-lg text-xs font-semibold text-emerald-700 border-emerald-200 hover:bg-emerald-50 disabled:opacity-40 transition"
                  >
                    Resolved
                  </button>
                  <button
                    disabled={updating || ticket.status === 'Closed'}
                    onClick={() => handleStatusChange('Closed')}
                    className="px-2.5 py-1.5 border rounded-lg text-xs font-semibold text-slate-700 border-slate-300 hover:bg-slate-50 disabled:opacity-40 transition"
                  >
                    Closed
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Employee Close / Reopen Control */}
          {!isAdmin && ticket.createdByUserId === user?.id && (
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm space-y-3">
              <h3 className="text-xs font-bold text-slate-700 uppercase tracking-wider">Ticket Confirmation</h3>
              {ticket.status === 'Resolved' && (
                <div className="space-y-2">
                  <p className="text-xs text-slate-500">Your issue was resolved by IT. Please verify the fix.</p>
                  <button
                    disabled={updating}
                    onClick={() => handleStatusChange('Closed')}
                    className="w-full py-2 px-3 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-semibold transition"
                  >
                    Accept Resolution & Close Ticket
                  </button>
                  <button
                    disabled={updating}
                    onClick={() => handleStatusChange('InProgress')}
                    className="w-full py-2 px-3 bg-rose-50 text-rose-700 border border-rose-200 hover:bg-rose-100 rounded-lg text-xs font-semibold transition"
                  >
                    Issue Persists (Reopen)
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
