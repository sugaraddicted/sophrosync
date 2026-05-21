import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppointmentDto {
  id: string;
  tenantId: string;
  clientId: string;
  therapistId: string;
  scheduledAt: string;       // ISO 8601
  durationMinutes: number;
  type: string;              // "InPerson" | "Video" | "Phone"
  status: string;
  notes?: string;
  cancellationReason?: string;
  createdAt: string;
}

export interface CreateAppointmentDto {
  clientId: string;
  therapistId: string;
  scheduledAt: string;       // ISO 8601
  durationMinutes: number;
  type: 'InPerson' | 'Video' | 'Phone';
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentsService {
  private readonly http = inject(HttpClient);

  getByDateRange(from: Date, to: Date): Promise<AppointmentDto[]> {
    const params = new HttpParams()
      .set('from', from.toISOString())
      .set('to', to.toISOString());
    return firstValueFrom(
      this.http.get<AppointmentDto[]>(`${environment.apiUrl}/appointments/range`, { params })
    );
  }

  createAppointment(dto: CreateAppointmentDto): Promise<AppointmentDto> {
    return firstValueFrom(
      this.http.post<AppointmentDto>(`${environment.apiUrl}/appointments`, dto)
    );
  }

  confirmAppointment(id: string): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`${environment.apiUrl}/appointments/${id}/confirm`, null)
    );
  }

  cancelAppointment(id: string, reason: string): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`${environment.apiUrl}/appointments/${id}/cancel`, { reason })
    );
  }

  rescheduleAppointment(id: string, newScheduledAt: string, newDurationMinutes?: number): Promise<void> {
    const body: Record<string, unknown> = { newScheduledAt };
    if (newDurationMinutes !== undefined) body['newDurationMinutes'] = newDurationMinutes;
    return firstValueFrom(
      this.http.post<void>(`${environment.apiUrl}/appointments/${id}/reschedule`, body)
    );
  }
}
