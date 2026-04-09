export type NoteStatus = 'Draft' | 'PendingCoSign' | 'Signed' | 'Locked' | 'Amended';
export type NoteType = 'DAP' | 'SOAP' | 'FreeForm' | 'Intake' | 'Treatment' | 'Discharge';

export const NOTE_TYPE_LABELS: Record<NoteType, string> = {
  SOAP: 'Progress Note — SOAP',
  DAP: 'Progress Note — DAP',
  FreeForm: 'Progress Note — Narrative',
  Intake: 'Intake Assessment',
  Treatment: 'Treatment Plan',
  Discharge: 'Discharge Summary',
};

export const NOTE_TYPES: NoteType[] = ['SOAP', 'DAP', 'FreeForm', 'Intake', 'Treatment', 'Discharge'];

export interface NoteDto {
  id: string;
  clientId: string;
  appointmentId?: string;
  therapistId: string;
  authorFullName: string;
  type: NoteType;
  title?: string;
  content: string;
  tags?: string;
  status: NoteStatus;
  signedAt?: string;
  signedByFullName?: string;
  lockedAt?: string;
  lockedByFullName?: string;
  amendedFromId?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNoteRequest {
  clientId: string;
  appointmentId?: string;
  type: NoteType;
  title?: string;
  content: string;
  tags?: string;
}

export interface UpdateNoteRequest {
  title?: string;
  content: string;
  tags?: string;
}
