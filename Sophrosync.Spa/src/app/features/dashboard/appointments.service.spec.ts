import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AppointmentsService, AppointmentDto } from './appointments.service';

describe('AppointmentsService', () => {
  let service: AppointmentsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppointmentsService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AppointmentsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('sends GET to /api/appointments/range with from and to params', () => {
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    service.getByDateRange(from, to);
    const req = httpMock.expectOne(r =>
      r.url.includes('/appointments/range') &&
      r.params.get('from') === from.toISOString() &&
      r.params.get('to')   === to.toISOString()
    );
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('returns mapped DTO array from HTTP response', async () => {
    const dto: AppointmentDto = {
      id: '1', tenantId: 't1', clientId: 'c1', therapistId: 'th1',
      scheduledAt: '2026-05-15T09:00:00Z', durationMinutes: 60,
      type: 'InPerson', status: 'Scheduled',
    };
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    const promise = service.getByDateRange(from, to);
    httpMock.expectOne(() => true).flush([dto]);
    const result = await promise;
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('1');
    expect(result[0].type).toBe('InPerson');
  });

  it('rejects when HTTP returns error', async () => {
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    const promise = service.getByDateRange(from, to);
    httpMock.expectOne(() => true).flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    await expect(promise).rejects.toBeDefined();
  });
});
