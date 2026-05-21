import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { retry } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Client, ClientDto as ClientWriteDto } from './models/client.model';

export interface ClientDto {
  id: string;
  name: string;
  email: string;
  phone: string;
  status: 'Active' | 'Inactive';
}

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/clients`;

  getAll(): Promise<ClientDto[]> {
    return firstValueFrom(this.http.get<ClientDto[]>(this.base));
  }

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.base).pipe(retry(1));
  }

  createClient(dto: ClientWriteDto): Observable<Client> {
    return this.http.post<Client>(this.base, dto);
  }

  updateClient(id: string, dto: ClientWriteDto): Observable<Client> {
    return this.http.put<Client>(`${this.base}/${id}`, dto);
  }

  deleteClient(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
