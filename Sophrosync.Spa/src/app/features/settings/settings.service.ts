import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationPreferenceDto, PracticeTargets, ProfileDto } from './settings.model';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly identityBase = `${environment.apiUrl}/identity`;
  private readonly prefsBase = `${environment.apiUrl}/notification-preferences`;

  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>(`${this.identityBase}/profile`);
  }

  updateProfile(firstName: string, lastName: string): Observable<ProfileDto> {
    return this.http.put<ProfileDto>(`${this.identityBase}/profile`, { firstName, lastName });
  }

  getPreferences(): Observable<NotificationPreferenceDto> {
    return this.http.get<NotificationPreferenceDto>(this.prefsBase);
  }

  updatePreferences(prefs: NotificationPreferenceDto): Observable<void> {
    return this.http.put<void>(this.prefsBase, prefs);
  }

  getPracticeTargets(): Observable<PracticeTargets> {
    return this.http.get<PracticeTargets>(`${this.identityBase}/practice-settings`);
  }

  savePracticeTargets(targets: PracticeTargets): Observable<void> {
    return this.http.put<void>(`${this.identityBase}/practice-settings`, targets);
  }
}
