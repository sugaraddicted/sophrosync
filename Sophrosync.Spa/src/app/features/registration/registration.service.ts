import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  practiceName: string;
  timeZone: string;
  acceptedTerms: boolean;
}

@Injectable({ providedIn: 'root' })
export class RegistrationService {
  private readonly http = inject(HttpClient);

  registerPractice(request: RegisterRequest): Observable<void> {
    return this.http.post<void>('/api/identity/register', request);
  }
}
