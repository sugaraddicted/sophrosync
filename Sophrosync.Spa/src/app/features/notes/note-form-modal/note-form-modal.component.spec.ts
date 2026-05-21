import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { NoteFormModalComponent } from './note-form-modal.component';
import { ClientsService, ClientDto } from '../../clients/clients.service';
import { Note } from '../models/note.model';

const makeClient = (n: number): ClientDto => ({
  id: `client-${n}`,
  name: `Client ${n}`,
  email: `client${n}@example.com`,
  phone: `000-000-000${n}`,
  status: 'Active',
});

const makeNote = (): Note => ({
  id: 'note-1',
  clientId: 'client-1',
  therapistId: 'therapist-1',
  type: 'SOAP',
  title: 'Existing title',
  content: 'Existing content',
  tags: 'tag1',
  status: 'Draft',
  createdAt: '2026-05-01T10:00:00Z',
  updatedAt: '2026-05-01T10:00:00Z',
  sessionDate: '2026-05-01T00:00:00Z',
  authorFullName: 'Dr. Smith',
  signedAt: null,
  signedByFullName: null,
  lockedAt: null,
  lockedByFullName: null,
  amendedFromId: null,
  isDeleted: false,
});

describe('NoteFormModalComponent', () => {
  let mockClientsService: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockClientsService = { getAll: vi.fn().mockResolvedValue([]) };

    await TestBed.configureTestingModule({
      imports: [NoteFormModalComponent],
      providers: [
        { provide: ClientsService, useValue: mockClientsService },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  it('creates the component', async () => {
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('calls ClientsService.getAll() on init in create mode', async () => {
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(mockClientsService.getAll).toHaveBeenCalledOnce();
  });

  it('populates clients signal after successful load', async () => {
    mockClientsService.getAll.mockResolvedValue([makeClient(1), makeClient(2)]);
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.clients()).toHaveLength(2);
    expect(fixture.componentInstance.clients()[0].name).toBe('Client 1');
  });

  it('sets clientsError signal on service failure', async () => {
    mockClientsService.getAll.mockRejectedValue(new Error('network error'));
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).not.toBeNull();
    expect(fixture.componentInstance.clients()).toHaveLength(0);
  });

  it('clears error and reloads on loadClients() retry', async () => {
    mockClientsService.getAll
      .mockRejectedValueOnce(new Error('network error'))
      .mockResolvedValue([makeClient(1)]);
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).not.toBeNull();

    fixture.componentInstance.loadClients();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).toBeNull();
    expect(fixture.componentInstance.clients()).toHaveLength(1);
  });

  it('does NOT call getAll() in edit mode', async () => {
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.componentRef.setInput('note', makeNote());
    fixture.detectChanges();
    await fixture.whenStable();
    expect(mockClientsService.getAll).not.toHaveBeenCalled();
  });

  it('form is invalid when clientId is empty', () => {
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.form.markAllAsTouched();
    expect(component.form.controls.clientId.invalid).toBe(true);
    expect(component.form.invalid).toBe(true);
  });

  it('emits submitted with correct create dto on valid form', async () => {
    mockClientsService.getAll.mockResolvedValue([makeClient(1)]);
    const fixture = TestBed.createComponent(NoteFormModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    const submittedValues: unknown[] = [];
    component.submitted.subscribe((v) => submittedValues.push(v));

    component.form.setValue({
      clientId: 'client-1',
      sessionDate: '2026-05-15',
      type: 'DAP',
      title: 'Test Note',
      content: 'Some meaningful content here.',
      tags: 'anxiety',
    });

    component.onSubmit();

    expect(submittedValues).toHaveLength(1);
    const result = submittedValues[0] as { mode: string; dto: Record<string, unknown> };
    expect(result.mode).toBe('create');
    expect(result.dto['clientId']).toBe('client-1');
    expect(result.dto['sessionDate']).toBe('2026-05-15');
    expect(result.dto['type']).toBe('DAP');
    expect(result.dto['title']).toBe('Test Note');
    expect(result.dto['content']).toBe('Some meaningful content here.');
    expect(result.dto['tags']).toBe('anxiety');
  });
});
