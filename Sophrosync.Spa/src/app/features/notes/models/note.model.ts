export type NoteType = 'DAP' | 'SOAP' | 'FreeForm' | 'Intake' | 'Treatment' | 'Discharge';
export type NoteStatus = 'Draft' | 'PendingCoSign' | 'Signed' | 'Locked' | 'Amended';

export interface Note {
  id: string;
  clientId: string;
  therapistId: string;
  type: NoteType;
  title: string;
  content: string;
  tags: string;
  status: NoteStatus;
  createdAt: string;
  updatedAt: string;
  sessionDate: string;
  authorFullName: string;
  signedAt: string | null;
  signedByFullName: string | null;
  lockedAt: string | null;
  lockedByFullName: string | null;
  amendedFromId: string | null;
  isDeleted: boolean;
}

export interface CreateNoteDto {
  clientId: string;
  appointmentId?: string;
  type: NoteType;
  title: string;
  content: string;
  tags?: string;
  sessionDate: string;
}

export interface UpdateNoteDto {
  title: string;
  content: string;
  tags?: string;
}
