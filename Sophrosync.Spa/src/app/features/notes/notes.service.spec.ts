import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { NotesService } from './notes.service';
import { environment } from '../../../environments/environment';
import { Note } from './models/note.model';

const BASE = `${environment.apiUrl}/notes`;

describe('NotesService', () => {
  let service: NotesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), NotesService],
    });
    service = TestBed.inject(NotesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  describe('getNotes retry behavior', () => {
    beforeEach(() => vi.useFakeTimers());
    afterEach(() => vi.useRealTimers());

    it('retries once on server error (5xx) and returns result on second attempt', async () => {
      let result: Note[] | undefined;
      service.getNotes().subscribe(notes => (result = notes));

      // First attempt fails with 500
      http.expectOne(BASE).flush(null, { status: 500, statusText: 'Server Error' });

      // Advance past the 1-second retry delay
      await vi.advanceTimersByTimeAsync(1000);

      // Retry fires — flush the second request
      http.expectOne(BASE).flush([{ id: '1', title: 'Test Note' }]);

      expect(result).toHaveLength(1);
    });

    it('does not retry on client error (404) — propagates immediately', async () => {
      let error: unknown;
      service.getNotes().subscribe({ error: e => (error = e) });

      http.expectOne(BASE).flush(null, { status: 404, statusText: 'Not Found' });
      await vi.advanceTimersByTimeAsync(1000);

      // No second request should be pending
      http.expectNone(BASE);
      expect(error).toBeDefined();
    });

    it('does not retry on client error (401)', async () => {
      let error: unknown;
      service.getNotes().subscribe({ error: e => (error = e) });

      http.expectOne(BASE).flush(null, { status: 401, statusText: 'Unauthorized' });
      await vi.advanceTimersByTimeAsync(1000);

      http.expectNone(BASE);
      expect(error).toBeDefined();
    });
  });

  describe('getNoteById', () => {
    it('fetches the note by id', () => {
      const id = 'abc-123';
      let result: Note | undefined;
      service.getNoteById(id).subscribe(n => (result = n));

      http.expectOne(`${BASE}/${id}`).flush({ id, title: 'Note' });

      expect(result).toBeDefined();
    });
  });

  describe('getNotesByClientId', () => {
    it('fetches notes for the given client', () => {
      const clientId = 'client-1';
      let result: Note[] | undefined;
      service.getNotesByClientId(clientId).subscribe(notes => (result = notes));

      http.expectOne(`${BASE}/client/${clientId}`).flush([{ id: '1' }, { id: '2' }]);

      expect(result).toHaveLength(2);
    });
  });

  describe('createNote', () => {
    it('POST to base URL with dto', () => {
      const dto = { clientId: 'c1', sessionDate: '2026-05-18', type: 'DAP' as const, title: 'T', content: 'C' };
      service.createNote(dto).subscribe();

      const req = http.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush({ id: 'new' });
    });
  });

  describe('updateNote', () => {
    it('PUT to note URL with dto', () => {
      const id = 'n1';
      const dto = { title: 'Updated', content: 'New content' };
      service.updateNote(id, dto).subscribe();

      const req = http.expectOne(`${BASE}/${id}`);
      expect(req.request.method).toBe('PUT');
      req.flush({ id });
    });
  });

  describe('signNote', () => {
    it('POST to sign endpoint', () => {
      service.signNote('n1').subscribe();

      const req = http.expectOne(`${BASE}/n1/sign`);
      expect(req.request.method).toBe('POST');
      req.flush({ id: 'n1' });
    });
  });

  describe('deleteNote', () => {
    it('DELETE to note URL', () => {
      service.deleteNote('n1').subscribe();

      const req = http.expectOne(`${BASE}/n1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
