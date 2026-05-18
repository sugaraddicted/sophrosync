import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ClientsService } from './clients.service';
import { environment } from '../../../environments/environment';
import { Client } from './models/client.model';

const BASE = `${environment.apiUrl}/clients`;

describe('ClientsService', () => {
  let service: ClientsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), ClientsService],
    });
    service = TestBed.inject(ClientsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  describe('getClients retry behavior', () => {
    beforeEach(() => vi.useFakeTimers());
    afterEach(() => vi.useRealTimers());

    it('retries once on server error (5xx) and returns result on second attempt', async () => {
      let result: Client[] | undefined;
      service.getClients().subscribe(clients => (result = clients));

      // First attempt fails with 500
      http.expectOne(BASE).flush(null, { status: 500, statusText: 'Server Error' });

      // Advance past the 1-second retry delay
      await vi.advanceTimersByTimeAsync(1000);

      // Retry fires — flush the second request
      http.expectOne(BASE).flush([
        { id: '1', name: 'Alice', email: 'a@b.com', phone: '123', status: 'active' },
      ]);

      expect(result).toHaveLength(1);
    });

    it('does not retry on client error (404)', async () => {
      let error: unknown;
      service.getClients().subscribe({ error: e => (error = e) });

      http.expectOne(BASE).flush(null, { status: 404, statusText: 'Not Found' });
      await vi.advanceTimersByTimeAsync(1000);

      http.expectNone(BASE);
      expect(error).toBeDefined();
    });

    it('does not retry on 403 Forbidden', async () => {
      let error: unknown;
      service.getClients().subscribe({ error: e => (error = e) });

      http.expectOne(BASE).flush(null, { status: 403, statusText: 'Forbidden' });
      await vi.advanceTimersByTimeAsync(1000);

      http.expectNone(BASE);
      expect(error).toBeDefined();
    });

    it('does not retry on 401 Unauthorized', async () => {
      let error: unknown;
      service.getClients().subscribe({ error: e => (error = e) });

      http.expectOne(BASE).flush(null, { status: 401, statusText: 'Unauthorized' });
      await vi.advanceTimersByTimeAsync(1000);

      http.expectNone(BASE);
      expect(error).toBeDefined();
    });
  });

  describe('createClient', () => {
    it('POST to base URL with dto', () => {
      const dto = { name: 'Bob', email: 'b@c.com', phone: '456', status: 'active' as const };
      service.createClient(dto).subscribe();

      const req = http.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      req.flush({ id: 'new', ...dto });
    });
  });

  describe('updateClient', () => {
    it('PUT to client URL with dto', () => {
      const id = 'c1';
      const dto = { name: 'Bob Updated', email: 'b@c.com', phone: '789', status: 'inactive' as const };
      service.updateClient(id, dto).subscribe();

      const req = http.expectOne(`${BASE}/${id}`);
      expect(req.request.method).toBe('PUT');
      req.flush({ id, ...dto });
    });
  });

  describe('deleteClient', () => {
    it('DELETE to client URL', () => {
      service.deleteClient('c1').subscribe();

      const req = http.expectOne(`${BASE}/c1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
