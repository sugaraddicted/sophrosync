import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError, timer } from 'rxjs';
import { retry } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Client, ClientDto } from './models/client.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clients`;

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.base).pipe(
      retry({
        count: 1,
        delay: (err) =>
          err instanceof HttpErrorResponse && err.status >= 400 && err.status < 500
            ? throwError(() => err)
            : timer(1000),
      })
    );
  }

  createClient(dto: ClientDto): Observable<Client> {
    return this.http.post<Client>(this.base, dto);
  }

  updateClient(id: string, dto: ClientDto): Observable<Client> {
    return this.http.put<Client>(`${this.base}/${id}`, dto);
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
