import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ConsentTemplateDto,
  ConsentRequestDto,
  ConsentRecordDto,
  IssueConsentRequestPayload,
} from './consent.model';

@Injectable({ providedIn: 'root' })
export class ConsentService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  listTemplates(): Observable<ConsentTemplateDto[]> {
    return this.http.get<ConsentTemplateDto[]>(`${this.base}/consent-templates`);
  }

  issueRequest(payload: IssueConsentRequestPayload): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/consent-requests`, payload);
  }

  getPendingRequests(clientId: string): Observable<ConsentRequestDto[]> {
    return this.http.get<ConsentRequestDto[]>(
      `${this.base}/consent-requests/client/${clientId}/pending`
    );
  }

  getConsentHistory(clientId: string): Observable<ConsentRecordDto[]> {
    return this.http.get<ConsentRecordDto[]>(
      `${this.base}/consent-requests/client/${clientId}/history`
    );
  }

  revokeRequest(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/consent-requests/${id}/revoke`, {});
  }
}
