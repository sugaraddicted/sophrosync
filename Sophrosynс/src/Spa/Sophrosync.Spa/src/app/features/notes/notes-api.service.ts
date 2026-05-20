import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NoteDto, CreateNoteRequest, UpdateNoteRequest } from './notes.model';

const BASE = '/api/notes';

@Injectable({ providedIn: 'root' })
export class NotesApiService {
  private http = inject(HttpClient);

  getAll(): Observable<NoteDto[]> {
    return this.http.get<NoteDto[]>(BASE);
  }

  getById(id: string): Observable<NoteDto> {
    return this.http.get<NoteDto>(`${BASE}/${id}`);
  }

  getByClient(clientId: string): Observable<NoteDto[]> {
    return this.http.get<NoteDto[]>(`${BASE}/client/${clientId}`);
  }

  create(req: CreateNoteRequest): Observable<NoteDto> {
    return this.http.post<NoteDto>(BASE, req);
  }

  update(id: string, req: UpdateNoteRequest): Observable<NoteDto> {
    return this.http.put<NoteDto>(`${BASE}/${id}`, req);
  }

  sign(id: string): Observable<NoteDto> {
    return this.http.post<NoteDto>(`${BASE}/${id}/sign`, {});
  }

  lock(id: string): Observable<NoteDto> {
    return this.http.post<NoteDto>(`${BASE}/${id}/lock`, {});
  }

  requestCoSign(id: string): Observable<NoteDto> {
    return this.http.post<NoteDto>(`${BASE}/${id}/request-cosign`, {});
  }

  amend(id: string, req: UpdateNoteRequest): Observable<NoteDto> {
    return this.http.post<NoteDto>(`${BASE}/${id}/amend`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${BASE}/${id}`);
  }
}
