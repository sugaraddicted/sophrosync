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
}
