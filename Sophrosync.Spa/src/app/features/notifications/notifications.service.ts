import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { NotificationDto, PaginatedList, UnreadCountResponse } from './notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/notifications`;

  getUnreadCount(): Observable<number> {
    return this.http
      .get<UnreadCountResponse>(`${this.base}/unread-count`)
      .pipe(map((response) => response.Count));
  }

  getInbox(page = 1, pageSize = 20): Observable<NotificationDto[]> {
    return this.http
      .get<PaginatedList<NotificationDto>>(`${this.base}/inbox`, {
        params: { page: page.toString(), pageSize: pageSize.toString() },
      })
      .pipe(map((response) => response.items));
  }

  dismiss(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/dismiss`, null);
  }
}
