import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Note, CreateNoteDto, UpdateNoteDto } from './models/note.model';

@Injectable({ providedIn: 'root' })
export class NotesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/notes`;

  getNotes(): Observable<Note[]> {
    return this.http.get<Note[]>(this.base).pipe(retry(1));
  }

  getNoteById(id: string): Observable<Note> {
    return this.http.get<Note>(`${this.base}/${id}`);
  }

  getNotesByClientId(clientId: string): Observable<Note[]> {
    return this.http.get<Note[]>(`${this.base}/client/${clientId}`);
  }

  createNote(dto: CreateNoteDto): Observable<Note> {
    return this.http.post<Note>(this.base, dto);
  }

  updateNote(id: string, dto: UpdateNoteDto): Observable<Note> {
    return this.http.put<Note>(`${this.base}/${id}`, dto);
  }

  signNote(id: string): Observable<Note> {
    return this.http.post<Note>(`${this.base}/${id}/sign`, {});
  }

  lockNote(id: string): Observable<Note> {
    return this.http.post<Note>(`${this.base}/${id}/lock`, {});
  }

  requestCoSign(id: string): Observable<Note> {
    return this.http.post<Note>(`${this.base}/${id}/request-cosign`, {});
  }

  amendNote(id: string, dto: UpdateNoteDto): Observable<Note> {
    return this.http.post<Note>(`${this.base}/${id}/amend`, dto);
  }

  deleteNote(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getNoteByAppointmentId(appointmentId: string): Observable<Note | null> {
    return this.http.get<Note>(`${this.base}/by-appointment/${appointmentId}`).pipe(
      catchError((err: HttpErrorResponse) =>
        err.status === 404 ? of(null) : throwError(() => err)
      )
    );
  }
}
