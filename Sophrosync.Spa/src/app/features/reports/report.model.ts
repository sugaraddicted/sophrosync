export interface AppointmentSummaryDto {
  totalScheduled: number;
  totalCompleted: number;
  totalCancelled: number;
  totalNoShow: number;
  periodStart: string;
  periodEnd: string;
}

export interface NoteCompletionRateDto {
  totalAppointments: number;
  notesCreated: number;
  notesSigned: number;
  notesOverdue: number;
  completionRate: number;
  periodStart: string;
  periodEnd: string;
}

export interface PracticeAnalyticsSummaryDto {
  tenantId: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  cancellationRate: number;
  noShowRate: number;
  newClientsOnboarded: number;
  activeTherapists: number;
  periodStart: string;
  periodEnd: string;
}
