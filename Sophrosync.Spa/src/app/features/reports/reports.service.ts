import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppointmentSummaryDto,
  NoteCompletionRateDto,
  PracticeAnalyticsSummaryDto,
} from './report.model';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reports`;

  getAppointmentSummary(from: Date, to: Date): Observable<AppointmentSummaryDto> {
    const params = this.dateParams(from, to);
    return this.http.get<AppointmentSummaryDto>(`${this.base}/appointment-summary`, { params });
  }

  getNoteCompletionRate(from: Date, to: Date): Observable<NoteCompletionRateDto> {
    const params = this.dateParams(from, to);
    return this.http.get<NoteCompletionRateDto>(`${this.base}/note-completion-rate`, { params });
  }

  getPracticeAnalytics(from: Date, to: Date): Observable<PracticeAnalyticsSummaryDto> {
    const params = this.dateParams(from, to);
    return this.http.get<PracticeAnalyticsSummaryDto>(`${this.base}/practice-analytics`, { params });
  }

  private dateParams(from: Date, to: Date): HttpParams {
    return new HttpParams()
      .set('from', from.toISOString())
      .set('to', to.toISOString());
  }
}
