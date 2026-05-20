import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { retry } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Client, ClientDto } from './models/client.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clients`;

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.base).pipe(retry(1));
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
