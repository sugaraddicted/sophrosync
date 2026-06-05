import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ClientsService } from '../clients.service';
import { Client } from '../models/client.model';
import { NotesService } from '../../notes/notes.service';
import { Note } from '../../notes/models/note.model';
import { AppointmentsService, AppointmentDto } from '../../dashboard/appointments.service';
import { ConsentService } from '../../consent/consent.service';
import { ConsentRequestDto } from '../../consent/consent.model';

interface ClientDetailData {
  client: Client;
  notes: Note[];
  appointments: AppointmentDto[];
  pendingConsent: ConsentRequestDto[];
}

@Component({
  selector: 'app-client-detail-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './client-detail-page.component.html',
  styleUrl: './client-detail-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClientDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly clientsService = inject(ClientsService);
  private readonly notesService = inject(NotesService);
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly consentService = inject(ConsentService);

  protected readonly isLoading = signal(true);
  protected readonly loadError = signal<string | null>(null);
  protected readonly data = signal<ClientDetailData | null>(null);

  protected readonly recentNotes = computed(() =>
    (this.data()?.notes ?? [])
      .slice()
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
      .slice(0, 5)
  );

  protected readonly upcomingAppointments = computed(() => {
    const now = new Date();
    return (this.data()?.appointments ?? [])
      .filter(a => new Date(a.scheduledAt) >= now)
      .sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime())
      .slice(0, 3);
  });

  protected readonly pastAppointments = computed(() => {
    const now = new Date();
    return (this.data()?.appointments ?? [])
      .filter(a => new Date(a.scheduledAt) < now)
      .sort((a, b) => new Date(b.scheduledAt).getTime() - new Date(a.scheduledAt).getTime());
  });

  protected readonly totalSessions = computed(() => this.pastAppointments().length);

  protected readonly lastSessionDate = computed(() => {
    const past = this.pastAppointments();
    return past.length > 0 ? past[0].scheduledAt : null;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/clients']);
      return;
    }
    this.loadAll(id);
  }

  protected loadAll(id: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    forkJoin({
      client: this.clientsService.getClientById(id),
      notes: this.notesService.getNotesByClientId(id),
      pendingConsent: this.consentService.getPendingRequests(id),
    }).subscribe({
      next: ({ client, notes, pendingConsent }) => {
        this.appointmentsService.getByClientId(id).then(appointments => {
          this.data.set({ client, notes, appointments, pendingConsent });
          this.isLoading.set(false);
        }).catch(() => {
          // appointments failed — still show the rest with empty appointments
          this.data.set({ client, notes, appointments: [], pendingConsent });
          this.isLoading.set(false);
        });
      },
      error: () => {
        this.loadError.set('Failed to load client profile. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  protected retry(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.loadAll(id);
  }

  protected goBack(): void {
    this.router.navigate(['/clients']);
  }

  protected formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    });
  }

  protected formatDateTime(iso: string): string {
    return new Date(iso).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  protected clientInitials(name: string): string {
    return name.substring(0, 2).toUpperCase();
  }
}
